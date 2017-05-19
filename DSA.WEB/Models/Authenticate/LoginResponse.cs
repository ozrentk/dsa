using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TypeLite;

namespace DSA.WEB.Models
{
    [TsClass]
    public class LoginResponse
    {
        public string Jwt { get; set; }
    }
}