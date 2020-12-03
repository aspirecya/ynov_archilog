using APILibrary.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace APILibrary.Core.Services
{
    public interface IUserService
    {
        UserBase GetById(int id);
    }

    public class UserService : IUserService
    {
        protected DbBase _context;

        public UserService(DbBase context)
        {
            _context = context;
        }

        public UserBase GetById(int id)
        {
            return _context.Users.Find(id);
        }
    }
}
