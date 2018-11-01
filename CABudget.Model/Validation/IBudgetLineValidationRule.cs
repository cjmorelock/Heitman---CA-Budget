using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    interface IBudgetLineValidationRule {
        int Column { get; }
        bool IsValid (BudgetLine bl);
    }
}
