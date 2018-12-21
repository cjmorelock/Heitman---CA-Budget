using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    public class CubeLockInfo {

        public CubeLockInfo() { }
        public CubeLockInfo(string cubeName, string email) {
            CubeName = cubeName;
            UserName = email;
        }

        public string CubeName { get; set; }
        public string CubeLock { get; set; }
        public string UserName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public string Notes { get; set; }
    }
}
