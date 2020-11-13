using System;
using System.ComponentModel.DataAnnotations;
using APILibrary.Core.Model;

namespace WebApplication.Model
{
    public class Customer : ModelBase
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Address { get; set; }

        public string ZipCode { get; set; }

        public string City { get; set; }   
    }
}
