using AdapterDb;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DigitalSignageAdapter
{
    public class ExternalData
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExternalData));

        //private int _dtOffset = 0;
        private string _baseDataUrl = "https://secure.digitalsignage.com/LineAnalyticsOpen";

        public ExternalData()
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
        private IEnumerable<Models.Shared.DataItem> GetDataItems(JsonTextReader reader)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            List<string> headerTokens = new List<string>();
            List<string> valueTokens = new List<string>();
            bool headerWritten = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (headerWritten)
                        continue;

                    headerTokens.Add(reader.Value.ToString());
                }
                else if (
                    reader.TokenType == JsonToken.Boolean ||
                    reader.TokenType == JsonToken.Date ||
                    reader.TokenType == JsonToken.Float ||
                    reader.TokenType == JsonToken.Integer ||
                    reader.TokenType == JsonToken.String)
                {
                    valueTokens.Add(reader.Value.ToString());
                }
                else if (
                    reader.TokenType == JsonToken.Null)
                {
                    valueTokens.Add("");
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    headerTokens.Clear();
                    valueTokens.Clear();
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    if (!headerWritten)
                    {
                        headerWritten = true;
                    }

                    var newItem = new Models.Shared.DataItem()
                    {
                        // TODO: tryparse... log?
                        line_id = int.Parse(valueTokens[0]),
                        service_id = int.Parse(valueTokens[1]),
                        analytic_id = int.Parse(valueTokens[2]),
                        business_id = int.Parse(valueTokens[3]),
                        name = valueTokens[4],
                        queue_id = int.Parse(valueTokens[5]),
                        verification = valueTokens[6],
                        serviced = !String.IsNullOrEmpty(valueTokens[7]) ? (DateTime?)epoch.AddSeconds(int.Parse(valueTokens[7])) : null,
                        called = !String.IsNullOrEmpty(valueTokens[8]) ? (DateTime?)epoch.AddSeconds(int.Parse(valueTokens[8])) : null,
                        entered = !String.IsNullOrEmpty(valueTokens[9]) ? (DateTime?)epoch.AddSeconds(int.Parse(valueTokens[9])) : null
                    };

                    newItem.wait_time = (newItem.called.HasValue && newItem.entered.HasValue) ? newItem.called.Value - newItem.entered.Value : TimeSpan.Zero;
                    newItem.service_time = (newItem.serviced.HasValue && newItem.called.HasValue) ? newItem.serviced.Value - newItem.called.Value : TimeSpan.Zero;

                    yield return newItem;
                }
            }
        }

        public List<Models.Shared.DataItem> Fetch(int? line_id, int? business_id, int? days)
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
            if (line_id.HasValue)
                tokens.Add(String.Format("line_id={0}", line_id.Value));
            if (business_id.HasValue)
                tokens.Add(String.Format("business_id={0}", business_id.Value));
            if (days.HasValue)
                tokens.Add(String.Format("days={0}", days.Value));

            string getQuery = "?" + String.Join("&", tokens);
            log.DebugFormat("get data from {0}{1}", _baseDataUrl, getQuery);
            HttpResponseMessage response = client.GetAsync(getQuery).Result;  // Blocking call!

            if (response.IsSuccessStatusCode)
            {
                log.DebugFormat("get succeeded");

                // Parse the response body. Blocking!
                using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader streamReader = new StreamReader(responseStream))
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    dataItems = GetDataItems(reader).ToList();
                }
            }
            else
            {
                log.ErrorFormat("get failed: {0} {1}", response.StatusCode, response.ReasonPhrase);
            }

            return dataItems;
        }

    }
}