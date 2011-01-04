using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Auth.Abstract;

namespace NRack.Auth
{
    public class BasicAuthHandler : AuthHandlerBase, ICallable
    {
        public BasicAuthHandler(dynamic app, string realm, Func<string, string, bool> authenticator) 
            : base((object)app, realm, authenticator)
        {
        }

        public BasicAuthHandler(dynamic app, Func<string, string, bool> authenticator) 
            : base((object)app, null, authenticator)
        {
        }

        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var auth = new BasicAuthRequest(environment);

            if (!auth.IsProvided())
            {
                return Unauthorized(null);
            }

            if (!auth.IsBasic())
            {
                return BadRequest();
            }

            if (IsValid(auth))
            {
                environment["REMOTE_USER"] = auth.Username;

                return App.Call(environment);
            }

            return Unauthorized(null);
        }

        #endregion

        private bool IsValid(BasicAuthRequest auth)
        {
            return Authenticator.Invoke(auth.Username, auth.Credentials.Last());
        }

        #region Overrides of AuthHandlerBase

        protected override string Challenge
        {
            get { return "Basic realm=\"" + Realm + "\""; }
        }

        #endregion
    }

    public class BasicAuthRequest : AuthRequestBase
    {
        private IEnumerable<string> _credentials;

        public BasicAuthRequest(IDictionary<string, object> environment) : base(environment)
        {
        }

        public bool IsBasic()
        {
            return Scheme == "basic";
        }

        public IEnumerable<string> Credentials
        {
            get
            {
                return _credentials ??
                       (_credentials = Encoding.ASCII.GetString(Convert.FromBase64String(Params)).Split(new[] {':'}, 2));
            }
        }

        public string Username
        {
            get { return Credentials.First(); }
        }
    }
}