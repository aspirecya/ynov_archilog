﻿using System;
using APILibrary.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication.Model;

namespace WebApplication.Data
{
    public class EatDbContext : DbBase
    {
        public EatDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Customer> Customers { get; set; }

    }
}
