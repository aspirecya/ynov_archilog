using APILibrary;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Data;
using WebApplication.Model;

namespace WebApplication.Controllers
{
    public class CustomerController : ControllerBaseAPI<Customer, EatDbContext>
    {
        public CustomerController(EatDbContext context) : base(context) { }
    }
}
