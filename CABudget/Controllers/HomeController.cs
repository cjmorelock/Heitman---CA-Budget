using CABudget.Code;
using CABudget.Model;
using CABudget.Model.SQL;
using CABudget.Model.Validation;
using CABudget.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CABudget.Controllers {
    public class HomeController : Controller {
        // default set of sheets (not required)
        private List<string> _sheets = new List<string>() { "Debt", "PENA", "PEEU", "PEAP", "PSAP", "PSEU", "PSNA", "Rsrch", "MARK", "Corp", "Exe", "CorpAcc", "IT", "Comp", "RM", "HR" };
        // required columns (in order)
        private List<string> _columns = new List<string>() { "Year", "Ledger Currency", "Entity Number", "Account Number", "State", "Sub Acct 1", "Sub Acct 2", "Vendor ID", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public ActionResult Index() {
            var model = new BudgetUpload {
                Sheets = _sheets,
                Columns = _columns
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase BudgetFile) {
            var uow = new UnitOfWork();

            if (BudgetFile != null) {
                // parse uploaded Excel file
                // do validation
                // save BudgetLines (if valid)
                // create stats
                // show result - send Error! if validation fails, or can't save data to budget locked scrubbing table
                var budgetLines = new List<BudgetLine>();
                try { 
                    var reader = BudgetLine.GetReader(BudgetFile.InputStream, BudgetFileFormat.XLSX); // not specifying required tabs here                    

                    while (reader.ReadLine()) {
                        budgetLines.Add(reader.Project());
                    }
                } catch (Exception e) {
                    return Json(
                        new { isError = "true", message = BudgetFile.FileName + " is not valid. Uploaded file must be an Excel Workbook.", html = ExceptionAsHtml(e) }
                        , JsonRequestBehavior.AllowGet);
                }
                
                // Validation
                string msg = $"File Processed: <b>{BudgetFile.FileName}</b> - {budgetLines.Count} Budget Lines found!";
                bool isError = true;
                var validator = new BLValidation(budgetLines, uow);
                if (validator.IsValid && budgetLines.Count > 0) {
                    try {
                        // Delete all
                        uow.BudgetLines.DeleteAll();
                        // Save all                    
                        uow.BudgetLines.AddBulk(budgetLines);
                        isError = false;
                    } catch (Exception e) {
                        return Json(
                            new { isError = "true", message = $"An error occurred saving Budget Lines found in {BudgetFile.FileName} to the database.", html = ExceptionAsHtml(e) }
                            , JsonRequestBehavior.AllowGet);
                    }
                } 
                msg += "\r\n<br />" + validator.ValidationSummaryAsHtml;

                var model = new BudgetUpload {
                    Sheets = budgetLines.Select(x => x.Department).Distinct().ToList(),  // actual sheets found
                    Columns = _columns,
                    BudgetLines = budgetLines,
                    Stats = new BLStats(budgetLines)
                };

                string html = this.RenderViewToString("_ssTables", model); // custom, extension method

                //return Json(new { isError = isError, message = msg, html = html }, JsonRequestBehavior.AllowGet); 
                // default max json returned is either 102,400 characters or 2,097,152 characters (i'm inclined to think the latter); need to increase the limit like this!
                return new JsonResult() {
                    Data = new { isError = isError, message = msg, html = html },
                    MaxJsonLength = 100000000
                };

            } else {
                // Save button was pressed!
                // To Do: Run stored procedure on SQL server that processes entries in the Budget_Locked_Scrubbing table added above (on sucessful file updload/processing/validation)
                return Json(new { isError = false, message = "Save button pressed - nothing done", html = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TEST() {
            return View();
        }
        
        private string ExceptionAsHtml(Exception e) {
            StringBuilder sb = new StringBuilder();
            int i = 0;

            do {
                sb.AppendLine($"<h{++i}>{e.Message}</h{i}");
                sb.AppendLine($"<p>{e.StackTrace.Replace("\r\n", "<br />")}</p>");

                e = e.InnerException;
            } while (e != null);

            return sb.ToString();
        }
    }
}