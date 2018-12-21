using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    /// <summary>
    /// Use - 1) Create an instance of this class passing in the set of valid Accounts
    ///       2) For each BudgetLine to be validated call the IsValid method on this instance
    /// </summary>
    class AccountValidRule : IBudgetLineValidationRule {
        // Account should always be in column 4 of the Excel sheets
        public int Column {
            get { return 4; }
        }
        protected readonly SortedSet<string> ValidAccounts;

        /// <summary>
        /// ctr
        /// </summary>
        /// <param name="validAccounts">if null, then any non-empty account will be valid; otherwise account must be found in validAccounts collection</param>
        public AccountValidRule (IEnumerable<string> validAccounts = null) {
            ValidAccounts = validAccounts == null
                ? null
                : new SortedSet<string>(validAccounts);
        }

        public bool IsValid(BudgetLine bl) {
            if (!string.IsNullOrEmpty(bl.Account) && (ValidAccounts == null || ValidAccounts.Contains(bl.Account))) return true;

            bl.InvalidColumns.Add(Column);
            bl.InvalidMessages.Add($"Account ({bl.Account ?? ""}) is invalid");

            return false;
        }
    }
}
