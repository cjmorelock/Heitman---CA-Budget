using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public class UnitOfWork : IUnitOfWork {
        // TO DO: may need to add additional connection strings here somewhere for Account and Vendor lookups
        public readonly string ConnectionString;
        public UnitOfWork () {
            ConnectionString = ConfigurationManager.ConnectionStrings["SLConnection"].ConnectionString;
        }

        private IBudgetLineRepository _blr;
        public IBudgetLineRepository BudgetLines => _blr ?? (_blr = new BudgetLineRepository(this));

        private IRepository<Account> _ar;
        public IRepository<Account> Accounts => _ar ?? (_ar = new AccountsRepository(this));

        private IRepository<Vendor> _vr;
        public IRepository<Vendor> Vendors => _vr ?? (_vr = new VendorsRepository(this));
    }
}
