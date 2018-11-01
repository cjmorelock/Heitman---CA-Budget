using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    /// <summary>
    /// Use - 1) Create an instance of this class
    ///       2) For each BudgetLine to be validated call the IsValid method on this instance
    ///       ** The Year for all Budget Lines must match the Year of the first BudgetLine
    /// </summary>
    class YearValidRule : IBudgetLineValidationRule {
        // Year should always be in column 1 of the Excel sheets
        public int Column {
            get { return 1; }
        }
        private int? _previousBLYear = null;

        public bool IsValid(BudgetLine bl) {
            if (int.TryParse(bl.Year, out int y)
                && y >= 0
                && y < 10000) {

                if (_previousBLYear.HasValue) {
                    if (_previousBLYear.Value == y) {                        
                        return true;
                    } else {
                        // year for all budget lines must be the same!
                        bl.InvalidColumns.Add(Column);
                        bl.InvalidMessages.Add($"Year ({bl.Year ?? ""}) is invalid");
                    }
                } else {
                    _previousBLYear = y; // this is the first bl processed with a valid year; all other must match this year
                    return true;
                }
            } else {
                // year not between 0 and 9999
                bl.InvalidColumns.Add(Column);
                bl.InvalidMessages.Add($"Year ({bl.Year ?? ""}) is invalid");
            }

            return false;
        }
    }
}
