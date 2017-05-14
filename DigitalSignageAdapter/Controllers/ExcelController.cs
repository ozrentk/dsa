using AdapterDb;
using AutoMapper;
//using DigitalSignageAdapter.DataSource;
using DigitalSignageAdapter.Filters;
using DigitalSignageAdapter.Models.Excel;
using DigitalSignageAdapter.Models.Shared;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    public class ExcelController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExcelController));

        const string _spreadsheetmlNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private string _reportTemplate = "ReportTemplate.xlsx";
        private string _reportOutFile = "Report.xlsx";

        private Object _fileAccessLock = new Object();
 
        public ExcelController()
        {
            var cfgReportTemplate = ConfigurationManager.AppSettings["my:reportTemplate"];
            var cfgReportOutFile = ConfigurationManager.AppSettings["my:reportOutputFilename"];

            if (cfgReportTemplate != null)
                _reportTemplate = cfgReportTemplate;

            if (cfgReportOutFile != null)
                _reportOutFile = cfgReportOutFile;

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;
        }

        [Authorize]
        [TimeZoneActionFilter]
        public ActionResult Index()
        {
            var dbBusinessList = Database.GetBusinessLineList(User);
            var businessList = Mapper.Map<List<Models.Config.Business>>(dbBusinessList);

            /* BEGIN ADJUST TIMES */
            DateTime clientCurrentTime = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);
            DateTime clientTimeFrom = clientCurrentTime.Date;
            /* END ADJUST TIMES */

            var model = new DataFilters
            {
                TimeEntryType = TimeEntryType.Days,
                Days = 1,   
                From = clientTimeFrom,
                To = clientCurrentTime,
                BusinessList = businessList
            };

            return View(model);
        }

        /// <summary>
        /// Get data from external JSON result and output it as an Excel table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TimeZoneActionFilter]
        [Authorize]
        public ActionResult Index(DataFilters excelFilters)
        {
            /* BEGIN ADJUST TIMES */
            DateTime clientTimeFrom;
            DateTime clientTimeTo;
            if (excelFilters.TimeEntryType == TimeEntryType.Days)
            {
                clientTimeTo = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);

                if (excelFilters.Days > 0)
                    clientTimeFrom = clientTimeTo.AddDays(-excelFilters.Days);
                else
                    clientTimeFrom = clientTimeTo.Date;
            }
            else
            {
                clientTimeFrom = excelFilters.From;
                clientTimeTo = excelFilters.To;
            }
            /* END ADJUST TIMES */

            string xlsxRelPath = String.Format("/App_Data/{0}", _reportTemplate);
            string xlsxPath = System.Web.HttpContext.Current.Server.MapPath(xlsxRelPath);

            MemoryStream memoryStream = new MemoryStream();

            lock (_fileAccessLock)
            {
                FileStream fileStream = System.IO.File.Open(xlsxPath, FileMode.Open);

                using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(fileStream, true))
                {
                    var conn = spreadSheet.WorkbookPart.ConnectionsPart.Connections.FirstChild;
                    OpenXmlAttribute url = conn.FirstChild.GetAttribute("url", "");

                    string dataLinkUrl = String.Format("{0}://{1}{2}",
                                                HttpContext.Request.Url.Scheme,
                                                HttpContext.Request.Url.Authority,
                                                Url.Action("HtmlData", "Excel", new
                                                {
                                                    LineId = excelFilters.LineId,
                                                    BusinessId = excelFilters.BusinessId,
                                                    TimeEntryType = excelFilters.TimeEntryType,
                                                    Days = excelFilters.Days,
                                                    From = excelFilters.From.ToString("s"),
                                                    To = excelFilters.To.ToString("s"),
                                                    TimeZoneOffset = ViewBag.TimeZoneOffset
                                                }));

                    url.Value = dataLinkUrl;
                    conn.FirstChild.SetAttribute(url);

                    // *** BEGIN For saving template ONLY ***
                    //var nt = new NameTable();
                    //var nsmgr = new XmlNamespaceManager(nt);
                    //nsmgr.AddNamespace("sh", spreadsheetmlNamespace);
                    //var xdoc = new XmlDocument(nt);
                    ////xdoc.Load(spreadSheet.WorkbookPart.ConnectionsPart.GetStream());
                    //xdoc.Save(spreadSheet.WorkbookPart.ConnectionsPart.GetStream());
                    //spreadSheet.WorkbookPart.Workbook.Save();
                    // *** END For saving template ONLY ***

                    spreadSheet.Close();
                }

                fileStream.Position = 0;

                // Pre-fill sheet
                using (SLDocument sl = new SLDocument(fileStream))
                {
                    //List<Models.Shared.DataItem> items =
                    //    Merger.GetDataItems(
                    //        businessId: excelFilters.BusinessId,
                    //        lineId: excelFilters.LineId,
                    //        timeFrom: clientTimeFrom,
                    //        timeTo: clientTimeTo);

                    var dbItems =
                        Database.GetDataItems(
                            businessId: excelFilters.BusinessId,
                            lineId: excelFilters.LineId,
                            timeFrom: clientTimeFrom,
                            timeTo: clientTimeTo);

                    var items = Mapper.Map<List<AdapterDb.DataItem>, List<Models.Excel.DataItem>>(dbItems);

                    // Add total row
                    Models.Excel.DataItem aggregateTotal = new Models.Excel.DataItem();
                    if (dbItems.Count() > 0)
                    {
                        aggregateTotal = new Models.Excel.DataItem
                        {
                            WaitTime = new TimeSpan(0, 0, (int)(dbItems.Average(_ => _.WaitTimeSec) ?? 0)),
                            ServiceTime = new TimeSpan(0, 0, (int)(dbItems.Average(_ => _.ServiceTimeSec) ?? 0)),
                            Name = String.Format("Total: {0} customers", dbItems.Count())
                        };
                    }
                    items.Add(aggregateTotal);


                    SLStyle intStyle = sl.CreateStyle();
                    intStyle.FormatCode = "0";
                    intStyle.Alignment = new SLAlignment { Horizontal = HorizontalAlignmentValues.Right };

                    int startRow = 2;
                    for (int idx = 0; idx < items.Count; idx++)
                    {
                        sl.SetCellValueNumeric(idx + startRow, 1, string.Format("{0}", items[idx].LineId));
                        sl.SetCellValueNumeric(idx + startRow, 2, string.Format("{0}", items[idx].ServiceId));
                        sl.SetCellValueNumeric(idx + startRow, 3, string.Format("{0}", items[idx].AnalyticId));
                        sl.SetCellValueNumeric(idx + startRow, 4, string.Format("{0}", items[idx].BusinessId));
                        sl.SetCellValue(idx + startRow, 5, string.Format("{0}", items[idx].Name));
                        sl.SetCellValueNumeric(idx + startRow, 6, string.Format("{0}", items[idx].QueueId));
                        sl.SetCellValueNumeric(idx + startRow, 7, string.Format("{0}", items[idx].Verification));
                        sl.SetCellValue(idx + startRow, 8, string.Format("{0}", items[idx].Serviced));
                        sl.SetCellValue(idx + startRow, 9, string.Format("{0}", items[idx].ServicedByName));
                        sl.SetCellValue(idx + startRow, 10, string.Format("{0}", items[idx].Called));
                        sl.SetCellValue(idx + startRow, 11, string.Format("{0}", items[idx].CalledByName));
                        sl.SetCellValue(idx + startRow, 12, string.Format("{0}", items[idx].Entered));
                        sl.SetCellValue(idx + startRow, 13, string.Format("{0}", items[idx].WaitTime));
                        sl.SetCellValue(idx + startRow, 14, string.Format("{0}", items[idx].ServiceTime));

                        sl.SetCellStyle(idx + startRow, 1, intStyle);
                        sl.SetCellStyle(idx + startRow, 2, intStyle);
                        sl.SetCellStyle(idx + startRow, 3, intStyle);
                        sl.SetCellStyle(idx + startRow, 4, intStyle);
                        sl.SetCellStyle(idx + startRow, 6, intStyle);
                        sl.SetCellStyle(idx + startRow, 7, intStyle);
                    }

                    var defNames = sl.GetDefinedNames();
                    var name = defNames[0].Name;
                    var val = Regex.Replace(defNames[0].Text, @"\$[0-9]+", "");
                    sl.SetDefinedName(name, val);

                    sl.SaveAs(memoryStream);
                }
                fileStream.Close();
            }

            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _reportOutFile);
        }

        [Authorize]
        public ActionResult Lines(int businessId)
        {
            var dbLineList = Database.GetLineList(User, businessId);
            var lineList = Mapper.Map<List<Models.Config.Line>>(dbLineList);

            return PartialView(lineList);
        }

        /// <summary>
        /// Get data from external JSON result and output it as HTML table (for Excel refresh)
        /// </summary>
        public ActionResult HtmlData(DataFilters excelFilters, int timeZoneOffset = 0)
        {
            List<Models.Excel.DataItem> items = new List<Models.Excel.DataItem>();

            try
            {
                /* BEGIN ADJUST TIMES */
                DateTime clientTimeFrom;
                DateTime clientTimeTo;
                if (excelFilters.TimeEntryType == TimeEntryType.Days)
                {
                    clientTimeTo = TimeZoneHelper.ClientCurrentTime(timeZoneOffset);
                    if (excelFilters.Days > 0)
                        clientTimeFrom = clientTimeTo.AddDays(-excelFilters.Days);
                    else
                        clientTimeFrom = clientTimeTo.Date;
                }
                else
                {
                    clientTimeFrom = excelFilters.From;
                    clientTimeTo = excelFilters.To;
                }
                /* END ADJUST TIMES */


                //items = 
                //    Merger.GetDataItems(
                //        businessId: excelFilters.BusinessId,
                //        lineId: excelFilters.LineId,
                //        timeFrom: clientTimeFrom,
                //        timeTo: clientTimeTo);
                var dbItems =
                    Database.GetDataItems(
                        businessId: excelFilters.BusinessId,
                        lineId: excelFilters.LineId,
                        timeFrom: clientTimeFrom,
                        timeTo: clientTimeTo);

                items = Mapper.Map<List<AdapterDb.DataItem>, List<Models.Excel.DataItem>>(dbItems);

            }
            catch (Exception ex)
            {
                var eatenEx = ex;
                log.Error("problem while merging data items", ex);
            }

            return View(items);
        }

    }
}