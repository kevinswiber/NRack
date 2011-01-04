using System;
using System.Collections.Generic;
using NRack.Adapters;

namespace NRack
{
    public class ShowExceptions : ICallable
    {
        private readonly dynamic _app;
        public ShowExceptions(dynamic app)
        {
            _app = app;
        }
        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
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
                    new Hash{{"Content-Type", "text/html"}, {"Content-Length", backtrace.ToString().Length.ToString()}}, 
                    backtrace
                };
            }
        }

        #endregion

        public IIterable Pretty(IDictionary<string, dynamic> environment, Exception exception)
        {
            //TODO: Actually make ShowExceptions pretty.
            return new IterableAdapter(exception.ToString());
        }
    }
}