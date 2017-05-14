using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class AddBusinessToSignupTicket
    {
        public SignupTicket Ticket { get; set; }
        public List<Business> AllBusinessList { get; set; }
        public int? SelectedBusinessId { get; set; }
        public bool IsFailed { get; set; }
    }
}