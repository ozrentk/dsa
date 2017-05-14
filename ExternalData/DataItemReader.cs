using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalData
{
    public class DataItemReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataItemReader));

        /// <summary>
        /// Take JSON reader as input and output collection of items
        /// </summary>
        internal static IEnumerable<DataItem> ReadItems(JsonTextReader reader)
        {
            int triedInt = 0;
            string propName = null;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            DataItem dataItem = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    dataItem = new DataItem();
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
                            dataItem.Entered = epoch.AddSeconds(triedInt).AddHours(2).AddMinutes(-4).AddSeconds(-18); // * FIXED BUG IN INCOMING DATA */
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    yield return dataItem;
                }
            }
        }
    }
}
