using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminFoodHouse.Models;
using AdminFoodHouse.Common;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Security.Principal;


namespace AdminFoodHouse.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        FoodStoreEntities1 db = new FoodStoreEntities1();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection f, AccountBuy Ac, ShoppingCard card)
        {
            var Hoten = f["username"];
            var MatKhau = f["password"];
            var NLMatKhau = f["confirmpassword"];
            var Email = f["email"];
            var SDT = f["phone"];
            var Address = f["address"];
            if (String.IsNullOrEmpty(Hoten))
            {
                ViewData["err1"] = " Họ tên không được để trống";
            }
            else if (String.IsNullOrEmpty(MatKhau))
            {
                ViewData["err2"] = "Mật khẩu không được để trống";
            }
            else if (String.IsNullOrEmpty(NLMatKhau))
            {
                ViewData["err3"] = "Phải nhập lại mật khẩu";
            }
            else if (MatKhau != NLMatKhau)
            {
                ViewData["err3"] = "Mật khẩu nhập lại không trùng khớp";
            }
            else if (String.IsNullOrEmpty(Email))
            {
                ViewData["err4"] = "Email không được để trống";
            }
            else if (String.IsNullOrEmpty(SDT))
            {
                ViewData["err5"] = "Số điện thoại không được để trống";
            }
            else if (db.AccountBuys.SingleOrDefault(n => n.Email == Email) != null)
            {
                ViewBag.ThongBao = "Tài khoản này đã tồn tại";
            }
            else if (String.IsNullOrEmpty(Address))
            {
                ViewData["err6"] = "Địa chỉ không được để trống";
            }
            else if (db.AccountBuys.SingleOrDefault(n => n.PhoneNumber == SDT) != null)
            {
                ViewBag.ThongBao = "Số điện thoại này đã tồn tại. Vui lòng sử dụng số điện thoại khác";
            }
            else
            {
                Ac.Email = Email;
                Ac.UserName = Hoten;
                Ac.DisplayName = Hoten;
                Ac.Password = MatKhau;
                Ac.PhoneNumber = SDT;
                Ac.Status = 0;
                Ac.Address = Address;
                db.AccountBuys.Add(Ac);
                db.SaveChanges();
                AccountBuy ac = db.AccountBuys.SingleOrDefault(n => n.Email == Ac.Email);
                card.AccountID = ac.AccountBuyID;
                db.ShoppingCards.Add(card);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return this.Register();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]


        public ActionResult Login(FormCollection f)
        {

            if (IsValidRecaptcha(Request["g-recaptcha-response"]))
            {
                int state = 0;
                if ((Request.QueryString["id"]) != null)
                {
                    state = int.Parse(Request.QueryString["id"]);
                }

                var email = f["Email"];
                var Matkhau = f["Password"];
                if (String.IsNullOrEmpty(email))
                {
                    ViewData["err1"] = "Email không được để trống";
                }
                else if (String.IsNullOrEmpty(Matkhau))
                {
                    ViewData["err2"] = "Mật khẩu không được để trống";
                }
                else
                {
                    AccountBuy ac = db.AccountBuys.SingleOrDefault(n => n.Email == email && n.Password == Matkhau);
                    if (ac != null)
                    {
                        ViewBag.ThongBao = "Đăng nhập thành công";
                        Session["TaiKhoan"] = ac;

                        ShoppingCard sc = db.ShoppingCards.SingleOrDefault(n => n.AccountID == ac.AccountBuyID);
                        if (sc == null)
                        {
                            // Tạo mới ShoppingCart
                            sc = new ShoppingCard();
                            sc.AccountID = ac.AccountBuyID;
                            db.ShoppingCards.Add(sc);
                            db.SaveChanges();
                        }

                        Session["CartID"] = sc.CartID;
                        if (state == 2)
                        {
                            return RedirectToAction("GioHang", "ShoppingCart");
                        }
                        return RedirectToAction("IndexAccountSell", "Food");
                    }

                    /*
                    AccountBuy ac = db.AccountBuys.SingleOrDefault(n => n.Email == email && n.Password == Matkhau);
                    if (ac != null)
                    {
                        ViewBag.ThongBao = "Đăng nhập thành công";
                        Session["TaiKhoan"] = ac;

                        ShoppingCard sc = db.ShoppingCards.SingleOrDefault(n => n.AccountID == ac.AccountBuyID);

                        Session["CartID"] = sc.CartID;
                        if (state == 2)
                        {
                            return RedirectToAction("GioHang", "Food");
                        }
                        return RedirectToAction("IndexAccountSell", "Food");
                    }

                    */

                    else
                    {
                        ViewBag.ThongBao = "Email hoặc mật khẩu không đúng";
                    }
                }
            }


            return View();
        }

        public ActionResult Logout()
        {
            Session["TaiKhoan"] = null;
            return RedirectToAction("IndexAccountSell", "Food");
        }
        [HttpGet]
        public ActionResult Info()
        {
            AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
            var acount = db.AccountBuys.Single(m => m.AccountBuyID == ac.AccountBuyID);
            return View(acount);
        }
        public ActionResult Info(int id, FormCollection f)
        {
            AccountBuy ac = db.AccountBuys.Single(n => n.AccountBuyID == id);
            if (String.IsNullOrEmpty(f["name"]))
            {
                ViewData["err1"] = "Vui lòng điền đầy đủ Họ và Tên";
            }
            else if (String.IsNullOrEmpty(f["address"]))
            {
                ViewData["err2"] = "Địa chỉ không được để trống";
            }
            else if (String.IsNullOrEmpty(f["sdt"]))
            {
                ViewData["err3"] = "Vui lòng nhập số điện thoại";
            }
            else if (String.IsNullOrEmpty(f["email"]))
            {
                ViewData["err4"] = "Email không đươc để trống";
            }
            else
            {
                if (ac != null)
                {
                    ac.UserName = f["name"];
                    ac.Address = f["address"];
                    ac.PhoneNumber = f["sdt"];
                    ac.Email = f["email"];
                    db.SaveChanges();
                    ViewBag.ThongBao = "Cập nhật thông tin thành công";
                    return this.Info();
                }
            }
            return this.Info();
        }
        public ActionResult changepass()
        {
            AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/changpass.html"));
            content = content.Replace("{{CustomerName}}", ac.DisplayName);
            content = content.Replace("{{Email}}", ac.Email);
            content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
            var random = RandomString();
            Session["random"] = random;
            var noidung = "http://localhost:10447/User/ChangePassword?id=" + random;
            content = content.Replace("{{Link}}", noidung.ToString());

            var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

            // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
            //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
            //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
            //của ứng dụng kém an toàn phải ở chế độ bật
            //  Đồng thời tài khoản Gmail cũng cần bật IMAP
            //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

            new MailHelper().SendMail(ac.Email, "Thay đổi mật khẩu", content);
            return RedirectToAction("Info");
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if ((Request.QueryString["id"]) != Session["random"].ToString())
            {
                return RedirectToAction("IndexAcountSell", "Food");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection f)
        {
            AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
            if (f["newpass"] != f["enterpass"])
            {
                ViewBag.ThongBao = "Mật khẩu không trùng khớp";
                return this.ChangePassword();
            }
            else
            {
                AccountBuy acn = db.AccountBuys.Single(m => m.AccountBuyID == ac.AccountBuyID);
                acn.Password = f["newpass"];
                db.SaveChanges();
                Session["random"] = null;
                return RedirectToAction("IndexAcountSell", "Food"); ;
            }
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
        public ActionResult resetpass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult resetpass(FormCollection f)
        {
            string email = f["email"];
            AccountBuy tk = db.AccountBuys.FirstOrDefault(m => m.Email == email);
            if (tk == null)
            {
                ViewBag.ThongBao = "Email không phù hợp.Vui lòng nhập lại hoặc tạo tài khoản mới";
                return this.resetpass();
            }
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/changpass.html"));
            content = content.Replace("{{CustomerName}}", tk.DisplayName);
            content = content.Replace("{{Email}}", tk.Email);
            content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
            var random = RandomString();
            Session["reset"] = random;
            var noidung = "http://localhost:10447//User/Reset?id=" + random + "&ms=" + tk.AccountBuyID;
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
        [HttpGet]
        public ActionResult Reset()
        {
            if ((Request.QueryString["id"]) != Session["reset"].ToString())
            {
                return RedirectToAction("IndexAcountSell", "Food");
            }
            int ms = int.Parse(Request.QueryString["ms"]);
            AccountBuy find = db.AccountBuys.Single(m => m.AccountBuyID == ms);
            return View(find);
        }
        [HttpPost]
        public ActionResult Reset(FormCollection f)
        {
            int mus = Convert.ToInt32(f["iduser"]);
            AccountBuy ac = db.AccountBuys.First(n => n.AccountBuyID == mus);
            if (f["newpass"] != f["enterpass"])
            {
                ViewBag.ThongBao = "Mật khẩu không trùng khớp";
                return this.ChangePassword();
            }
            else
            {
                ac.Password = f["newpass"];
                db.SaveChanges();
                Session["reset"] = null;
                return RedirectToAction("Login", "User");
            }
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