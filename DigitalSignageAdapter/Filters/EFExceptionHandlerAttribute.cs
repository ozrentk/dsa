using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Web;

namespace DigitalSignageAdapter.Filters
{
    public class EFExceptionHandlerAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            if (filterContext.Exception is System.Data.Entity.Core.EntityException)
            {
                // Error number not documented ?!?!?!
                // 0x80131501 ?
                if (filterContext.Exception.Message.Equals("The underlying provider failed on Open."))
                {
                    // Try failover
                    using (NamedPipeClientStream namedPipeClient =
                        new NamedPipeClientStream(
                            ".",
                            "DSA_ls_test_lspipe",
                            PipeDirection.InOut))
                    {
                        try
                        {
                            namedPipeClient.Connect(200);
                        }
                        catch (System.TimeoutException ex)
                        {
                            log.Error("Nothing on the other end of the pipe!");
                        }
                        namedPipeClient.ReadMode = PipeTransmissionMode.Message;

                        var request = new object();
                        string requestJson = JsonConvert.SerializeObject(request);
                        //string responseJson = await WriteRequestReadResponse(namedPipeClient, requestJson);
                        byte[] messageBytes = Encoding.UTF8.GetBytes(requestJson);
                        namedPipeClient.Write(messageBytes, 0, messageBytes.Length);

                        //var response = JsonConvert.DeserializeObject<ServiceResponse>(responseJson);
                        //return response;
                    }
                }
                log.Error("Caught EF exception", filterContext.Exception);
            }
            base.OnException(filterContext);
        }
    }
}
