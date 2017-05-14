using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DigitalSignageAdapter.DataSource
{
    public static class ExternalData
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExternalData));

        //private int _dtOffset = 0;
        private static string _baseDataUrl = "https://secure.digitalsignage.com/LineAnalyticsOpen";

        public static void Initialize()
        {
            var cfgBaseDataUrl = ConfigurationManager.AppSettings["my:externalDataUrl"];
            //var cfgDtOffset = ConfigurationManager.AppSettings["my:dateTimeOffset"];

            //if (cfgDtOffset != null)
            //    _dtOffset = int.Parse(cfgDtOffset);

            if (cfgBaseDataUrl != null)
                _baseDataUrl = cfgBaseDataUrl;

        }

        /// <summary>
        /// Take JSON reader as input and output collection of items
        /// </summary>
        private static IEnumerable<Models.Shared.DataItem> ReadDataItems(JsonTextReader reader)
        {
            int triedInt = 0;
            string propName = null;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Models.Shared.DataItem dataItem = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    dataItem = new Models.Shared.DataItem();
                }
                else if (reader.TokenType == JsonToken.PropertyName)
                {
                    propName = reader.Value.ToString();
                }
                else if (
                    reader.TokenType == JsonToken.Boolean ||
                    reader.TokenType == JsonToken.Date ||
                    reader.TokenType == JsonToken.Float ||
                    reader.TokenType == JsonToken.Integer ||
                    reader.TokenType == JsonToken.String ||
                    reader.TokenType == JsonToken.Null)
                {
                    // Any nulls won't be explicitly written as null, just proceed
                    if (reader.TokenType == JsonToken.Null)
                        continue;

                    var val = reader.Value;
                    switch (propName)
                    {
                        case "line_id":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast line_id='{0}' to int", val);
                            dataItem.LineId = triedInt;
                            break;
                        case "service_id":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast service_id='{0}' to int", val);
                            dataItem.ServiceId = triedInt;
                            break;
                        case "analytic_id":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast analytic_id='{0}' to int", val);
                            dataItem.AnalyticId = triedInt;
                            break;
                        case "called_by":
                            dataItem.CalledByName = val.ToString();
                            break;
                        case "serviced":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast serviced='{0}' to int", val);
                            dataItem.Serviced = epoch.AddSeconds(triedInt);
                            break;
                        case "serviced_by":
                            dataItem.ServicedByName = val.ToString();
                            break;
                        case "business_id":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast business_id='{0}' to int", val);
                            dataItem.BusinessId = triedInt;
                            break;
                        case "name":
                            dataItem.Name = val.ToString();
                            break;
                        case "queue_id":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast queue_id='{0}' to int", val);
                            dataItem.QueueId = triedInt;
                            break;
                        case "verification":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast verification='{0}' to int", val);
                            dataItem.Verification = triedInt;
                            break;
                        case "called":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast called='{0}' to int", val);
                            dataItem.Called = epoch.AddSeconds(triedInt);
                            break;
                        case "entered":
                            if (!int.TryParse(val.ToString(), out triedInt))
                                log.ErrorFormat("Can't cast entered='{0}' to int", val);
                            dataItem.Entered = epoch.AddSeconds(triedInt);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    yield return dataItem;
                }
            }
        }

        public static List<Models.Shared.DataItem> Fetch(int? businessId, int? lineId, int? days)
        {
            log.Debug("fetching external data...");

            List<Models.Shared.DataItem> dataItems = new List<Models.Shared.DataItem>();

            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(_baseDataUrl)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //line_id=1552&business_id=423662&days=10
            List<string> tokens = new List<string>();
            if (lineId.HasValue)
                tokens.Add(String.Format("line_id={0}", lineId.Value));
            if (businessId.HasValue)
                tokens.Add(String.Format("business_id={0}", businessId.Value));
            if (days.HasValue)
                tokens.Add(String.Format("days={0}", days.Value));

            string getQuery = "?" + String.Join("&", tokens);
            log.DebugFormat("get data from {0}{1}", _baseDataUrl, getQuery);

            bool success = false;
            HttpResponseMessage response = null;
            try
            {
                response = client.GetAsync(getQuery).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    success = true;
                }
                else
                {
                    log.ErrorFormat("Error {0}: {1}", response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (TaskCanceledException ex)
            {
                if(ex.CancellationToken.IsCancellationRequested)
                    log.Error("Timeout exceeded!", ex);
            }
            catch (Exception ex)
            {
                log.Error("Error!", ex);
            }

            if (success)
            {
                log.DebugFormat("get succeeded");

                // Parse the response body. Blocking!
                using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader streamReader = new StreamReader(responseStream))
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    dataItems = ReadDataItems(reader).ToList();
                }
            }

            return dataItems;
        }

    }
}