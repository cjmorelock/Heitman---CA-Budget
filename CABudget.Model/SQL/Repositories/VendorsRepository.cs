using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public class VendorsRepository : GenericRepository<Vendor> {
        private readonly string _tableName = "??";
        private readonly UnitOfWork _uow;
        public VendorsRepository(UnitOfWork uow) {
            _uow = uow;
        }
        

        public override List<Vendor> GetList(Func<Vendor, bool> where) {
            List<Vendor> vendors = new List<Vendor>();

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

            return vendors;
        }
    }
}
