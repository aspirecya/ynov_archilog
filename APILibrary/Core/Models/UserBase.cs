using System;
using System.Collections.Generic;
using System.Text;

namespace APILibrary.Core.Models
{
    public class UserBase
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    } 
}
