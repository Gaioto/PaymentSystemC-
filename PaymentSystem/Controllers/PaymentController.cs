using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using PaymentSystem.Context;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;

namespace PaymentSystem.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        db_paymentEntities db_Payment = new db_paymentEntities();
        public ActionResult Payment(tb_payment payment)
        {
            return View(payment);
        }

        [HttpPost]
        public ActionResult AddPayment(tb_payment payment)
        {
            tb_payment newPayment = new tb_payment();
            if (ModelState.IsValid)
            {
                newPayment.cd_payment = payment.cd_payment;
                newPayment.nm_payment = payment.nm_payment;
                newPayment.vl_payment = payment.vl_payment;
                newPayment.dt_payment = payment.dt_payment;
                newPayment.vl_tax_payment = payment.vl_payment / 0.05;
                newPayment.ds_payment = payment.ds_payment;

                if (payment.cd_payment == 0)
                {
                    db_Payment.tb_payment.Add(newPayment);
                    db_Payment.SaveChanges();
                }
                else
                {
                    db_Payment.Entry(newPayment).State = EntityState.Modified;
                    db_Payment.SaveChanges();
                }

                ModelState.Clear();
                
            }
            
            return View("Payment");
        }

        public ActionResult PaymentList()
        {
            var paymentsList = db_Payment.tb_payment.ToList();
            return View(paymentsList);
        }

        public ActionResult DeletePayment(int id)
        {
            var paymentDelete = db_Payment.tb_payment.Where(x => x.cd_payment == id).First();
            db_Payment.tb_payment.Remove(paymentDelete);
            db_Payment.SaveChanges();

            var paymentList = db_Payment.tb_payment.ToList();

            return View("PaymentList", paymentList);
        }

        [HttpPost]
        public ActionResult ReadExcel(HttpPostedFileBase excelFile)
        {
            if(excelFile == null || excelFile.ContentLength == 0)
            {
                ViewBag.error = "Please select a excel file";
                return View("Payment");
            }
            else
            {
                if (excelFile.FileName.EndsWith("xls") || excelFile.FileName.EndsWith("xlsx"))
                {
                    string path = Server.MapPath("~/Content/" + excelFile.FileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    excelFile.SaveAs(path);

                    Excel.Application application = new Excel.Application();
                    Excel.Workbook workbook = application.Workbooks.Open(path);
                    Excel.Worksheet worksheet = workbook.ActiveSheet;
                    Excel.Range range = worksheet.UsedRange;
                    List<tb_payment> paymentsList = new List<tb_payment>();
                    for(int row = 2; row <= range.Rows.Count; row++)
                    {
                        tb_payment newPayment = new tb_payment();
                        newPayment.nm_payment = ((Excel.Range)range.Cells[row, 1]).Text;
                        newPayment.dt_payment = ((Excel.Range)range.Cells[row, 2]).Text;
                        newPayment.vl_payment = double.Parse(((Excel.Range)range.Cells[row, 3]).Text);
                        newPayment.vl_tax_payment = newPayment.vl_payment / 0.05;
                        newPayment.ds_payment = ((Excel.Range)range.Cells[row, 4]).Text;

                        db_Payment.tb_payment.Add(newPayment);
                        db_Payment.SaveChanges();
                    }
                        
                    return View("Payment");
                }
                else
                {
                    ViewBag.error = "File type is incorrect, please select a excel file";
                    return View("Payment");
                }
            }
        
        }
    }
}