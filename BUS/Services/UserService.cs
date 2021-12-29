using BUS.Interfaces;
using BUS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS.Services
{
    public class UserService : IUserService
    {
        public UserService() { }
        public bool Register(User user)
        {
            try
            {
                using (var db = new SocketContext())
                {
                    var u = db.Users.FirstOrDefault(x => x.UserName == user.UserName);
                    if (u != null)
                    {
                        return false;
                    }
                    db.Users.Add(user);
                    if (db.SaveChanges() > 0)
                    {
                        return true;
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }

        public bool Login(User user)
        {
            try
            {
                using (var db = new SocketContext())
                {
                    var u = db.Users.FirstOrDefault(x => x.UserName == user.UserName && x.Password == user.Password);
                    if (u != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }

    }
}
