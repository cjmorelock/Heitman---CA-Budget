using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public class AccountsRepository : GenericRepository<Account> {
        private readonly string _tableName = "??";
        private readonly UnitOfWork _uow;
        public AccountsRepository(UnitOfWork uow) {
            _uow = uow;
        }
        

        public override List<Account> GetList(Func<Account, bool> where) {
            List<Account> accounts = new List<Account>();

            string cmdStr = $"select Id as [Id], Name as [Name] from {_tableName}";


            //using (var conn = new SqlConnection(_uow.ConnectionString)) {
            //    conn.Open();

            //    var cmd = new SqlCommand(cmdStr, conn);
            //    var reader = cmd.ExecuteReader();

            //    while (reader.Read()) {
            //        var item = Project(reader);
            //        accounts.Add(item);
            //    }
            //}

            return accounts;
        }
    }
}
