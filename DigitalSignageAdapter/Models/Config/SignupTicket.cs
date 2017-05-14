using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class SignupTicket
    {
        public DateTime? Created { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        [Display(Name="Ticket number")]
        public Guid Guid { get; set; }
        public List<Business> Businesses { get; set; }
        public bool IsFailed { get; internal set; }
    }
}