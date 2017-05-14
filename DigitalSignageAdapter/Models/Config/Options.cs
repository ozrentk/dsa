using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Options
    {
        [Display(Name = "Data collection schedule (CRON expression)")]
        public string DataCollectionCronSchedule { get; set; }

        [Display(Name = "Number of hours after an old and invalid entry gets removed")]
        public int CleanupTreshold { get; set; }

        public bool IsFailed { get; set; }
    }
}