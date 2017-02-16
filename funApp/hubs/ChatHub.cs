
using funApp.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace funApp.hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> FromUsers = new ConcurrentDictionary<string, string>();         // <connectionId, userName>
        private static ConcurrentDictionary<string, string> ToUsers = new ConcurrentDictionary<string, string>();           // <userName, connectionId>
        private string userName = "";

        public override Task OnConnected()
        {
            DoConnect();
            //Clients.AllExcept(Context.ConnectionId).broadcastMessage(new ChatMessage() { UserName = userName, Message = "I'm Online" });
            return base.OnConnected();
        }
        public ConcurrentDictionary<string, string>  ReturnUsers()
        {

            return ToUsers;
         
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled) // Client explicitly closed the connection
            {
                string id = Context.ConnectionId;
                FromUsers.TryRemove(id, out userName);
                ToUsers.TryRemove(userName, out id);
                //Clients.AllExcept(Context.ConnectionId).broadcastMessage(new ChatMessage() { UserName = userName, Message = "I'm Offline" });
            }
            else // Client timed out
            {
                // Do nothing here...
                // FromUsers.TryGetValue(Context.ConnectionId, out userName);            
                // Clients.AllExcept(Context.ConnectionId).broadcastMessage(new ChatMessage() { UserName = userName, Message = "I'm Offline By TimeOut"});                
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            DoConnect();
            return base.OnReconnected();
        }

        private void DoConnect()
        {
            userName = Context.Request.Headers["login"];
            if (userName == null || userName.Length == 0)
            {
                userName = Context.QueryString["login"]; // for javascript clients
            }
            FromUsers.TryAdd(Context.ConnectionId, userName);
            String oldId; // for case: disconnected from Client
            ToUsers.TryRemove(userName, out oldId);
            ToUsers.TryAdd(userName, Context.ConnectionId);
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
                    Clients.Caller.UpdateMessage(mes);
                    var User = FromUsers.Where(x => x.Value == receiver_user.Login);
                    Clients.User(User.FirstOrDefault().Key).UpdateMessage(mes);          
                    return true;
                }
            }
            return false;
        }

        public List<User> selectUsers(int userID = 0)
        {
            using (var db = new MessengerContext())
            {
                var items = (from u in db.Users where u.Id > userID select u).ToList();
                

                return items;
            }
        }
        public List<Mail> selectMails(string login,int mailID = 0)
        {
            using (var db = new MessengerContext())
            {
                var items = (from u in db.Mails where u.Id > mailID && (u.receiver.Login== login || u.sender.Login== login)   select u).ToList();


                return items;
            }
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
                    if (i == 1)
                    {
                        Clients.All.updateUsers(user);
                        return true;
                    }
                    
                }
            }
            return false;
        }
        public bool DeleteUser(int userID, string password)
        {
            using (var db = new MessengerContext())
            {
                //db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Users]");
                var users = from u in db.Users where u.Id == userID && u.Password == password select u;
                User user = users.FirstOrDefault();
                if (user != null)
                {
                    string str = string.Format("DELETE FROM Users WHERE Id = {0}", userID);
                    db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=0");
                    db.Users.Remove(user);
                    db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=1");
                    int i = db.SaveChanges();
                    if (i == 1)
                    {
                        Clients.All.UserDeleted(user);
                        return true;
                    }
                }
            }
            return false;
        }
        //public JsonResult selectUsers(int userID = 0)
        //{
        //    using (var db = new MessengerContext())
        //    {
        //        var items = (from u in db.Users where u.Id > userID select u).ToList();
        //        JsonSerializerSettings jsSettings = new JsonSerializerSettings();
        //        jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        //        return Json(items, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public JsonResult selectMails(int mailID = 0)
        //{
        //    using (var db = new MessengerContext())
        //    {
        //        var items = (from u in db.Mails where u.Id > mailID select u).ToList();
        //        JsonSerializerSettings jsSettings = new JsonSerializerSettings();
        //        jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        //        return JsonConvert.DeserializeObject(items);
        //    }
        //}
    }
}