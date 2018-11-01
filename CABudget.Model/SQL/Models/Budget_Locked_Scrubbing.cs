using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public class Budget_Locked_Scrubbing {
        public string Year { get; set; }
        public string LedgerCurrency { get; set; }
        public string Entity { get; set; }
        public string Account { get; set; }
        public string State { get; set; }
        public string subAcct_1 { get; set; }
        public string subAcct_2 { get; set; }
        public string Vendor { get; set; }
        public string Month { get; set; }
        public DateTime Period { get; set; }
        public decimal Amount { get; set; }
    }
}
