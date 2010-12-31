using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NRack.Adapters;
using Kayak;
using Owin;

namespace NRack.Hosting.Kayak
{
    public class KayakHandler
    {
        public void Run(dynamic app, Hash options)
        {
            var server =
                new KayakServer(new IPEndPoint((Dns.Resolve(options[ServerOptions.Host].ToString())).AddressList[0],
                                               options[ServerOptions.Port]));

            var pipe = server.Invoke(new OwinApplicationAdapter(app));

            Console.WriteLine("Kayak is running at http://" + server.ListenEndPoint.Address + ":" +
                              server.ListenEndPoint.Port);
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            if (pipe != null)
            {
                pipe.Dispose();
            }
        }
    }

    public class OwinApplicationAdapter : Owin.IApplication
    {
        private readonly dynamic _app;
        private readonly Func<IRequest, IResponse> _responder;

        public OwinApplicationAdapter(dynamic app)
        {
            _app = app;
            _responder = Respond;
        }

        #region Implementation of IApplication

        /// <summary>
        /// Begins the asynchronous process to get the <see cref="T:Owin.IResponse"/>.
        /// </summary>
        /// <param name="request">The request.</param><param name="callback">The callback.</param><param name="state">The state.</param>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that represents the asynchronous invocation.
        /// </returns>
        public IAsyncResult BeginInvoke(IRequest request, AsyncCallback callback, object state)
        {
            return _responder.BeginInvoke(request, callback, state);
        }

        /// <summary>
        /// Ends the asynchronous process to get the <see cref="T:Owin.IResponse"/>.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The <see cref="T:Owin.IResponse"/>.
        /// </returns>
        public IResponse EndInvoke(IAsyncResult result)
        {
            return _responder.EndInvoke(result);
        }

        #endregion

        private IResponse Respond(IRequest request)
        {
            return new OwinResponseAdapter(_app.Call(new Dictionary<string, dynamic>()));
        }
    }

    public class OwinResponseAdapter : IResponse
    {
        private readonly dynamic[] _response;

        public OwinResponseAdapter(dynamic[] response)
        {
            _response = response;
        }

        #region Implementation of IResponse

        /// <summary>
        /// Gets the body <see cref="!:IEnumerable<object>"/>.
        /// </summary>
        /// <returns>
        /// The response as an <see cref="!:IEnumerable<object>"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="!:IEnumerable<object>"/> is not guaranteed to be hot.
        ///             This method should be considered safe to generate either a cold or hot enumerable
        ///             so that it _could_ be called more than once, though the expectation is only one call.
        /// </remarks>
        public IEnumerable<object> GetBody()
        {
            string body = _response[2].ToString();
            return new[] {Encoding.ASCII.GetBytes(body)};
        }

        /// <summary>
        /// Gets the status code and description.
        /// </summary>
        /// <remarks>
        /// The string should follow the format of "200 OK".
        /// </remarks>
        public string Status
        {
            get { return _response[0].ToString(); }
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <remarks>
        /// Each header key may have one or more values matching the HTTP spec.
        ///             Example:
        ///               HTTP/1.1 200 OK
        ///               Set-Cookie: foo=bar
        ///               Set-Cookie: baz=quux
        ///               Generates a headers dictionary with key "Set-Cookie" containing string ["foo=bar";"baz=quux"]
        /// </remarks>
        public IDictionary<string, IEnumerable<string>> Headers
        {
            get
            {
                var adapted = new Dictionary<string, IEnumerable<string>>();
                var headers = new HeaderHash(_response[1]);

                headers.Each(pair => adapted.Add(pair.Key, new string[] {pair.Value.ToString()}));

                return adapted;
            }
        }

        #endregion
    }
}