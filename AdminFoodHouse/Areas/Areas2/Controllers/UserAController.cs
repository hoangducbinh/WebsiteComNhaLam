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

namespace AdminFoodHouse.Areas.Areas2.Controllers
{
    public class UserAController : Controller
    {
        // GET: Areas2/UserA
        FoodStoreEntities1 db = new FoodStoreEntities1();
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
                    ADMIN ad = db.ADMINs.SingleOrDefault(n => n.UserName == sTenDN && n.PassWord == sMatKhau);
                    if (ad != null)
                    {
                        Session["Admin"] = ad;
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    }
                }
            }
            return View();
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