using APILibrary.Core.Extensions;
using APILibrary.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APILibrary
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerBaseUser<TModel, TContext> : ControllerBase where TModel : UserBase where TContext : DbContext
    {
        protected readonly DbContext _context;
        public ControllerBaseUser(DbContext context)
        {
            this._context = context;
        }

        public UserBase getUserById(int id)
        {
            return _context.Set<TModel>().FirstOrDefault(x => x.ID == id);
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(TModel user)
        {
            var query = _context.Set<TModel>().AsQueryable();
            var result = query.SingleOrDefault(x => x.Username == user.Username && x.Password == user.Password);
            
            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = AuthenticationExtension.GenerateJwtToken(user);

            return Ok(new
            {
                ID = user.ID,
                Username = user.Username,
                Token = token
            }); ;
        }
    }
}
