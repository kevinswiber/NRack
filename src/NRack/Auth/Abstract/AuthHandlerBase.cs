using System;
using NRack.Helpers;

namespace NRack.Auth.Abstract
{
    public abstract class AuthHandlerBase
    {
        protected AuthHandlerBase(dynamic app, string realm, Func<string, string, bool> authenticator)
        {
            App = app;
            Realm = realm;
            Authenticator = authenticator;
        }

        protected dynamic App { get; set; }
        public string Realm { get; set; }
        protected Func<string, string, bool> Authenticator { get; set; }

        protected dynamic[] Unauthorized(string wwwAuthenticate)
        {
            if (wwwAuthenticate == null)
            {
                wwwAuthenticate = Challenge;
            }

            return new dynamic[] {401, new Hash{
                {"Content-Type", "text/plain"}, 
                {"Content-Length", "0"}, 
                {"WWW-Authenticate", wwwAuthenticate}},
            new string[0]};
        }

        protected dynamic[] BadRequest()
        {
            return new dynamic[] {400, new Hash{{"Content-Type", "text/plain"}, {"Content-Length", "0"}}, new string[0]};
        }

        protected abstract string Challenge { get; }
    }
}