//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdapterDb
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmployeeStats
    {
        public Nullable<System.DateTime> Created { get; set; }
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int LineId { get; set; }
        public int EmployeeId { get; set; }
        public int EnteredYear { get; set; }
        public int EnteredMonth { get; set; }
        public int WaitTimeSec { get; set; }
        public int ServiceTimeSec { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}
