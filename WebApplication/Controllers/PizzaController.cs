using APILibrary;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Data;
using WebApplication.Model;

namespace WebApplication.Controllers
{
    public class PizzaController : ControllerBaseAPI<Pizza, EatDbContext>
    {
        public PizzaController(EatDbContext context) : base(context) {}
    }
}
