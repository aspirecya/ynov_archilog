using APILibrary.Core.Attributes;

namespace WebApi.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        [NotJson]
        public string Password { get; set; }
    }
}