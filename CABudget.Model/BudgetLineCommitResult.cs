using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public class BudgetLineCommitResult {
        public BLCommitStatus Status { get; set; }
        public CubeLockInfo Info { get; set; }
        public Exception Exception { get; set; }

        // ctr
        internal BudgetLineCommitResult() { }
    }

    public enum BLCommitStatus {
        SUCCESS = 1,
        FAIL_CUBE_LOCKED = 2,
        FAIL_ERROR = 3
    }
}
