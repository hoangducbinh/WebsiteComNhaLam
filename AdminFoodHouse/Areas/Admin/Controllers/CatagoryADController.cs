using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using AdminFoodHouse.Models;
using System.IO;
using System.Data.Entity;

namespace AdminFoodHouse.Areas.Admin.Controllers
{
    public class CatagoryADController : Controller
    {
        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Admin/CatagoryAD
        [HttpGet]
        public ActionResult Index(int? size, int? page, string sortProperty, string searchString ="", string sortOrder = "")
        {
            int mspc = Convert.ToInt32(Session["Category"]);
            List<CategoryP> books = (from s in db.CategoryPs where s.AccountSellID == mspc select s).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }
            page = page ?? 1;
            int pageSize = (size ?? 5);
            ViewBag.pageSize = pageSize;
            int pageNumber = (page ?? 1);
            int checkTotal = (int)(books.ToList().Count / pageSize) + 1;
            if (pageNumber > checkTotal) pageNumber = checkTotal;
            return View(books.OrderBy(n => n.CategoryID).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]

        public ActionResult Create(CategoryP category, FormCollection f)
        {
            var STT = int.Parse(f["Stt"]);
            var Name = f["sTenLSP"];
            
             if (String.IsNullOrEmpty(Name))
            {
                ViewData["err1"] = "Tên sản phẩm không được để trống";
            }
            category.Name = Name;
            category.STT = STT;
            category.AccountSellID = Convert.ToInt32(Session["AccountSell"]);
            db.CategoryPs.Add(category);
            db.SaveChanges();
            return RedirectToAction("Index");

        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
           
            var account = db.CategoryPs.SingleOrDefault(n => n.CategoryID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(account);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var account = db.CategoryPs.SingleOrDefault(n => n.CategoryID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            db.CategoryPs.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var caterogy = db.CategoryPs.SingleOrDefault(n => n.CategoryID == id);
            if (caterogy == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(caterogy);
        }
        [HttpPost]
        public ActionResult Edit(CategoryP category, FormCollection f, int id)
        {
            var STT = int.Parse(f["Stt"]);
            var Name = f["sTenLSP"];
            if (db.CategoryPs.SingleOrDefault(n => n.STT == STT) != null)
            {
                ViewBag.ThongBao = "Số này đã tồn tại";
            }
            else if (String.IsNullOrEmpty(Name))
            {
                ViewData["err1"] = "Tên sản phẩm không được để trống";
            }
            category = db.CategoryPs.SingleOrDefault(n => n.CategoryID == id);
            if (ModelState.IsValid)
            {
                category.Name = Name;
                category.STT = STT;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();

        }
    }
}