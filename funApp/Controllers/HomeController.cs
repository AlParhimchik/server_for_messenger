using funApp.Models;
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
        public bool NewMessage(Guid senderID, Guid receiverID, string Text)
        {
            using (var db = new MessengerContext())
            {
                var sender = from u in db.Users where u.Id == senderID select u;
                var receiver = from u in db.Users where u.Id == receiverID select u;
                var sender_user = sender.FirstOrDefault();
                var receiver_user = receiver.FirstOrDefault();
                if (sender_user != null && receiver_user != null)
                {
                    Guid mailID = Guid.NewGuid();
                    var mes = new Mail { Id = mailID, receiver = receiver_user, sender = sender_user, Text = Text, Time = DateTime.Now };
                    db.Mails.Add(mes);
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public bool addNewUser(string FirstName, string login, string password, string LastName, string PhoneNumber)
        {
            using (var db = new MessengerContext())
            {
                var users = from u in db.Users where u.Login == login select u;
                User user_ = users.FirstOrDefault();
                if (user_ == null)
                {
                    Guid id = Guid.NewGuid();
                    User user = new User { FirstName = FirstName, LastName = LastName, PhoneNumber = PhoneNumber, Login = login, Password = password, Id = id };
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
        public bool DeleteMessage(Guid mailID)
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
        public bool DeleteUser(Guid userID)
        {
            using (var db = new MessengerContext())
            {
                //db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Users]");
                var users = from u in db.Users where u.Id == userID select u;
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
        public ActionResult noIndex()
        {
            using (var db = new MessengerContext())
            {

                Guid receiverId = Guid.NewGuid();
                Guid senderId = Guid.NewGuid();
                Guid mailId = Guid.NewGuid();
                var receiver = new User { Id = receiverId, FirstName = "Sasha", LastName = "Pasrhimchik", PhoneNumber = "", Login = "123", Password = "123" };
                var sender = new User { Id = senderId, FirstName = "Sasha2", LastName = "Pasrhimchik", PhoneNumber = "", Login = "123", Password = "123" };
                var mail = new Mail { Id = mailId, Text = "hello", receiver = receiver, sender = sender, Time = DateTime.Now };
                db.Users.Add(sender);
                db.Users.Add(receiver);
                db.Mails.Add(mail);
                ViewBag.mes = db.Database.Connection.ConnectionString;
                int i = db.SaveChanges();

                ViewBag.i = db.SaveChanges();
                return View();
            }
        }
        public bool addUser(string login, string password, string first_name, string last_name, string phone)
        {
            return true;

        }
    }
}
