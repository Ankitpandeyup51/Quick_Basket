
using MYFinalProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MYFinalProject.Controllers
{
    public class AdminController : Controller
    {
        DBManager db = new DBManager();
       
        // GET: Admin
        public ActionResult Index()
        {
            DataTable dt = db.ExecuteSelect("select count(orderid) from tbl_order");
            ViewBag.dt = dt;

            //for count delivery
            DataTable dn = db.ExecuteSelect("select count(orderid) from tbl_order where cur_status='Delivered'");
            ViewBag.dn = dn;

            //for total category

            DataTable tc = db.ExecuteSelect("select count(id) from tbl_category");
            ViewBag.tc = tc;
            //For total Product
            DataTable p = db.ExecuteSelect("select count(id) from tbl_product");
            ViewBag.p = p;

            //for recent order
            DataTable ro = db.ExecuteSelect("select top 5 * from tbl_orderitem order by id desc");
            ViewBag.ro = ro;

            //for recent customer
            DataTable rc = db.ExecuteSelect("select top 5 * from tbl_user order by added_on desc");

            ViewBag.rc = rc;
            return View();
        }
        public ActionResult AddCategory()
        {
            //select all category
            DataTable dt = db.ExecuteSelect("select * from tbl_category");
            ViewBag.dt = dt;
            return View();
        }
        [HttpPost]
        public ActionResult AddCategory(string cname,HttpPostedFileBase cpic)
        {
          int res=  db.ExecuteSQL($"insert into tbl_category values('{cname}','{cpic.FileName}')");
        if(res>0)
            {
                cpic.SaveAs(Server.MapPath("/content/catpic/"+ cpic.FileName));
                TempData["msg"] = "Category Saved Successfully";
            }
        else
            {
                TempData["msg"] = "Data not Saved due to Some Error";
            }
                return RedirectToAction("AddCategory");
        }
        public ActionResult AddProduct()
        {
            //select all category
            DataTable dt = db.ExecuteSelect("select * from tbl_category");
            ViewBag.dt = dt;
            //select all products 
            DataTable p = db.ExecuteSelect("select * from tbl_product order by id desc");
            ViewBag.p = p;
            return View();
        }
        [HttpPost]
        public ActionResult AddProduct(string name, int category,int mrp,int discount , int salerate,string brand , string quantity, HttpPostedFileBase pic, DateTime expiry, string detail)
        {
            int res = db.ExecuteSQL($"insert into tbl_product values ({category}, '{name}',{mrp},{discount},{salerate},'{brand}', '{quantity}', '{detail}',1 , '{DateTime.Now.ToString("yyyy-MM-dd")}','{expiry.ToString("yyyy-MM-dd")}', '{pic.FileName}')");
            if(res>0)
            {
                pic.SaveAs(Server.MapPath("/content/productpic/"+pic.FileName));
                TempData["msg"] = "Product Saved Successfully";
            }
            else
            {
                TempData["msg"] = "Product not Saved. Try Again";
            }
                return RedirectToAction("AddProduct");
        }

        public ActionResult ManageCustomers()
        {
            //select all customers from tbl_user
            DataTable dt = db.ExecuteSelect("select * from tbl_user order by added_on desc");
            ViewBag.dt = dt;
            return View();
        }
        public ActionResult ManageOrders()
        {
            DataTable dt = db.ExecuteSelect("select * from tbl_order where cur_status !='Deliverd' order by order_on desc");
            ViewBag.dt = dt;
            return View();
        }
        public ActionResult UpdateStatus(int oid,string status)
        { 
            if(status=="Accepted")
            {
                int res = db.ExecuteSQL($"update tbl_order set accept_on=getdate(),cur_status='Accepted' where orderid={oid}");
                if(res>0)
                {
                    return Content("<script>alert('Order Accepted Successfully!');location.href='/admin/manageorders/';</script>");
                }
                else
                {
                    return Content("<script>alert('Order not Accepted');location.href='/admin/manageorders/';</script>");
                }
            }
            else if (status == "Out_for_delivery")
            {
                int res = db.ExecuteSQL($"update tbl_order set out_for_delivery=getdate(),cur_status='Out_for_delivery' where orderid={oid}'");
                if (res > 0)
                {
                    return Content("<script>alert('Order out for delivery Successfully!');location.href='/admin/manageorders/';</script>");
                }
                else
                {
                    return Content("<script>alert('Order not out for delivery');location.href='/admin/manageorders/';</script>");
                }
            }
            else if (status == "Delivered")
            {
                int res = db.ExecuteSQL($"update tbl_order set deliver_on=getdate(),cur_status='Delivered where orderid={oid}'");
                if (res > 0)
                {
                    return Content("<script>alert('Order Deliverd Successfully!');location.href='/admin/manageorders/';</script>");
                }
                else
                {
                    return Content("<script>alert('Order not Deliverd');location.href='/admin/manageorders/';</script>");
                }
            }
            else
            {

                return Content("<script>alert('No Changes !');location.href='/admin/manageorders/';</script>");
            }
          
        }
        public ActionResult ManageEnquiry()
        {
            return View();
        }

        public ActionResult AllOrders()
        {
            DataTable dt = db.ExecuteSelect("Select * from tbl_order order by order_on desc");
            ViewBag.dt = dt;
            return View();
        }
    }
}