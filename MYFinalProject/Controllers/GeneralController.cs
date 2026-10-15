using MYFinalProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

using System.Web.Mvc;

namespace MYFinalProject.Controllers
{
    public class GeneralController : Controller
    {
        // GET: General
        DBManager db = new DBManager();
        public ActionResult Index()
        {
            //select top 10 category
            DataTable cat = db.ExecuteSelect("select top 10 * from tbl_category");
            ViewBag.cat = cat;
            //select top 6 products
            DataTable p = db.ExecuteSelect($"select top 6 * from tbl_product left join tbl_cart on id=productid and email='{Session["emailid"]}' order by id desc");
            ViewBag.p = p;
            return View();
        }
        public ActionResult Pdetails()
        {
            return View();
        }
        public ActionResult Products(int? catid)
        {
            //select top 10 category
            DataTable cat = db.ExecuteSelect("select * from tbl_category");
            ViewBag.cat = cat;
            //select top 6 products
            DataTable p = db.ExecuteSelect($"select * from tbl_product left join tbl_cart on id=productid and email='{Session["emailid"]}' order by id desc");
            if(catid.HasValue)
            {
                p = db.ExecuteSelect($"select * from tbl_product left join tbl_cart on id=productid and email='{Session["emailid"]}' where catid={catid} order by id desc");
            }
            ViewBag.p = p;
            return View();
        }
        public ActionResult Cart(int? cartid)
        {
            //delete item from cart
            if (cartid.HasValue)
            {
                db.ExecuteSQL($"delete from tbl_cart where cartid={cartid}");
                //select total items of cart
                DataTable dt1 = db.ExecuteSelect($"select sum(qty) from tbl_cart where email='{Session["emailid"]}'");
                Session["cartitem"] = dt1.Rows[0][0];
            }
            if (Session["emailid"] == null)
            {
                TempData["msg"] = "Login first to access this page";
                return RedirectToAction("login");
            }
            string email = Session["emailid"].ToString();
            DataTable dt = db.ExecuteSelect($"select * from tbl_cart where email='{email}'");
            ViewBag.dt = dt;

            //select all address of user
            DataTable add = db.ExecuteSelect($"select * from tbl_address where email='{email}'");
            ViewBag.add = add;
            return View();
        }

        [HttpPost]
        public ActionResult Cart(int? address, int? total)
        {
            //Add data into tbl_order , tbl_orderitem , delete data from tbl_cart
            Random r = new Random();
            int orderid = r.Next(100000, 999999);
            string email = Session["emailid"].ToString();
            if (address.HasValue && total.HasValue)
            {
                db.ExecuteSQL($"insert into tbl_order values({orderid}, '{email}' ,{address} ,{total} ,'Cash on Delivery' ,'Success' , '{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}' , null , null, null, 'Ordered' , '{DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd hh:mm:ss")}')");
                db.ExecuteSQL($"insert into tbl_orderitem select {orderid},* from tbl_cart where email='{email}'");
                db.ExecuteSQL($"delete from tbl_cart where email='{email}'");
                Session["cartitem"] = 0;

                return Content("<script>alert('Order Placed Successfully. You can check status from My Orders section');location.href='/general/myorders'</script>");
            }
            else
            {
                return Content("<script>alert('Address Id or Total bill not found . Please Try Again.');location.href='/general/cart'</script>");
            }
        }


