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
    public class ProductController : Controller
    {
        FoodStoreEntities1 db = new FoodStoreEntities1();
        // GET: Areas2/Product
        public ActionResult Index(int? size, int? page, string sortProperty, string searchString ="", string sortOrder = "", int categoryID = 0)
        {
            
            List<Product> books = (from s in db.Products select s).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "5", Value = "5" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "20", Value = "20" });
            items.Add(new SelectListItem { Text = "25", Value = "25" });
            items.Add(new SelectListItem { Text = "50", Value = "50" });
            items.Add(new SelectListItem { Text = "100", Value = "100" });
            items.Add(new SelectListItem { Text = "200", Value = "200" });
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.Size = items;
            ViewBag.CurrentSize = size;
            page = page ?? 1;
            int pageSize = (size ?? 5);
            ViewBag.pageSize = pageSize;
            int pageNumber = (page ?? 1);
            int checkTotal = (int)(books.ToList().Count / pageSize) + 1;
            if (pageNumber > checkTotal) pageNumber = checkTotal;
            return View(books.OrderBy(n => n.ProductID).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Brows(int id)
        {
            var account = db.Products.SingleOrDefault(n => n.ProductID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                if(account.Status == 0)
                {
                    account.Status = int.Parse("1");
                }
                else if(account.Status ==1)
                {
                    account.Status = int.Parse("0");
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
    }
    
}