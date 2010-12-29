using System;
using NRack.Adapters;

namespace NRack.Auth.Abstract
{
    public abstract class HandlerBase
    {
        protected HandlerBase(dynamic app, string realm, Action authenticator)
        {
            App = app;
            Realm = realm;
            Authenticator = authenticator;
        }

        protected dynamic App { get; set; }
        public string Realm { get; protected set; }
        protected Action Authenticator { get; set; }

        protected dynamic[] Unauthorized(string wwwAuthenticate)
        {
            return new dynamic[] {401, new Headers{
                {"Content-Type", "text/plain"}, 
                {"Content-Length", "0"}, 
                {"WWW-Authenticate", wwwAuthenticate}},
            new string[0]};
        }

        protected dynamic[] BadRequest()
        {
            return new dynamic[] {400, new Headers{{"Content-Type", "text/plain"}, {"Content-Length", "0"}}, new string[0]};
        }
    }
}