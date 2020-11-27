using APILibrary.Core.Attributes;
using APILibrary.Core.Model;

namespace WebApi.Entities
{
    public class User : ModelBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        [NotJson]
        public string Password { get; set; }
    }
}