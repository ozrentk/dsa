using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Overview
    {
        [Display(Name = "Number of users")]
        public int UserCount { get; set; }
        [Display(Name = "Numer of issued subscription tickets")]
        public int SignupTicketCount { get; set; }
        [Display(Name = "Total number of businesses")]
        public int BusinessCount { get; set; }
        [Display(Name = "Total number of lines")]
        public int LineCount { get; set; }
        [Display(Name = "Number of cached lines")]
        public int CachedLineCount { get; set; }
        [Display(Name = "Total batches backed up")]
        public int BatchCount { get; set; }
        [Display(Name = "Total number of items")]
        public int DataItemCount { get; set; }
        [Display(Name = "Server UTC offset")]
        public int ServerUtcOffset { get; set; }
        [Display(Name = "Client (your) UTC offset")]
        public int ClientUtcOffset { get; set; }
    }
}