using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    public class StatLine {
        public string Department { get; set; }
        public string LedgerCurrency { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public slType StatLineType {
            get {
                if (string.IsNullOrEmpty(Account))
                    return slType.Other;
                else
                    return Account[0] == '5' ? slType.Revenue
                         : Account[0] == '6' ? slType.Expense
                         : slType.Other;
            }
        }

    }

    public enum slType {
        Other = 1,
        Revenue = 5,
        Expense = 6
    }
}
