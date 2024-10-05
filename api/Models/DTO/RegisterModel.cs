using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class RegisterModel
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ContactInfo { get; set; } = null!;
        public string Role { get; set; } = UserRoles.Customer!;
    }
}