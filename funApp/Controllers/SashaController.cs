using funApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace funApp.Controllers
{
    public class SashaController : Controller
    {
        //
        // GET: /Sasha/

        public ActionResult Index()
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
                var user = from c in db.Users select c;
                User user1 = user.FirstOrDefault();
                if (user1 == null) ViewBag.mes = "no users";
                else ViewBag.mes = string.Format("{0} {1}", user1.Login, user1.Password);
                return View();
            }

        }

    }
}
