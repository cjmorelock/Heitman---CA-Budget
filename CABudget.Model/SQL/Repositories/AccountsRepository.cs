using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    /// <summary>
    ///  Use this class to get a list of all expense and revenue Accounts against which accounts found in the Budget File should be validated
    /// </summary>
    public class AccountsRepository : GenericRepository<Account> {
        private readonly string _tableName = "Accounts";
        private readonly UnitOfWork _uow;
        public AccountsRepository(UnitOfWork uow) {
            _uow = uow;
        }
        
        public override List<Account> GetList(Func<Account, bool> where) {
            List<Account> accounts = new List<Account>();
            string cmdStr = $"select Child_ID as [Id], Name as [Name] from {_tableName} where Child_ID >= 50000 and Child_ID < 70000";
            
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                using (var cmd = new SqlCommand(cmdStr, conn)) {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            var item = Project(reader);
                            accounts.Add(item);
                        }
                    }
                }                    
            }

            return accounts;
        }
    }
}
