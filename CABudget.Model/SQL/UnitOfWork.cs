using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public class UnitOfWork : IUnitOfWork {
        // TO DO: may need to add additional connection strings here somewhere for Account and Vendor lookups
        public readonly string ConnectionString;     // this is for the SL database
        public readonly string ConnectionStringHI;   // this is where the cube lock table lives
        public readonly string ConnectionStringMSDB; // this is where the sp_start_job stored proc lives
        public UnitOfWork () {
            ConnectionString = ConfigurationManager.ConnectionStrings["SLConnection"].ConnectionString;
            ConnectionStringHI = ConfigurationManager.ConnectionStrings["HIConnection"].ConnectionString;
            ConnectionStringMSDB = ConfigurationManager.ConnectionStrings["MSDBConnection"].ConnectionString;
        }

        private IBudgetLineRepository _blr;
        public IBudgetLineRepository BudgetLines => _blr ?? (_blr = new BudgetLineRepository(this));

        private IRepository<Account> _ar;
        public IRepository<Account> Accounts => _ar ?? (_ar = new AccountsRepository(this));

        private IRepository<Vendor> _vr;
        public IRepository<Vendor> Vendors => _vr ?? (_vr = new VendorsRepository(this));
    }
}
