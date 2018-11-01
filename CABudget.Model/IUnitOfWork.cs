using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public interface IUnitOfWork {
        IBudgetLineRepository BudgetLines { get; }
        IRepository<Account> Accounts { get; }
        IRepository<Vendor> Vendors { get; }
    }
}
