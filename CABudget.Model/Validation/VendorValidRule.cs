using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    /// <summary>
    /// Use - 1) Create an instance of this class passing in the set of valid Vendors
    ///       2) For each BudgetLine to be validated call the IsValid method on this instance
    /// </summary>
    class VendorValidRule : IBudgetLineValidationRule {
        // Vendor should always be in column 8 of the Excel sheets
        public int Column {
            get { return 8; }
        }
        protected readonly SortedSet<string> ValidVendors;

        public VendorValidRule(IEnumerable<string> validVendors = null) {
            ValidVendors = validVendors == null
            ? null
            : new SortedSet<string>(validVendors, StringComparer.CurrentCultureIgnoreCase);

        }
        public bool IsValid(BudgetLine bl) {
            if (!string.IsNullOrEmpty(bl.Vendor) && (ValidVendors == null || ValidVendors.Contains(bl.Vendor))) return true;

            bl.InvalidColumns.Add(Column);
            bl.InvalidMessages.Add($"Vendor ({bl.Vendor ?? ""}) is invalid");

            return false;
        }
    }
}
