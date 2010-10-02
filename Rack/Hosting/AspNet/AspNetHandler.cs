using System;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Rack.Hosting.AspNet
{
    public class AspNetHandler : IHttpHandler
    {
        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            var rawEnvironment = context.Request.Params;
            var environment = rawEnvironment.AllKeys.ToDictionary(key => key, key => rawEnvironment[key]);

            var stopWatch = new Stopwatch();
            stopWatch.Reset();
            stopWatch.Start();

            var responseArray = AspNetHttpModule.Builder.Call(environment);

            stopWatch.Stop();

            Debug.WriteLine(stopWatch.ElapsedMilliseconds);

            var response = AspNetResponse.Create(responseArray);

            context.Response.StatusCode = response.StatusCode;

            //context.Response.Headers.Add(response.Headers);
            if (response.Headers != null)
            {
                foreach (var key in response.Headers.AllKeys)
                {
                    context.Response.AddHeader(key, response.Headers[key]);
                }
            }

            if (response.Body is string)
            {
                context.Response.Write(response.Body);
            }
            else
            {
                response.Body.Each((Action<object>)(body => context.Response.Write(body)));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion
    }
}