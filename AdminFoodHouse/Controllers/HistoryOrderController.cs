using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using AdminFoodHouse.Models;


namespace Food.Controllers
{
    public class HistoryOrderController : Controller
    {
        // GET: HistoryOrder
        FoodStoreEntities1 db = new FoodStoreEntities1();
        public ActionResult ManageOrder()
        {
            AccountBuy ac = (AccountBuy)Session["TaiKhoan"];
            var orders = from o in db.Orders
                         where o.AccountID == ac.AccountBuyID
                         select o;
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var order in orders)
            {
                var details = from d in db.OrderDetails
                              where d.OrderID == order.OrderID
                              select d;
                orderDetails.AddRange(details);
            }
            return View(orderDetails);
        }


        public ActionResult DetailOrder(int id)
        {
            var mn = from s in db.OrderDetails
                     where s.OrderDetailID == id
                     select s;
            return View(mn.Single());
        }
        public ActionResult Edit(int id, FormCollection f)
        {
            OrderDetail od = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
            if (od != null)
            {
                od.CustomerName = f["name"];
                od.Address = f["address"];
                od.PhoneNumber = f["sdt"];
                od.Email = f["email"];
                db.SaveChanges();
            }
            return RedirectToAction("ManageOrder", "HistoryOrder");
        }
    }
}