using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public interface IRepository<T> where T : class {
        List<T> GetList(Func<T, bool> where);
    }

    public interface IBudgetLineRepository : IRepository<BudgetLine> {
        void AddBulk(IEnumerable<BudgetLine> items);
        void DeleteAll();
    }
    
}

   
