using BUS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS.Interfaces
{
    public interface IUserService
    {
        bool Register(User user);
        bool Login(User user);
    }
}
