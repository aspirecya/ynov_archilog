using WebApplication.Data;
using WebApplication.Model;
using APILibrary;

namespace WebApplication.Controllers
{
    public class UserController : ControllerBaseUser<User, EatDbContext>
    {
        public UserController(EatDbContext context) : base(context) { }
    }
}
