﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class User
    {
        public int? Id { get; set; }
        public string Email { get; set; }
        public List<Role> Roles { get; set; }
        public bool IsActive { get; set; }
    }
}