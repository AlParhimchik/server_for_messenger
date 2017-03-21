
using funApp.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
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
             return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {            
            using (var db = new MessengerContext())
            {
                var users = ToUsers.Where(x => x.Value == Context.ConnectionId).ToList();
                var user = users.First();
                try
                {
                    {
                        var login = user.Key;
                        var login_qout = string.Format(@"""{0}""", login);
                        var users_db = db.Users.SqlQuery("select * from  messengerbd.users where binary Login = " + login_qout).ToList();
                        var user_db = users_db.First();
                        if (user_db != null)
                        {
                            user_db.Online = false;
                            db.Users.Attach(user_db);
                            db.Entry(user_db).State = EntityState.Modified;
                            db.SaveChanges();
                            Clients.Others.isOffline(user_db);

                        }
                        else Clients.Caller.Ecxeption("no user found in db when disconect");
                    }
                }
                catch(Exception e)
                {
                    Clients.Caller.Ecxeption("no user found in ToUsers when delete");
                }                    
            }
            string id = Context.ConnectionId;
            Clients.Others.Offline("id "+id);
            FromUsers.TryRemove(id, out userName);
            ToUsers.TryRemove(userName, out id);           
        return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            DoConnect();
            return base.OnReconnected();
        }

        private void DoConnect()
        {
            string login = Context.Request.Headers["login"];
            string password = Context.Request.Headers["password"];
            User user=null;
            using (var db = new MessengerContext())
            {
                var login_qout = string.Format(@"""{0}""", login);
                var password_qout = string.Format(@"""{0}""", password);

                var users = db.Users.SqlQuery("select * from  messengerbd.users where binary Login = " + login_qout + "  and binary  Password = " + password_qout).ToList();
                user = users.FirstOrDefault();
                if (user != null)
                {
                    user.Online = true;
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                    if (db.SaveChanges() == 1)
                    {
                        var user_to_result = new User();
                        user_to_result = user;
                        user_to_result.Password = "";
                        user_to_result.Login = "";
                        Clients.Others.singedIn(user_to_result);
                        Clients.Caller.singIn(user);
                        FromUsers.TryAdd(Context.ConnectionId, userName);
                        ToUsers.TryAdd(userName, Context.ConnectionId);
                    }
                    else
                    {
                        Clients.Caller.Exception("error in db when change status");
                    }
                    
                }
                else
                {
                    string firstName = Context.Request.Headers["first_name"];
                    string LastName = Context.Request.Headers["last_name"];
                    if (firstName==null && LastName==null)
                    {
                        Clients.Caller.singIn(null);
                    }
                    addNewUser(firstName,login,password,LastName);
                    
                }
            }       
        }

        private string setChanges(string FirstName , string LastName , byte[] photo)
        {
            var id = Context.ConnectionId;
            var users = ToUsers.Where(x => x.Value == id).ToList();
            var user = users.First();
            try
            {
                using (var db = new MessengerContext())
                {
                    var login = user.Key;
                    var login_qout = string.Format(@"""{0}""", login);
                    var users_db = db.Users.SqlQuery("select * from  messengerbd.users where binary Login = " + login_qout).ToList();
                    var user_db = users_db.First();
                    if (user_db != null)
                    {
                        user_db.FirstName = FirstName;
                        user_db.LastName = LastName;
                        user_db.Image = photo;
                        db.Users.Attach(user_db);
                        db.Entry(user_db).State = EntityState.Modified;
                        if (db.SaveChanges() == 1)
                            return "ok";
                        else return "error in SaveChanges";

                    }
                    else
                    {
                        return "user is null";
                    }
                }
            }
            catch (Exception )
            {
                return "exception(no user in hub)";
            }
        }

         //public bool DeleteMessage(int mailID)
        //{
        //    using (var db = new MessengerContext())
        //    {
        //        var mails = from u in db.Mails where u.Id == mailID select u;
        //        Mail mail = mails.FirstOrDefault();
        //        db.Mails.Remove(mail);
        //        int i = db.SaveChanges();
        //        if (i == 1) return true;
        //    }
        //    return false;
        //}
        public Mail NewMessage(int senderID, int receiverID, string Text)
        {
            Mail message=null;
            using (var db = new MessengerContext())
            {
                var sender = from u in db.Users where u.Id == senderID select u;
                var receiver = from u in db.Users where u.Id == receiverID select u;
                var sender_user = sender.FirstOrDefault();
                var receiver_user = receiver.FirstOrDefault();
                if (sender_user != null && receiver_user != null)
                {
                    var belarus = TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
                    message = new Mail { receiver = receiver_user, sender = sender_user, Text = Text, Time =belarus};
                    db.Mails.Add(message);
                    db.SaveChanges();
                    //Clients.User(receiver_user.Login).UpdateMessage(message);
                    
                    try
                    {
                        var user = ToUsers.Where(x => x.Key == receiver_user.Login);
                        if (user == null)
                        {
                            Clients.Caller.noUser();
                        }
                        else
                        {
                            Clients.Client(user.First().Value).UpdateMessage(message);
                            Clients.User(receiver_user.Login).UpdateMessage(message);
                        }

                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return message;
        }        
        public ConcurrentDictionary<string, string> showToUsers()
        {
            return ToUsers;
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
        public User addNewUser(string FirstName, string login, string password, string LastName)
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
                        var user_to_result = new User();
                        user_to_result = user;
                        user_to_result.Password = "";
                        user_to_result.Login = "";
                        FromUsers.TryAdd(Context.ConnectionId, userName);
                        ToUsers.TryAdd(userName, Context.ConnectionId);
                        Clients.Others.updateUsers(user_to_result);
                        Clients.Caller.singed_Up(user);
                        return user;
                    }
                    else
                    {
                        Clients.Caller.Exception("error in db when add new user "+login);
                    }
  
                    
                }
                else
                {
                    Clients.Caller.Exception("error in db  (has already such user " + login);
                }
            }
            return null;
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
                        Clients.Others.UserDeleted(user);
                        return true;
                    }
                }
            }
            return false;
        }
        
    }
}