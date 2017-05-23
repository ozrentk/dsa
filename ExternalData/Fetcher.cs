using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ExternalData
{
    public class Fetcher
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Fetcher));

        private static string _baseDataUrl = "https://secure.digitalsignage.com/LineAnalyticsOpen";

        internal static List<DataItem> FetchItems(int businessCode, int lineCode, int days)
        {
            log.DebugFormat("Fetching external data items for ({0}, {1}, {2} days)", businessCode, lineCode, days);

            List<DataItem> dataItems = new List<DataItem>();

            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(_baseDataUrl)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //line_id=1552&business_id=423662&days=10
            List<string> tokens = new List<string>();
            tokens.Add(String.Format("line_id={0}", lineCode));
            tokens.Add(String.Format("business_id={0}", businessCode));
            tokens.Add(String.Format("days={0}", days));
            string getQuery = "?" + String.Join("&", tokens);
            log.DebugFormat("Sending HTTP GET request to {0}{1}", _baseDataUrl, getQuery);

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
                if (ex.CancellationToken.IsCancellationRequested)
                    log.Error("Timeout exceeded!", ex);
            }
            catch (Exception ex)
            {
                log.Error("Error!", ex);
            }

            if (success)
            {
                log.DebugFormat("HTTP GET succeeded!");

                // Parse the response body. Blocking!
                using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader streamReader = new StreamReader(responseStream))
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    dataItems = DataItemReader.ReadItems(reader).ToList();
                }
            }

            return dataItems;
        }
    }
}
