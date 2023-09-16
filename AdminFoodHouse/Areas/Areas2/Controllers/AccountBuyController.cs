using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using AdminFoodHouse.Models;
using System.IO;
using System.Data.Entity;

namespace AdminFoodHouse.Areas.Areas2.Controllers
{
    public class AccountBuyController : Controller
    {
        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Admin/AccountBuy
        public ActionResult Index(int? size, int? page, string sortProperty, string searchString, string sortOrder = "")
        {

            List<AccountBuy> accounts = (from s in db.AccountBuys select s).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                accounts = accounts.Where(s => s.DisplayName.ToLower().Contains(searchString.ToLower())).ToList();
            }
            page = page ?? 1;
            int pageSize = (size ?? 5);
            ViewBag.pageSize = pageSize;
            int pageNumber = (page ?? 1);
            int checkTotal = (int)(db.AccountBuys.ToList().Count / pageSize) + 1;
            if (pageNumber > checkTotal) pageNumber = checkTotal;
            return View(accounts.OrderBy(n => n.AccountBuyID).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var account = db.AccountBuys.SingleOrDefault(n => n.AccountBuyID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(account);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f, AccountBuy acount, int id)
        {

            acount = db.AccountBuys.SingleOrDefault(n => n.AccountBuyID == id);
            if (ModelState.IsValid)
            {

                acount.UserName = f["sTenDN"];
                acount.DisplayName = f["sTenND"];
                acount.Password = f["sMK"];
                acount.Email = f["sEM"];
                acount.PhoneNumber = f["sPN"];
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(acount);

        }
        public ActionResult Lock(int id)
        {
            var account = db.AccountBuys.SingleOrDefault(n => n.AccountBuyID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                if (account.Status == 0)
                {
                    account.Status = int.Parse("1");
                }
                else if (account.Status == 1)
                {
                    account.Status = int.Parse("0");
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}