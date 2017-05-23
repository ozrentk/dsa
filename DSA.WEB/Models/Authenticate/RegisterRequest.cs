using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TypeLite;

namespace DSA.WEB.Models
{
    [TsClass]
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Password2 { get; set; }
    }
}