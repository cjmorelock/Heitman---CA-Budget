using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    public class BLValidation {
        private readonly List<BudgetLine> _budgetLines;
        private readonly List<IBudgetLineValidationRule> _blRules;
        private IUnitOfWork _uow;

        public BLValidation(IEnumerable<BudgetLine> bls, IUnitOfWork uow) {
            _budgetLines = bls.ToList();
            _uow = uow;

            _blRules = new List<IBudgetLineValidationRule>();
            _blRules.Add(new YearValidRule());
            _blRules.Add(new AccountValidRule(uow.Accounts.GetList(x => true).Select(x => x.Id)));
            _blRules.Add(new VendorValidRule(uow.Vendors.GetList(x => true).Select(x => x.Id)));
        }

        public List<ValidationResult> ValidationSummary {
            get {
                var invalidBLS = _budgetLines.Where(x => !x.IsValid);
                List<ValidationResult> results = new List<ValidationResult>();
                foreach (var rule in _blRules) {
                    results.Add(new ValidationResult() {
                        Name = rule.GetType().Name,
                        InvalidCount = invalidBLS.Where(x => x.InvalidColumns.Contains(rule.Column)).Count()
                    });
                }
                results.Add(new ValidationResult() {
                    Name = "AmountsValidRule",
                    InvalidCount = invalidBLS.Where(x => x.InvalidColumns.Any(y => y > 8)).Count()
                });

                return results;
            }
        }

        public string ValidationSummaryAsHtml {
            get {
                StringBuilder sb = new StringBuilder();
                var results = ValidationSummary;

                for (int i = 0; i < results.Count; i++) {
                    if (results[i].IsValid) {
                        sb.AppendLine("<span class=\"text-success\">" + (i + 1) + ". " + results[i].Name + " - Passed</span><br />");
                    } else {
                        sb.AppendLine("<span class=\"text-danger\">" + (i + 1) + ". " + results[i].Name + " - Failed - Found " + results[i].InvalidCount + " invalid Budget Line(s)</span><br />");
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// IsValid = true if all Validation Rules pass for all Budget Lines
        /// </summary>
        public bool IsValid {
            get {
                bool valid = true;

                foreach (var bl in _budgetLines) {
                    bool blValid = bl.IsValid; // bl might already be invalid!
                    foreach (var rule in _blRules) {
                        blValid = rule.IsValid(bl) && blValid;
                    }
                    valid = valid && blValid;
                }

                return valid;
            }
        }
    }
}
