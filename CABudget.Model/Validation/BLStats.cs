using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    public class BLStats : IEnumerable<StatLine> {
        private readonly List<StatLine> _stats;

        public BLStats (IEnumerable<BudgetLine> bls) {
            Dictionary<DeptCurrAcctMatch, StatLine> stats = new Dictionary<DeptCurrAcctMatch, StatLine>();

            foreach (var bl in bls) {
                DeptCurrAcctMatch key = new DeptCurrAcctMatch(bl.Department, bl.LedgerCurrency, bl.Account);
                StatLine sl;
                if (!stats.TryGetValue(key, out sl)) {
                    sl = new StatLine() {
                        Department = bl.Department,
                        LedgerCurrency = bl.LedgerCurrency,
                        Account = bl.Account
                    };
                    stats[key] = sl;
                }
                sl.Amount += bl.Amounts.Sum(); // add sum of 12 months to new or existing StatLine
            }

            _stats = stats.Values.ToList();
        }

        public IEnumerator<StatLine> GetEnumerator() {
            return _stats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    
    /// <summary>
    ///  Class used to match Budget Lines just on the properties: Department, LedgerCurrency and Account
    /// </summary>
    internal class DeptCurrAcctMatch {
        private readonly string _dept;
        private readonly string _curr;
        private readonly string _acct;

        public DeptCurrAcctMatch(string dept, string curr, string acct) {
            this._dept = dept;
            this._curr = curr;
            this._acct = acct;
        }

        public override bool Equals(object obj) {
            var oItem = obj as DeptCurrAcctMatch;
            if (oItem == null) return false;

            return this._dept == oItem._dept
                && this._curr == oItem._curr
                && this._acct == oItem._acct;
        }

        public override int GetHashCode() {
            return ((this._dept ?? "null") + "_" + (this._curr ?? "null") + "_" + (this._acct ?? "null")).GetHashCode();
        }
    }
}
