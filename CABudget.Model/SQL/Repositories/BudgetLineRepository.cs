using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CABudget.Model;
using System.Transactions;
using System.Configuration;

namespace CABudget.Model.SQL {
    public class BudgetLineRepository : GenericRepository<BudgetLine>, IBudgetLineRepository {
        private readonly string _tableName = "Budget_Locked_Scrubbing";
        Type _tableObjectType = typeof(Budget_Locked_Scrubbing);
        private readonly UnitOfWork _uow;

        public BudgetLineRepository(UnitOfWork uow) {
            _uow = uow;
        }

        public override List<BudgetLine> GetList(Func<BudgetLine, bool> where) {
            // Map Budget_Locked_Scrubbing rows to BudgetLine objects.
            // - should be 12:1 mapping (Budget line has 12 amounts--one for each month of the year)
            
            Dictionary<BudgetLine,BudgetLine> blMap = new Dictionary<BudgetLine, BudgetLine>(new EqualityComparer_map_BLS_to_BudgetLine());

            //string cmdStr = $"select * from {tableName}";
            string cmdStr = $"select Year, LedgerCurrency, Entity, Account, State, SubAcct_1, SubAcct_2, Vendor, Month, Period, Amount from {_tableName}";
                        
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                using (var cmd = new SqlCommand(cmdStr, conn)) {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            var blItem = Project(reader); // project reader to BudgetLine
                            int month = int.Parse(reader.Value<string>("Month")); // exception if not valid month
                            decimal amt = reader.GetDecimal(reader.GetOrdinal("Amount")); // exception if not valid value

                            if (blMap.TryGetValue(blItem, out BudgetLine bl)) {
                                bl.Amounts[month - 1] += amt; // There can be multiple budget lines that match on all non-month-amount columns
                                                              // -- we aggregate matching lines, hence the +=
                            } else {
                                blItem.Amounts[month - 1] = amt;
                                blMap.Add(blItem, blItem);
                            }
                        }
                    }
                }                
            }

            return blMap.Values.ToList();
        }

        public void AddBulk(IEnumerable<BudgetLine> items) {
            // Create DataTable to be used for bulk insert
            DataTable dt = new DataTable(_tableName);
            foreach (var p in _tableObjectType.GetProperties()) {
                dt.Columns.Add(p.Name, p.PropertyType);
            }

            // Fill DataTable
            foreach (var item in items) {
                var blsItems = ToBLS(item);
                foreach (var blsItem in blsItems) {
                    var row = dt.NewRow();
                    foreach (var p in _tableObjectType.GetProperties()) {
                        row[p.Name] = p.GetValue(blsItem) ?? DBNull.Value;
                    }
                    dt.Rows.Add(row);
                }
            }

            // Persist using SqlBulkCopy class
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                conn.Open();
                using (var bulkCopy = new SqlBulkCopy(conn)) {
                    bulkCopy.BatchSize = 2000;
                    bulkCopy.DestinationTableName = _tableName;
                    bulkCopy.WriteToServer(dt);
                }
            }
        }
        
        public void DeleteAll() {
            string cmdStr = $"delete from {_tableName}";
            
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                using (var cmd = new SqlCommand(cmdStr, conn)) {
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }                    
            }
        }

        // Call this method to commit uploaded budget info and refresh the SL cube
        public BudgetLineCommitResult CommitAll(string performedBy) {
            BudgetLineCommitResult result = new BudgetLineCommitResult();
            
            string cubeName = "SL";
            CAUser user = new ActiveDirectoryHelper().FindUserByUserName(performedBy);
            string email = user == null ? null : user.Email;
            CubeLockInfo lockInfo = new CubeLockInfo(cubeName, email);

            // only execute if we can get exclusive access to refresh the cube
            if (GetCubeLock(lockInfo)) {
                // run stored proc async and return the result
                try {
                    string connStr = _uow.ConnectionStringMSDB;
                    string cmdStr = ConfigurationManager.AppSettings["SP_START_JOB"]; // sp_start_job
                    string jobName = ConfigurationManager.AppSettings["JOB_NAME"];    // "SSAS - Refresh SL Cube - Budget Upload" (in PROD)

                    using (var conn = new SqlConnection(connStr)) {
                        using (var cmd = new SqlCommand(cmdStr, conn)) {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            if (!string.IsNullOrEmpty(jobName)) {
                                cmd.Parameters.AddWithValue("job_name", jobName);
                            }
                            conn.Open();

                            int count = cmd.ExecuteNonQuery();

                            result.Status = BLCommitStatus.SUCCESS;
                            result.Info = lockInfo;
                        }
                    }

                } catch (Exception ex) {
                    result.Status = BLCommitStatus.FAIL_ERROR;
                    result.Exception = ex;
                }
            } else {
                result.Status = BLCommitStatus.FAIL_CUBE_LOCKED;
                result.Info = lockInfo;
            }

            return result;
        }

        private bool GetCubeLock(CubeLockInfo lockInfo) {
            string connStr = _uow.ConnectionStringHI;
            string cmdStr = "select top 1 CubeName, CubeLock, UserName, StartDate, StopDate from HTM_SSASOnDemandInfo where CubeName = @cubeName";
            string previousUser = "";
            try {
                using (var conn = new SqlConnection(connStr)) {
                    using (var cmd = new SqlCommand(cmdStr, conn)) {
                        conn.Open();
                        cmd.Parameters.AddWithValue("cubeName", lockInfo.CubeName);
                        using (var reader = cmd.ExecuteReader()) {
                            reader.Read();
                            lockInfo.CubeLock = reader["CubeLock"] as string;
                            previousUser = reader["UserName"] as string;
                        }
                        if (lockInfo.CubeLock == "No") {
                            // okay, lock cube
                            string cmdStr2 = "update HTM_SSASOnDemandInfo set CubeLock='Yes', UserName=@email where CubeName=@cubeName";
                            using (var cmd2 = new SqlCommand(cmdStr2, conn)) {
                                cmd2.Parameters.AddWithValue("email", lockInfo.UserName);
                                cmd2.Parameters.AddWithValue("cubeName", lockInfo.CubeName);
                                int count = cmd2.ExecuteNonQuery();
                                if (count > 0) {
                                    // success
                                    lockInfo.CubeLock = "Yes";
                                    return true;
                                } else {
                                    // fail
                                    lockInfo.UserName = previousUser;
                                    return false;
                                }
                            }
                        } else {
                            // fail - CubeLock = "Yes", presumably the cube is already, currently being refreshed by previousUser
                            lockInfo.UserName = previousUser;
                            // to do: StartDate and StopDate
                            return false;
                        }                  
                    }
                }
            } catch (Exception ex) {
                lockInfo.Notes = $"An error occured getting or setting lock status for cube {lockInfo.CubeName}.\r\n {ex.Message} - {ex.StackTrace}";
                return false;
            }
        }

        // do not use - unlock is performed on the sql side!
        private void Unlock() { }

        // Maps 1 Budget Line to 12 rows in the SQL table (one for each month)
        public List<Budget_Locked_Scrubbing> ToBLS(BudgetLine bl) {
            List<Budget_Locked_Scrubbing> items = new List<Budget_Locked_Scrubbing>();
            for (int i=0; i<12; i++) {
                var item = new Budget_Locked_Scrubbing() {
                    Year = bl.Year,
                    LedgerCurrency = bl.LedgerCurrency,
                    Entity = bl.Entity,
                    Account = bl.Account,
                    State = bl.State,
                    subAcct_1 = bl.subAcct_1,
                    subAcct_2 = bl.subAcct_2,
                    Vendor = bl.Vendor,
                    Month = ((i < 9) ? "0" : "") + (i+1),
                    Period = new DateTime(int.Parse(bl.Year), i + 1, 1),
                    Amount = bl.Amounts[i]
                };
                items.Add(item);
            }
            return items;
        }
    }
}
