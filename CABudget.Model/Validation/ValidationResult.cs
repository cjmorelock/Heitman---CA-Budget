using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.Validation {
    public class ValidationResult {
        public string Name { get; set; }
        public int InvalidCount { get; set; }
        public bool IsValid {
            get {
                return InvalidCount == 0;
            }
        }

    }
}
