using System;
using APILibrary.Core.Model;

namespace WebApplication.Model
{
    public class Pizza : ModelBase
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Topping { get; set; }
    }
}
