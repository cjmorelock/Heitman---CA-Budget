using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    /// <summary>
    ///  Use this class to get a list of all Vendors against which Vendors found in the Budget File should be validated
    /// </summary>
    public class VendorsRepository : GenericRepository<Vendor> {
        private readonly string _tableName = "Vendor_XRef";
        private readonly UnitOfWork _uow;
        public VendorsRepository(UnitOfWork uow) {
            _uow = uow;
        }        

        public override List<Vendor> GetList(Func<Vendor, bool> where) {
            List<Vendor> vendors = new List<Vendor>();
            string cmdStr = $"select LTrim(RTrim(VendID)) as [Id], LTrim(RTrim(VendDesc)) as [Name] from {_tableName} where DB = 'SolomonApp'";
            
            using (var conn = new SqlConnection(_uow.ConnectionString)) {
                using (var cmd = new SqlCommand(cmdStr, conn)) {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            var item = Project(reader);
                            vendors.Add(item);
                        }
                    }
                }
            }

            vendors.Add(new Vendor { Id = "Budget", Name = "Budget" });
            return vendors;
        }
    }
}
