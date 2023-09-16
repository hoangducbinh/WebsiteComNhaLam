using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminFoodHouse.Models;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using AdminFoodHouse.Common;
namespace AdminFoodHouse.Areas.Admin.Controllers
{
    public class UserADController : Controller
    {

        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Admin/UserAD
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            if (IsValidRecaptcha(Request["g-recaptcha-response"]))
            {
                var sTenDN = f["UserName"];
                var sMatKhau = f["PassWord"];
                if (String.IsNullOrEmpty(sTenDN))
                {
                    ViewData["err1"] = "Tên đăng nhập không được để trống";
                }
                else if (String.IsNullOrEmpty(sMatKhau))
                {
                    ViewData["err2"] = "Mật khẩu không được để trống";
                }
                else
                {
                    AccountSell ad = db.AccountSells.SingleOrDefault(n => n.UserName == sTenDN && n.Password == sMatKhau);
                    if (ad != null)
                    {
                        Session["AccountSell"] = ad.AccountSellID;
                        Session["AccountSell2"] = ad;
                        Product pt = db.Products.FirstOrDefault(n => n.AccountSellID == ad.AccountSellID);
                        CategoryP ct = db.CategoryPs.FirstOrDefault(n => n.AccountSellID == ad.AccountSellID);
                        if (pt == null)
                        {
                            return RedirectToAction("Admin", "ProductAD");
                        }
                        else
                        {
                            Session["Product"] = pt.AccountSellID;
                            Session["Category"] = ct.AccountSellID;
                            return RedirectToAction("Admin", "ProductAD");
                        }
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    }
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Register(FormCollection f, AccountSell Ac, ShoppingCard card, HttpPostedFileBase fFileUpload)
        {
            var Hoten = f["displayname"];
            var TenDN = f["username"];
            var MatKhau = f["password"];
            var NLMatKhau = f["confirmpassword"];
            var Email = f["email"];
            var SDT = f["phone"];
            var Address = f["address"];
            var Infomation = f["information"];
            if (String.IsNullOrEmpty(Hoten))
            {
                ViewData["err1"] = " Họ tên không được để trống";
            }
            else if(String.IsNullOrEmpty(TenDN))
            {
                ViewData["err2"] = " Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(MatKhau))
            {
                ViewData["err3"] = "Mật khẩu không được để trống";
            }
            else if (String.IsNullOrEmpty(NLMatKhau))
            {
                ViewData["err4"] = "Phải nhập lại mật khẩu";
            }
            else if (MatKhau != NLMatKhau)
            {
                ViewData["err11"] = "Mật khẩu nhập lại không trùng khớp";
            }
            else if (String.IsNullOrEmpty(Email))
            {
                ViewData["err5"] = "Email không được để trống";
            }
            else if (String.IsNullOrEmpty(SDT))
            {
                ViewData["err6"] = "Số điện thoại không được để trống";
            }
            else if (db.AccountSells.SingleOrDefault(n => n.Email == Email) != null)
            {
                ViewBag.ThongBao = "Email này đã tồn tại";
            }
            else if (String.IsNullOrEmpty(Address))
            {
                ViewData["err7"] = "Địa chỉ không được để trống";
            }
            else if (String.IsNullOrEmpty(Infomation))
            {
                ViewData["err8"] = "Mô tả không được để trống";
            }
            else if (db.AccountSells.SingleOrDefault(n => n.PhoneNumber == SDT) != null)
            {
                ViewBag["err10"] = "Số điện thoại này đã tồn tại. Vui lòng sử dụng số điện thoại khác";
            }
            else
            {
                if (fFileUpload == null)
                {
                    ViewData["err9"] = "Hãy chọn ảnh";
                }
                else
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    Ac.AcImage = sFileName;
                    Ac.DisplayName = Hoten;
                    Ac.Email = Email;
                    Ac.UserName = TenDN;
                    Ac.DisplayName = Hoten;
                    Ac.Password = MatKhau;
                    Ac.PhoneNumber = SDT;
                    Ac.Status = 0;
                    Ac.Address = Address;
                    Ac.Information = Infomation;
                    db.AccountSells.Add(Ac);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
            }
            return View("Error", "Đã xảy ra lỗi khi đăng ký tài khoản");
        }
        [HttpGet]
        public ActionResult Change()
        {
            int id = Convert.ToInt32(Session["AccountSell"]);
            var account = db.AccountSells.SingleOrDefault(n => n.AccountSellID == id);
            return View(account);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Change(FormCollection f, AccountSell Ac, ShoppingCard card, HttpPostedFileBase fFileUpload)
        {
            int id = Convert.ToInt32(Session["AccountSell"]);
            var account = db.AccountSells.SingleOrDefault(n => n.AccountSellID == id);
            if (ModelState.IsValid)
            {
                var Hoten = f["displayname"];
                var TenDN = f["username"];
                
                var Email = f["email"];
                var SDT = f["phone"];
                var Address = f["address"];
                var Infomation = f["information"];
                if (String.IsNullOrEmpty(Hoten))
                {
                    ViewData["err1"] = " Họ tên không được để trống";
                }
                else if (String.IsNullOrEmpty(TenDN))
                {
                    ViewData["err2"] = " Tên đăng nhập không được để trống";
                }
                
                else if (String.IsNullOrEmpty(Email))
                {
                    ViewData["err5"] = "Email không được để trống";
                }
                else if (String.IsNullOrEmpty(SDT))
                {
                    ViewData["err6"] = "Số điện thoại không được để trống";
                }
                else if (db.AccountSells.SingleOrDefault(n => n.Email == Email) != null)
                {
                    ViewBag.ThongBao = "Email này đã tồn tại";
                }
                else if (String.IsNullOrEmpty(Address))
                {
                    ViewData["err7"] = "Địa chỉ không được để trống";
                }
                else if (String.IsNullOrEmpty(Infomation))
                {
                    ViewData["err8"] = "Mô tả không được để trống";
                }
                else if (db.AccountSells.SingleOrDefault(n => n.PhoneNumber == SDT) != null)
                {
                    ViewBag.ThongBao = "Số điện thoại này đã tồn tại. Vui lòng sử dụng số điện thoại khác";
                }
                else
                {
                    if (fFileUpload == null)
                    {
                        ViewData["err9"] = "Hãy chọn ảnh";
                    }
                    else
                    {
                        var sFileName = Path.GetFileName(fFileUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                        if (!System.IO.File.Exists(path))
                        {
                            fFileUpload.SaveAs(path);
                        }
                        Ac.AcImage = sFileName;
                        Ac.DisplayName = Hoten;
                        Ac.Email = Email;
                        Ac.UserName = TenDN;
                        Ac.DisplayName = Hoten;
                        Ac.PhoneNumber = SDT;
                        Ac.Status = 0;
                        Ac.Address = Address;
                        Ac.Information = Infomation;
                        db.AccountSells.Add(Ac);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                return View("Error", "Đã xảy ra lỗi khi Lưu thông tin");
            }
            return View(account);
        }
        public ActionResult Logout()
        {
            Session.Remove("AccountSell");
            return RedirectToAction("Login");
        }
        private static Random random = new Random();
        public static string RandomString()
        {
            int length = 8;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgetPassword(FormCollection f)
        {
            string email = f["email"];
            AccountSell tk = db.AccountSells.FirstOrDefault(m => m.Email == email);
            if (tk == null)
            {
                ViewBag.ThongBao = "Email không phù hợp.Vui lòng nhập lại hoặc tạo tài khoản mới";
                return this.ForgetPassword();
            }
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/changpass.html"));
            content = content.Replace("{{CustomerName}}", tk.DisplayName);
            content = content.Replace("{{Email}}", tk.Email);
            content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
            var random = RandomString();
            Session["reset"] = random;
            var noidung = "http://localhost:59039/User/Reset?id=" + random + "&ms=" + tk.AccountSellID;
            content = content.Replace("{{Link}}", noidung.ToString());

            // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
            //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
            //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
            //của ứng dụng kém an toàn phải ở chế độ bật
            //  Đồng thời tài khoản Gmail cũng cần bật IMAP
            //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

            new MailHelper().SendMail(tk.Email, "Đặt lại mật khẩu", content);
            ViewBag.ThongBao = "Vui lòng kiểm tra Email của bạn";
            return this.Login();
        }
        private bool IsValidRecaptcha(string recaptcha)
        {
            if (string.IsNullOrEmpty(recaptcha))
            {
                return false;
            }
            var secretKey = "6Lf-yw0jAAAAALn29HBaqIfdhm_N1vXxWFzP42sH";//Mã bí mật
            string remoteIp = Request.ServerVariables["REMOTE_ADDR"];
            string myParameters = String.Format("secret={0}&response={1}&remoteip={2}", secretKey, recaptcha, remoteIp);
            RecaptchaResult captchaResult;
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var json = wc.UploadString("https://www.google.com/recaptcha/api/siteverify", myParameters);
                var js = new DataContractJsonSerializer(typeof(RecaptchaResult));
                var ms = new MemoryStream(Encoding.ASCII.GetBytes(json));
                captchaResult = js.ReadObject(ms) as RecaptchaResult;
                if (captchaResult != null && captchaResult.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}