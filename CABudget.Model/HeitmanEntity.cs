using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public class HeitmanEntity {
        public string EntityNumber { get; set; }
        public string EntityDescription { get; set; }
        public string Entity_Legal { get; set; }
        public string BusinessUnit1 { get; set; }
        public string BusinessUnit2 { get; set; }
        public string LedgerCurrency { get; set; }
    }
}
