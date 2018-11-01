using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CABudget.Model;
using System.Transactions;

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
                conn.Open();

                var cmd = new SqlCommand(cmdStr, conn);
                var reader = cmd.ExecuteReader();

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
                var bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.BatchSize = 2000;
                bulkCopy.DestinationTableName = _tableName;
                bulkCopy.WriteToServer(dt);
            }
        }

        public void DeleteAll() {
            string cmdStr = $"delete from {_tableName}";
            
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                conn.Open();

                var cmd = new SqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();
            }
        }

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
