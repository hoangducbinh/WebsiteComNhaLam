﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using AdminFoodHouse.Models;
using AdminFoodHouse.Other;
using AdminFoodHouse.Common;
using System.Security.Principal;
using System.Web.UI;

namespace AdminFoodHouse.Controllers
{
    public class FoodController : Controller
    {
        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Food
        /* public ActionResult Index(System.Nullable<int> id, int? page)*/

        
        public ActionResult Index(int? page, int id)
        {

           
            int size = 8;
            int iPageNum = (page ?? 1);
            var Food = from s in db.Products where s.Status == 1 && s.AccountSellID == id select s;
            return View(Food.OrderBy(n => n.Status == 1).ToPagedList(iPageNum, size));

        }

        public ActionResult IndexAccountSell(int? page)
        {

            string thongBao = TempData["ThongBao"] as string;
            if (!string.IsNullOrEmpty(thongBao))
            {
                ViewBag.ThongBao = thongBao;
            }

            int size = 8;
            int iPageNum = (page ?? 1);
            var Account = from s in db.AccountSells where s.Status == 0 select s;
            return View(Account.OrderBy(n => n.Status == 0).ToPagedList(iPageNum, size));
        }




        public ActionResult Category(int id)
        {
            var Cate = from s in db.CategoryPs where s.AccountSellID == id orderby s.STT ascending select s;
            return PartialView(Cate);
        }
        public ActionResult CategoryDetail(int idP, int id, int? page)
        {
            ViewBag.IDCate = id;
            int size = 6;
            int iPageNum = (page ?? 1);
            var ca = from s in db.Products
                     where s.CategoryID == id && s.Status == 1 && s.AccountSellID == idP
                     orderby s.ProductID
                     select s;
            ViewBag.IDsell = idP;
            return View(ca.ToPagedList(iPageNum, size));
        }
        public ActionResult ProductDetail(int id)
        {
            var pro = from s in db.Products
                      where s.ProductID == id
                      select s;
            return View(pro.Single());
        }
        public ActionResult _LoginLogout()
        {
            return PartialView("_LoginLogout");
        }




        public ActionResult Payment(double value)
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
            Session["value"] = value;
            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", value.ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Ulti.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();
                int money = Convert.ToInt32(Session["value"]);
                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
                        Order ctac = db.Orders.FirstOrDefault(n => n.AccountID == ac.AccountBuyID);
                        if (ctac == null)
                        {
                            Order od = new Order();
                            od.Date = DateTime.Now;
                            od.AccountID = ac.AccountBuyID;
                            db.Orders.Add(od);
                            db.SaveChanges();
                        }
                        var noidung = "";
                        List<CartDetail> em = Session["pay"] as List<CartDetail>;
                        Order ct = db.Orders.FirstOrDefault(n => n.AccountID == ac.AccountBuyID);
                        foreach (var item in em)
                        {
                            OrderDetail ctdh = new OrderDetail();
                            ctdh.OrderID = ct.OrderID;
                            ctdh.ProductID = item.ProductID;
                            Product pr = db.Products.SingleOrDefault(m => m.ProductID == item.ProductID);
                            ctdh.Quantity = Convert.ToInt32(item.Quantity);
                            noidung += "  <tr>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;word-wrap:break-word'>";
                            noidung += pr.Name + "</td>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'>";
                            noidung += item.Quantity + "</td>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'><span>";
                            noidung += pr.Price + "&nbsp;<span> VND </span></span></td>";
                            ctdh.PayStatus = 1;
                            db.OrderDetails.Add(ctdh);
                            db.SaveChanges();
                            CartDetail cd = db.CartDetails.First(n => n.ProductID == ctdh.ProductID);
                            db.CartDetails.Remove(cd);
                            db.SaveChanges();
                        }
                        OrderDetail dl = db.OrderDetails.FirstOrDefault(m => m.OrderID == ct.OrderID);
                        string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/send1.html"));
                        int Total = Convert.ToInt32(em.Sum(n => n.Quantity * n.Price)); ;
                        content = content.Replace("{{CustomerName}}", dl.CustomerName);
                        content = content.Replace("{{Phone}}", dl.PhoneNumber);
                        content = content.Replace("{{Email}}", dl.Email);
                        content = content.Replace("{{Address}}", dl.Address);
                        content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
                        content = content.Replace("{{Total}}", money.ToString("N0"));
                        content = content.Replace("{{mes}}", noidung.ToString());
                        var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

                        // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
                        //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
                        //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
                        //của ứng dụng kém an toàn phải ở chế độ bật
                        //  Đồng thời tài khoản Gmail cũng cần bật IMAP
                        //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

                        new MailHelper().SendMail(dl.Email, "Đơn hàng mới từ Cơm nhà làm", content);
                        new MailHelper().SendMail(toEmail, "Đơn hàng mới từ Cơm nhà làm", content);
                        Session["pay"] = null;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            // return this.Index();
            return RedirectToAction("ManageOrder", "HistoryOrder");

        }







        public ActionResult PostReview()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Review(int id)
        {
            var rv = from s in db.Rate_
                     where s.ProductID == id
                     select s;
            return PartialView(rv);
        }
        public ActionResult Star(int id)
        {
            List<Rate_> rv = (from s in db.Rate_
                              where s.ProductID == id
                              select s).ToList();
            ViewBag.count = rv.Count;
            ViewBag.Avg = Convert.ToInt32(rv.Average(m => m.NumberStar));
            return PartialView(rv);
        }
        [HttpPost]
        public ActionResult InsertReview(int id, FormCollection f)
        {
            Rate_ rv = new Rate_();
            AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
            if (id != 0)
            {
                rv.AccountID = ac.AccountBuyID;
                rv.Comment = f["comment"];
                rv.NumberStar = Convert.ToInt32(f["star"]);
                rv.Date = DateTime.Now;
                rv.ProductID = id;
                db.Rate_.Add(rv);
                db.SaveChanges();
            }

            return RedirectToAction("ProductDetail", new { id = id });
        }

        public ActionResult Find(int? page, string searchString="")
        {
            var links = from l in db.Products where l.Status == 1
                        select l;
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                links = links.Where(s => s.Name.ToLower().Contains(searchString));
            }
            
            var stores = from s in db.AccountSells where s.Status == 1
                         select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                stores = stores.Where(s => s.DisplayName.ToLower().Contains(searchString));
            }

        

            int pageNumber = (page ?? 1);
            int pageSize = 8;
            var sortedLinks = links.OrderBy(l => l.Name); // sắp xếp dữ liệu
            return View(sortedLinks.ToPagedList(pageNumber, pageSize));
        }

















        public ActionResult popup()
        {
            return View();
        }

    }
}


