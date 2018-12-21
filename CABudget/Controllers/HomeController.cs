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
        // default set of sheets to look for in uploaded workbook
        private List<string> _sheets = new List<string>() { "Debt", "PENA", "PEEU", "PEAP", "PSAP", "PSEU", "PSNA", "Rsrch", "MARK", "Corp", "Exe", "CorpAcc", "IT", "Comp", "RM", "HR" };
        // required columns (in order)
        private List<string> _columns = new List<string>() { "Year", "Ledger Currency", "Entity Number", "Account Number", "State", "Sub Acct 1", "Sub Acct 2", "Vendor ID", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        [CABudgetAuthorize(Roles = "Intranet_CABudget-Full")]
        public ActionResult Index() {
            var model = new BudgetUpload {
                Sheets = _sheets,
                Columns = _columns
            };

            return View(model);
        }

        [CABudgetAuthorize(Roles = "Intranet_CABudget-Full")]
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase BudgetFile) {
            var uow = new UnitOfWork();

            if (BudgetFile != null) {
                // 1) parse uploaded Excel file, 2) do validation, 3) save BudgetLines (if valid)
                // 4) create stats, 5) show result (send Error! if validation fails, or can't save data to budget locked scrubbing table)
                var budgetLines = new List<BudgetLine>();
                string msg = "";

                // 1) Read Excel file
                try { 
                    var reader = BudgetLine.GetReader(BudgetFile.InputStream, BudgetFileFormat.XLSX, _sheets); // we are requiring all specified sheets!                    

                    while (reader.ReadLine()) {
                        budgetLines.Add(reader.Project());
                    }

                    msg = $"File Processed: <b>{BudgetFile.FileName}</b> - {budgetLines.Count} Budget Lines found!";

                } catch (Exception e) {
                    return Json(
                        new { isError = "true",
                            message = BudgetFile.FileName + " is not valid. Uploaded file must be an Excel Workbook with the following sheets: " + string.Join(", ", _sheets.ToArray()) + ".",
                            html = ExceptionAsHtml(e) }
                        , JsonRequestBehavior.AllowGet);
                }
                
                // 2) Validation
                BLValidation validator;
                bool isError = true;
                try {
                    validator = new BLValidation(budgetLines, uow);
                                
                    if (validator.IsValid && budgetLines.Count > 0) {
                        msg += "\r\n<br />" + validator.ValidationSummaryAsHtml;
                        //3) save BudgetLines
                        try {
                            // Delete all
                            uow.BudgetLines.DeleteAll();
                            // Save all                    
                            uow.BudgetLines.AddBulk(budgetLines);
                            isError = false;

                        } catch (Exception e) {
                            return Json(
                                new { isError, message = msg + "\r\n<br />" + $"An error occurred saving Budget Lines found in {BudgetFile.FileName} to the database.", html = ExceptionAsHtml(e) }
                                , JsonRequestBehavior.AllowGet);
                        }
                    } else {
                        msg += "\r\n<br />" + validator.ValidationSummaryAsHtml;
                    }           

                } catch (Exception oe) {
                    return Json(
                           new { isError, message = msg + "\r\n<br />" + $"An error occurred validating the uploaded file: {BudgetFile.FileName}.", html = ExceptionAsHtml(oe) }
                           , JsonRequestBehavior.AllowGet);
                }

                // 4-5) Create Stats and Show Results
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
                    Data = new { isError, message = msg, html = html },
                    MaxJsonLength = 100000000
                };

            } else {
                // Save button ("Commit & Refresh SL Cube") was pressed!
                // Run SQL Server Agent job that processes entries in the Budget_Locked_Scrubbing table added above (on sucessful file updload/processing/validation)
                // and proceeds to refresh the SL cube.
                
                BudgetLineCommitResult result = null;
                    
                try {
                    result = uow.BudgetLines.CommitAll(Request.LogonUserIdentity.Name);
                } catch (Exception ex) {
                    return Json(
                           new { isError = true, message = $"An error occurred Committing and Refreshing the SL Cube.", html = ExceptionAsHtml(ex) }
                           , JsonRequestBehavior.AllowGet);
                }

                bool isError = result.Status != BLCommitStatus.SUCCESS;
                string message = "";
                string html = "";
                switch (result.Status) {
                    case BLCommitStatus.SUCCESS:
                        message = "All budget lines have been commited, and the SL cube is now being refreshed.  You should receive notification by email when this process is complete.";
                        break;
                    case BLCommitStatus.FAIL_CUBE_LOCKED:
                        message = "Commit Failed! Could not get exclusive access to refresh the SL cube. ";
                        if (!string.IsNullOrEmpty(result.Info.Notes)) {
                            message += result.Info.Notes;
                        } else {
                            message += result.Info.UserName + " may be currently refreshing the cube. You should try this action again in a little while.";
                        }
                        break;
                    case BLCommitStatus.FAIL_ERROR:
                        message = $"Commit Failed! An error occurred processing your request.";
                        html = ExceptionAsHtml(result.Exception);
                        break;
                }

                return Json(new { isError, message, html }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Permissions() {
            return View();
        }

        [CABudgetAuthorize(Roles = "Intranet_CABudget-Full")]
        public ActionResult Lookups () {
            var uow = new UnitOfWork();

            var model = new Lookups {
                Accounts = uow.Accounts.GetList(x => true).OrderBy(x => x.Id).ToList(),
                Vendors = uow.Vendors.GetList(x => true).OrderBy(x => x.Id).ToList()
            };

            return View(model);
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