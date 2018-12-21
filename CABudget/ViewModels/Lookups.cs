using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CABudget.Model;

namespace CABudget.ViewModels {
    public class Lookups {
        public List<Account> Accounts { get; set; }
        public List<Vendor> Vendors { get; set; }
    }
}