using CABudget.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CABudget.ViewModels {
    public class BudgetLinesTable {
        public IEnumerable<BudgetLine> BudgetLines { get; set; }
        public List<string> Columns { get; set; }
    }
}