using AutoMapper;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DigitalSignageAdapter
{
    public static class AutomapperConfig
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        public static void Configure()
        {
            ConfigureDataItemMapping();
        }

        public static void ConfigureDataItemMapping()
        {
            log.DebugFormat("configuring automapper");

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<AdapterDb.Overview, Models.Config.Overview>();

                cfg.CreateMap<AdapterDb.User, Models.Home.User>()
                   .ForMember(d => d.Name, opt => opt.MapFrom(s => s.UserName));

                cfg.CreateMap<AdapterDb.Roles, Models.Home.Role>();

                cfg.CreateMap<AdapterDb.Business, Models.Home.Business>()
                   .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Line));

                cfg.CreateMap<Models.Home.Business, AdapterDb.Business>()
                   .ForMember(d => d.Line, opt => opt.MapFrom(s => s.Lines));

                cfg.CreateMap<AdapterDb.Line, Models.Home.Line>();

                cfg.CreateMap<Models.Home.Line, AdapterDb.Line>();

                cfg.CreateMap<AdapterDb.Employee, Models.Shared.Employee>();

                cfg.CreateMap<Models.Shared.Employee, AdapterDb.Employee>();

                cfg.CreateMap<List<AdapterDb.ConfigItem>, Models.Config.Options>()
                   //.ForMember(d => d.TimeOffsetHours, opt => opt.MapFrom(s => s.Where(i => i.Name == "TimeOffsetHours").First().Value))
                   .ForMember(d => d.DataCollectionCronSchedule, opt => opt.MapFrom(s => s.First(i => i.Name == "DataCollectionCronSchedule").Value))
                   .ForMember(d => d.CleanupTreshold, opt => opt.MapFrom(s => s.First(i => i.Name == "CleanupTreshold").Value));

                cfg.CreateMap<Models.Config.Options, List<AdapterDb.ConfigItem>>()
                    .AfterMap((s, d) =>
                    {
                        //d.Add(new AdapterDb.ConfigItem() { Name = "TimeOffsetHours", Value = s.TimeOffsetHours.ToString() });
                        d.Add(new AdapterDb.ConfigItem { Name = "DataCollectionCronSchedule", Value = s.DataCollectionCronSchedule });
                        d.Add(new AdapterDb.ConfigItem { Name = "CleanupTreshold", Value = s.CleanupTreshold.ToString() });
                    });

                cfg.CreateMap<AdapterDb.Employee, Models.Config.Employee>()
                    .ForMember(d => d.BusinessName, opt => opt.MapFrom(s => s.Business.Name))
                    .ReverseMap();

                cfg.CreateMap<AdapterDb.Line, Models.Config.Line>()
                    .ForMember(d => d.BusinessName, opt => opt.MapFrom(s => s.Business.Name))
                    .ReverseMap();

                cfg.CreateMap<AdapterDb.Business, Models.Config.Business>()
                    .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Line))
                    .ReverseMap();

                cfg.CreateMap<AdapterDb.UserSignupTicket, Models.Config.SignupTicket>()
                    .ForMember(d => d.Businesses, opt => opt.MapFrom(s => s.Business))
                    .ReverseMap();

                cfg.CreateMap<AdapterDb.DataItem, Models.Excel.DataItem>()
                    // BEGIN: special handling for null's, for total row in excel
                   .ForMember(s => s.BusinessId, opt => opt.MapFrom(d => d.BusinessId != default(int) ? (int?)d.Business.Code : null))
                   .ForMember(s => s.LineId, opt => opt.MapFrom(d => d.LineId != default(int) ? (int?)d.Line.Code : null))
                   .ForMember(s => s.ServiceId, opt => opt.MapFrom(d => d.ServiceId != default(int) ? (int?)d.ServiceId : null))
                   .ForMember(s => s.AnalyticId, opt => opt.MapFrom(d => d.AnalyticId != default(int) ? (int?)d.AnalyticId : null))
                   .ForMember(s => s.QueueId, opt => opt.MapFrom(d => d.QueueId != default(int) ? (int?)d.QueueId : null))
                   // END: special handling for null's, for total row in excel
                   .ForMember(s => s.ServicedById, opt => opt.MapFrom(d => d.ServicedById))
                   .ForMember(s => s.ServicedByName, opt => opt.MapFrom(d => d.ServicingEmployee != null ? d.ServicingEmployee.Name : ""))
                   .ForMember(s => s.CalledById, opt => opt.MapFrom(d => d.CalledById))
                   .ForMember(s => s.CalledByName, opt => opt.MapFrom(d => d.CallingEmployee != null ? d.CallingEmployee.Name : ""))
                   .ForMember(s => s.WaitTime, opt => opt.MapFrom(d => d.WaitTimeSec.HasValue ? new TimeSpan(0, 0, d.WaitTimeSec.Value) : new TimeSpan()))
                   .ForMember(s => s.ServiceTime, opt => opt.MapFrom(d => d.ServiceTimeSec.HasValue ? new TimeSpan(0, 0, d.ServiceTimeSec.Value) : new TimeSpan()));

                cfg.CreateMap<Models.Excel.DataItem, AdapterDb.DataItem>()
                   .ForMember(s => s.AnalyticId, opt => opt.MapFrom(d => d.AnalyticId))
                   .ForMember(s => s.BusinessId, opt => opt.MapFrom(d => d.BusinessId))
                   .ForMember(s => s.LineId, opt => opt.MapFrom(d => d.LineId))
                   .ForMember(s => s.QueueId, opt => opt.MapFrom(d => d.QueueId))
                   .ForMember(s => s.ServiceId, opt => opt.MapFrom(d => d.ServiceId))
                   .ForMember(s => s.ServicedById, opt => opt.MapFrom(d => d.ServicedById))
                   .ForMember(s => s.CalledById, opt => opt.MapFrom(d => d.CalledById))
                   .ForMember(s => s.WaitTimeSec, opt => opt.MapFrom(d => d.WaitTime.TotalSeconds))
                   .ForMember(s => s.ServiceTimeSec, opt => opt.MapFrom(d => d.ServiceTime.TotalSeconds));

                cfg.CreateMap<AdapterDb.EmployeeTimes, Models.Home.EmployeeTimes>()
                   .ForMember(d => d.EmployeeId, opt => opt.MapFrom(s => s.EmployeeId))
                   .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.EmployeeName))
                   .ForMember(d => d.AverageServiceTime, opt => opt.MapFrom(s =>
                        new TimeSpan(0, 0, s.DailyServiceTimeSec)))
                   .ForMember(d => d.MonthlyServiceTime, opt => opt.MapFrom(s =>
                        new TimeSpan(0, 0, s.MonthlyServiceTimeSec)))
                   .ForMember(d => d.YearlyServiceTime, opt => opt.MapFrom(s =>
                        new TimeSpan(0, 0, s.YearlyServiceTimeSec)));

                   //.ForMember(d => d.MonthlyServiceTime, opt => opt.MapFrom(s =>
                   //     (s.EmployeeStats != null && s.EmployeeStats.Count > 0) ?
                   //         new TimeSpan(0, 0, (int)s.EmployeeStats.Average(i => i.ServiceTimeSec)) :
                   //         new TimeSpan(0, 0, 0)))
                   //.ForMember(d => d.YearlyServiceTime, opt => opt.MapFrom(s =>
                   //     (s.EmployeeStats != null && s.EmployeeStats.Count > 0) ?
                   //         new TimeSpan(0, 0, (int)s.EmployeeStats.Average(i => i.ServiceTimeSec)) :
                   //         new TimeSpan(0, 0, 0)));
            });

        }
    }
}
