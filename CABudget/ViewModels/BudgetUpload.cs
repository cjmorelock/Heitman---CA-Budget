using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CABudget.Model;
using CABudget.Model.Validation;

namespace CABudget.ViewModels {
    public class BudgetUpload {
        public List<string> Sheets { get; set; }
        public List<string> Columns { get; set; }
        public HttpPostedFileBase BudgetFile { get; set; }
        public List<BudgetLine> BudgetLines { get; set; }
        public BLStats Stats { get; set; }
    }
}