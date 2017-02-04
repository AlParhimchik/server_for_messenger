using funApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace funApp.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public bool NewMessage(int senderID, int receiverID, string Text)
        {
            using (var db = new MessengerContext())
            {
                var sender = from u in db.Users where u.Id == senderID select u;
                var receiver = from u in db.Users where u.Id == receiverID select u;
                var sender_user = sender.FirstOrDefault();
                var receiver_user = receiver.FirstOrDefault();
                if (sender_user != null && receiver_user != null)
                {
                    
                    var mes = new Mail { receiver = receiver_user, sender = sender_user, Text = Text, Time = DateTime.Now };
                    db.Mails.Add(mes);
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public bool addNewUser(string FirstName, string login, string password, string LastName)
        {
            using (var db = new MessengerContext())
            {
                var users = from u in db.Users where u.Login == login select u;
                User user_ = users.FirstOrDefault();
                if (user_ == null)
                {
                    User user = new User { FirstName = FirstName, LastName = LastName, Login = login, Password = password };
                    db.Users.Add(user);
                    int i = db.SaveChanges();
                    if (i == 1) return true;
                }
            }
            return false;
        }
        public bool SingIn(string login, string password)
        {
            using (var db = new MessengerContext())
            {
                var users = from u in db.Users where u.Login == login && u.Password == password select u;
                User user = users.FirstOrDefault();
                if (user != null) return true;

            }
            return false;
        }
        public bool DeleteMessage(int mailID)
        {
            using (var db = new MessengerContext())
            {
                var mails = from u in db.Mails where u.Id == mailID select u;
                Mail mail = mails.FirstOrDefault();
                db.Mails.Remove(mail);
                int i = db.SaveChanges();
                if (i == 1) return true;
            }
            return false;
        }
        public ActionResult Index()
        {

            return View();
        }
        public bool DeleteUser(int userID,string password)
        {
            using (var db = new MessengerContext())
            {
                //db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Users]");
                var users = from u in db.Users where u.Id == userID && u.Password==password select u;
                User user = users.FirstOrDefault();
                if (user!=null)
                {
                    string str = string.Format("DELETE FROM Users WHERE Id = {0}", userID);
                    db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=0");
                    db.Users.Remove(user);
                    db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=1");
                    //db.Users.Remove(user);
                    int i = db.SaveChanges();
                    if (i == 1) return true;
                }
            }
            return false;
        }
        public JsonResult selectUsers(int userID=0)
        {
            using (var db = new MessengerContext())
            {
                var items = (from u in db.Users where u.Id > userID select u).ToList();
                JsonSerializerSettings jsSettings = new JsonSerializerSettings();
                jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                
                return Json(items, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult selectMails(int mailID=0)
        {
            using (var db = new MessengerContext())
            {
                var items = (from u in db.Mails where u.Id > mailID select u).ToList();
                JsonSerializerSettings jsSettings = new JsonSerializerSettings();
                jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                return Json(items, JsonRequestBehavior.AllowGet);
            }
        }
 
    }
}
