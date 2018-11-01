using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CABudget.Model;
using CABudget.Model.SQL;
using System.Collections.Generic;
using System.IO;
using CABudget.Model.Validation;
using System.Linq;

namespace CABudget.Tests {
    [TestClass]
    public class UnitTest1 {

        [TestMethod]
        public void Test_Stats() {
            string path = @"c:\temp\BudgetFile.xlsx";
            Stream input = new StreamReader(path).BaseStream;
            var reader = BudgetLine.GetReader(input, BudgetFileFormat.XLSX);

            var budgetLines = new List<BudgetLine>();

            while (reader.ReadLine()) {
                budgetLines.Add(reader.Project());
            }

            BLStats stats = new BLStats(budgetLines);

            var byDepartments = stats.GroupBy(x => x.Department);
            foreach (var deptGroup in byDepartments) {
                Console.WriteLine(deptGroup.Key);
                var byCurrency = deptGroup.GroupBy(x => x.LedgerCurrency);
                foreach (var currGroup in byCurrency) {
                    Console.WriteLine("    " + currGroup.Key + " : " + currGroup.Count());
                }
            }
        }

        [TestMethod]
        public void Test_FullCircle() {
            string path = @"c:\temp\BudgetFile.xlsx";
            Stream input = new StreamReader(path).BaseStream;
            var reader = BudgetLine.GetReader(input, BudgetFileFormat.XLSX);
            
            var budgetLines = new List<BudgetLine>();

            while (reader.ReadLine()) {
                budgetLines.Add(reader.Project());
            }
            decimal amt = BudgetLine.TotalNetBudgetOf(budgetLines);
            
            IUnitOfWork uow = new UnitOfWork();
            uow.BudgetLines.DeleteAll();
            uow.BudgetLines.AddBulk(budgetLines);
            var dbBLS = uow.BudgetLines.GetList(x => true);

            Assert.IsTrue(amt - BudgetLine.TotalNetBudgetOf (dbBLS) < Math.Abs(1)); // sums within $1.00
        }

        [TestMethod]
        public void Test_ReadExcel() {
            string path = @"c:\temp\BudgetFile.xlsx";
            Stream input = new StreamReader(path).BaseStream;
            var reader = BudgetLine.GetReader(input, BudgetFileFormat.XLSX);
            int count = 0;
            while (reader.ReadLine()) {
                count++;
            }

            Assert.AreEqual(count, 1784);
        }

        [TestMethod]
        public void Test_AddBulk() {
            IUnitOfWork uow = new UnitOfWork();

            int count1 = uow.BudgetLines.GetList(x => true).Count;

            var bl1 = new BudgetLine() {
                Year = "2018",
                Entity = "0001",
                Account = "1001",
                LedgerCurrency = "USD"
            };
            var bl2 = new BudgetLine() {
                Year = "2018",
                Entity = "0002",
                Account = "1002",
                LedgerCurrency = "USD"
            };

            List<BudgetLine> bls = new List<BudgetLine>() { bl1, bl2 };
            uow.BudgetLines.AddBulk(bls);
            int count2 = uow.BudgetLines.GetList(x => true).Count;

            Assert.AreEqual(count1 + 2, count2);
        }

        [TestMethod]
        public void Test_GetAll() {
            IUnitOfWork uow = new UnitOfWork();
            List<BudgetLine> bls = uow.BudgetLines.GetList(x => true);

            Assert.IsTrue(bls.Count > 0);
        }
    }
}
