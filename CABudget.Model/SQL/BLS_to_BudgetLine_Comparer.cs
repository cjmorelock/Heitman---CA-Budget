using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    // This class is used to compare two BudgetLine objects for "Equality" --
    // This is used for aggregating 12 monthly records in the database into
    // one BudgetLine object containing values for all 12 months.
    class EqualityComparer_map_BLS_to_BudgetLine : IEqualityComparer<BudgetLine> {
        public bool Equals(BudgetLine x, BudgetLine y) {
            if (x == null || y == null) return false;

            return x.Year           == y.Year
                && x.LedgerCurrency == y.LedgerCurrency
                && x.Entity         == y.Entity
                && x.Account        == y.Account
                && x.State          == y.State
                && x.subAcct_1      == y.subAcct_1
                && x.subAcct_2      == y.subAcct_2
                && x.Vendor         == y.Vendor;
        }

        public int GetHashCode(BudgetLine obj) {
            return (obj.Year ?? "null"
                  + obj.LedgerCurrency ?? "null"
                  + obj.Entity ?? "null"
                  + obj.Account ?? "null"
                  + obj.State ?? "null"
                  + obj.subAcct_1 ?? "null"
                  + obj.subAcct_2 ?? "null"
                  + obj.Vendor ?? "null"
                   ).GetHashCode();
        }
    }
}
