using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model {
    /// <summary>
    /// Reads all Budget Lines from Excel file
    /// Usage of this class is similar to System.Data.SqlClient.SqlDataReader
    /// var reader = new BudgetLine.GetReader(..);
    /// while (reader.ReadLine()) {
    ///     budgetLines.Add(reader.Project());
    /// }
    /// </summary>
    class ExcelBudgetLineReader : IBudgetLineReader {
        protected readonly List<string> sheets;
        protected readonly ExcelPackage package;
        protected ExcelWorksheet currentSheet;
        protected int currentRow;

        /// <summary>
        /// Reads an Execel file passed as a stream
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sheetNames">omit or set null to process all sheets; include to process only specified sheets</param>
        internal ExcelBudgetLineReader(Stream input, IEnumerable<string> sheetNames=null) {
            package = new ExcelPackage(input);
            
            sheets = sheetNames == null
                ? package.Workbook.Worksheets.Select(x => x.Name).ToList()
                : sheetNames.ToList();

            currentSheet = package.Workbook.Worksheets[sheets[0]]; // exception if sheet name not found or if sheets is empty
            currentRow = 1; // expect that there will be a header row - row numbers start with 1
        }

        public bool ReadLine() {
            currentRow++;
            if (currentRowIsEmpty()) {
                return nextSheet();
            } else { 
                return true;
            }
        }
        /// <summary>
        /// Convert row of Excel file to instance of BudgetLine
        /// Requires all columns to appear in order
        /// 1. Year, 2. LedgerCurrency, 3. Entity, 4. Account, 5. State, 6. subAcct_1, 7. subAcct_2, 8. Vendor, 9.-20. January-December (amounts)
        /// </summary>
        /// <returns></returns>
        public BudgetLine Project() {
            var item = new BudgetLine();
            int i = 0;
            item.Year = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.LedgerCurrency = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.Entity = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.Account = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.State = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.subAcct_1 = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.subAcct_2 = currentSheet.Cells[currentRow, ++i].Text.Trim();
            item.Vendor = currentSheet.Cells[currentRow, ++i].Text.Trim();

            for(int j=0; j<12; j++) {
                // I do validation for amounts here!
                object valC = currentSheet.Cells[currentRow, ++i].Value;
                string strAmt = Convert.ToString(valC);
                //string strAmt = currentSheet.Cells[currentRow, ++i].Text;
                decimal amt;
                if (decimal.TryParse(strAmt, out amt)) {
                    item.Amounts[j] = Math.Round(amt, 2);
                } else {
                    item.InvalidColumns.Add(i);
                    item.InvalidMessages.Add($"Amount ({strAmt ?? "null"}) is invalid");
                }
            }

            item.Department = currentSheet.Name;
            item.RowNumber = currentRow;
            return item;
        }

        private bool currentRowIsEmpty () {
            var val = currentSheet.Cells[currentRow, 1].Text.Trim();
            return string.IsNullOrEmpty(val);
        }

        private bool nextSheet () {
            int next = sheets.IndexOf(currentSheet.Name) + 1;

            if (next == sheets.Count) {
                return false; // no more sheets
            } else {
                currentSheet = package.Workbook.Worksheets[sheets[next]]; // throw exception if sheet not found
                currentRow = 2; // expect header row, rows start at 1
                                
                if (currentRowIsEmpty()) {
                    return nextSheet();
                } else {
                    return true;
                }
            }
        }
    }
}
