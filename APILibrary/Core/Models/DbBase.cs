using Microsoft.EntityFrameworkCore;
using APILibrary.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace APILibrary.Core.Models
{
    public class DbBase : DbContext
    {
        public DbBase(DbContextOptions options) : base(options)
        {

        }

        public DbSet<UserBase> Users { get; set; }
    }
}
