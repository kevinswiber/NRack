using System;
using System.Collections.Generic;
using NRack.Adapters;

namespace NRack
{
    public class ShowExceptions : IApplication
    {
        private readonly dynamic _app;
        public ShowExceptions(dynamic app)
        {
            _app = app;
        }
        #region Implementation of IApplication

        public dynamic[] Call(IDictionary<string, object> environment)
        {
            try
            {
                return _app.Call(environment);
            }
            catch (Exception exception)
            {
                var backtrace = Pretty(environment, exception);
                return new dynamic[] 
                {
                    500, 
                    new Headers{{"Content-Type", "text/html"}, {"Content-Length", backtrace.ToString().Length.ToString()}}, 
                    backtrace
                };
            }
        }

        #endregion

        public IResponseBody Pretty(IDictionary<string, object> environment, Exception exception)
        {
            return new EnumerableBodyAdapter(exception.ToString());
        }
    }
}