        public ActionResult SignUp()
        {           
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(string name,string email, string mobile , string gender)
        {
            int res = db.ExecuteSQL($"insert into tbl_user values ('{name}' ,{mobile}, '{email}' , '{gender}', '{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}' ,1)");
            if (res > 0)
            {
                TempData["msg"] = "You are registered successfully. Now you can start ordering";
            }
            else
            {
                TempData["msg"] = "Data not Saved due to Some Error";
            }
            return RedirectToAction("SignUp");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string email)
        {
            //check this email in tbl_user
            DataTable dt = db.ExecuteSelect($"select * from tbl_user where email='{email}'");
            if (dt.Rows.Count > 0)
            {
                Session["name"] = dt.Rows[0][0];
                Session["email"] = email;
                //generate OTP
                Random r = new Random();
                int otp = r.Next(100000, 999999);
                Session["otp"] = otp;
                //compose mail to send
                string mail = $"Your OTP to login on QuickBasket is {otp}";
                //send mail
                SendEmail(email,"Login OTP from QuickBasket",mail);
                return RedirectToAction("checkotp");
            }
            else
            {
                TempData["msg"] = "This Email is not registered with us.";
                return RedirectToAction("Login");
            }             
        }
        public ActionResult CheckOTP()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckOTP(int otp)
        {
            if (Session["otp"]!=null)
            {
                if (otp == Convert.ToInt32(Session["otp"]))
                {
                    TempData["msg"] = "OTP Matched. Login Successful. ";
                    Session["emailid"] = Session["email"];
                    //select total items of cart
                    DataTable dt = db.ExecuteSelect($"select sum(qty) from tbl_cart where email='{Session["emailid"]}'");
                    Session["cartitem"] = dt.Rows[0][0];
                    return RedirectToAction("index");
                }
                else
                {
                    TempData["msg"] = "OTP not Matched.Try Again. ";
                }
            }
            else
            {
                TempData["msg"] = "Kindly goto login and then check OTP ";
            }
                return View();
        }
        public ActionResult MyOrders()
        {
            if (Session["emailid"]==null)
            {
                TempData["msg"] = "Login first to access this page";
                return RedirectToAction("login");
            }
            string email = Session["emailid"].ToString();
            DataTable dt = db.ExecuteSelect($"select * from tbl_order where email='{email}' order by order_on desc");
            ViewBag.dt = dt;
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Address()
        {
            if (Session["emailid"] == null)
            {
                TempData["msg"] = "Login first to access this page";
                return RedirectToAction("login");
            }
            string email = Session["emailid"].ToString();
            DataTable dt = db.ExecuteSelect($"select * from tbl_address where email='{email}'");
            ViewBag.add = dt;
            return View();
        }
        [HttpPost]
        public ActionResult Address(string Address,string City,string Pincode,string State,string Landmark,string Addresstype,string Mobile)
        {
            if (Session["emailid"]==null)
            {
                TempData["msg"] = "Login First to access this page";
                return RedirectToAction("login");
            }
            string email = Session["emailid"].ToString();
            int res = db.ExecuteSQL($"insert into tbl_address values ('{email}','{Address}','{City}','{Pincode}','{State}','{Landmark}','{Addresstype}','{DateTime.Now.ToString()}','{Mobile}')");
            if (res > 0)
            {
                return Content("<script>alert('Address Saved Successfully !');location.href='/general/address';<script>");
            }
            else
            {
                return Content("<script>alert('Address Not Saved!');location.href='/general/address';<script>");
            }
        }
        public ActionResult deleteaddress(int id)
        {
            int res = db.ExecuteSQL($"delete from tbl_address where id={id}");
            if (res > 0)
            {
                return Content("<script> alert ('Address Deleted Successfully !');location.href='/general/address'; <script>");
            }
            else
            {
                return Content("<script>alert('Address Not Deleted!');location.href='/general/address';<script>");
            }
        }
        public ActionResult Addcart()
        {
            if (Session["emailid"]==null)
            {
                TempData["msg"] = "Login first to access this page";
                return RedirectToAction("login");

            }
            return View();

        }
        public ActionResult FAQs()
        {
            return View();
        }
        public ActionResult ChangePassword()
        {
            if (Session["emailid"] == null)
            {
                TempData["msg"] = "Login first to access this page";
                return RedirectToAction("login");
            }
            return View();
        }
        public ActionResult Developer()
        {
            return View();
        }

       
      

        [NonAction]
        public void SendEmail(string receiver, string subject, string body)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress("ankit2021pandey@gmail.com");
                message.To.Add(receiver);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new NetworkCredential(
                        "ankit2021pandey@gmail.com",
                        "xalt pvvp akev jvra"
                    );

                     smtp.Send(message);
                }
            }
        }







    public ActionResult Logout()
        {         
            Session.RemoveAll(); //delete all active session
            TempData["msg"] = "Logout Successfully. You can login again here.";
            return RedirectToAction("login");
        }

        public ActionResult addtocart(int pid, int qty)
        {
            if (Session["emailid"] == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            string email = Session["emailid"].ToString();
            //select product already in cart or not
            DataTable dt = db.ExecuteSelect($"select * from tbl_cart where email='{email}' and productid={pid}");
            if(dt.Rows.Count>0)
            {
                int res = db.ExecuteSQL($"update tbl_cart set qty={qty},totalprice=price*{qty} where email='{email}' and productid={pid}");
            }
            else
            {
                int res = db.ExecuteSQL($"insert into tbl_cart select '{email}',{pid},name,image,mrp,salerate,discount,{qty},salerate*{qty},getdate() from tbl_product where id={pid}");
            }
            DataTable dt1 = db.ExecuteSelect($"select sum(qty) from tbl_cart where email='{Session["emailid"]}'");
            Session["cartitem"] = dt.Rows[0][0];
            return Json(dt1.Rows[0][0], JsonRequestBehavior.AllowGet);
        }

        public ActionResult orderproducts(int? oid)
        {
            if(oid.HasValue)
            {
                DataTable dt = db.ExecuteSelect($"select * from tbl_orderitem where orderid={oid}");
                ViewBag.dt = dt;
            }
            return View();
        }
    }
}