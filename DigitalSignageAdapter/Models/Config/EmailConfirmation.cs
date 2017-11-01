using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class EmailConfirmation
    {
        public string ConfirmationUrl { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}