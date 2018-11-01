using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CABudget.Model {
    public class BudgetLine {
        // data
        public string Year { get; set; }
        public string LedgerCurrency { get; set; }
        public string Entity { get; set; }
        public string Account { get; set; }
        public string State { get; set; }
        public string subAcct_1 { get; set; }
        public string subAcct_2 { get; set; }
        public string Vendor { get; set; }
//      public string Month { get; set; }
//      public DateTime? Period { get; set; }
        public readonly decimal[] Amounts = new decimal[12];

        // logic properties - from Excel and Validation
        public string Department { get; set; }
        public int RowNumber { get; set; }
        public readonly List<int> InvalidColumns = new List<int>();
        public readonly List<string> InvalidMessages = new List<string>();
        public bool IsValid {
            get {
                return InvalidColumns == null || InvalidColumns.Count == 0;
            }
        }

        // logic methods
        /// <summary>
        /// Use this Factory method to get a Reader for a Budget File. Currently only supports Excel files.
        /// </summary>
        /// <param name="input">Pass Budget file as Stream</param>
        /// <param name="blFormat">BudgetFileFormat.XLSX</param>
        /// <param name="sheetNames">omit or set null to process all sheets; include to process only specified sheets</param>
        /// <returns></returns>
        public static IBudgetLineReader GetReader(Stream input, BudgetFileFormat blFormat = BudgetFileFormat.XLSX, IEnumerable<string> sheetNames = null) {
            switch (blFormat) {
                case BudgetFileFormat.XLSX:
                    return new ExcelBudgetLineReader(input, sheetNames);
                default:
                    throw new NotSupportedException("Budget File must be in Excel format.");
            }
        }

        // unused
        public static decimal TotalNetBudgetOf(IEnumerable<BudgetLine> bls) {
            decimal amt = 0M;
            foreach (var bl in bls) {
                for (int i=0; i<12; i++) {
                    amt += bl.Amounts[i];
                }
            }

            return amt;
        }
    }
}
