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
    public class ProductADController : Controller
    {
        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Admin/ProductAD

        public ActionResult Admin()
        {

            if (Session["AccountSell"] == null)
            {
                return RedirectToAction("Login", "UserAD");
            }
            return View();
        }
        public ActionResult Index(int? size, int? page, string sortProperty, FormCollection f,  string sortOrder = "",string searchString =" ")
        {
            if(Session["AccountSell"]== null)
            {
                return RedirectToAction("Login","UserAD");
            }
            int mspc = Convert.ToInt32(Session["Product"]);
            List<Product> books = (from s in db.Products where s.AccountSellID == mspc select s).ToList();
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
            return View(books.OrderBy(n => n.ProductID).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult LoginLogoutAD()
        {
            return PartialView();
        }
        [HttpGet]
        public ActionResult Create()
        {
            int msdm = Convert.ToInt32(Session["Category"]);
            ViewBag.CategoryID = new SelectList(db.CategoryPs
                                        .Where(s => s.AccountSellID == msdm)
                                        .OrderBy(n => n.Name)
                                        .ToList(), "CategoryID", "Name");
            
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Product product, FormCollection f, HttpPostedFileBase fFileUpload)
        {
            ViewBag.CategoryID = new SelectList(db.CategoryPs.ToList().OrderBy(n => n.Name), "CategoryID", "Name");
            if (fFileUpload == null)
            {
                ViewBag.ThongBao = "Hãy Chọn Ảnh";
                ViewBag.Name = f["sTenSP"];
                ViewBag.Decription = f["sMota"];
                ViewBag.Price = (f["mGiaBan"]);
                ViewBag.Amount = f["sSoLuong"];
                ViewBag.CategoryID = new SelectList(db.CategoryPs.ToList().OrderBy(n => n.Name), "CategoryID", "Name", int.Parse(f["CategoryID"]));
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);

                    }
                    product.Image = sFileName;
                    product.Name = f["sTenSP"];
                    product.Decription = f["sMota"].Replace("<p>", "").Replace("</p>", "\n");
                    product.Price = int.Parse(f["mGiaBan"]);
                    product.Amount = int.Parse(f["sSoLuong"]);
                    product.CategoryID = int.Parse(f["CategoryID"]);
                    product.Status = int.Parse("0");
                    product.AccountSellID = Convert.ToInt32(Session["AccountSell"]);
                    db.Products.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View();
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = db.Products.SingleOrDefault(n => n.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.CategoryID = new SelectList(db.CategoryPs.ToList().OrderBy(n => n.Name), "CategoryID", "Name", product.CategoryID);
            return View(product);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f, HttpPostedFileBase fFileUpload, int id)
        {
            var product = db.Products.SingleOrDefault(n => n.ProductID == id);
            ViewBag.CategoryID = new SelectList(db.CategoryPs.ToList().OrderBy(n => n.Name), "CategoryID", "Name");
            if (ModelState.IsValid)
            {
                if (fFileUpload != null && fFileUpload.ContentLength > 0)
                {

                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    product.Image = sFileName;

                }
                product.Name = f["sTenSP"];
                product.Decription = f["sMota"].Replace("<p>", "").Replace("</p>", "\n");
                product.Price = int.Parse(f["mGiaBan"]);
                product.Amount = int.Parse(f["sSoLuong"]);
                product.CategoryID = int.Parse(f["CategoryID"]);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            return View(product);
        }
        public ActionResult Details(int id)
        {
            var product = db.Products.SingleOrDefault(n => n.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(product);
        }
        public ActionResult Delete(int id)
        {
            var product = db.Products.SingleOrDefault(n => n.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var product = db.Products.SingleOrDefault(n => n.ProductID == id);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var orderDetail = db.OrderDetails.Where(n => n.ProductID == id);
            if (orderDetail.Count() > 0)
            {
                ViewBag.ThongBao = "Sản Phẩm này đang có trong chi tiết đặt hàng<br>" + "Nếu muốn xóa thì phải xóa hết mã này trong chi tiết đặt hàng";
                return View(product);
            }
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }

        
}
