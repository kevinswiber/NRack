using System.Collections.Generic;
using System.Linq;

namespace NRack.Auth.Abstract
{
    public abstract class RequestBase
    {
        private static readonly string[] AuthorizationKeys =
            new[] { "HTTP_AUTHORIZATION", "X-HTTP_AUTHORIZATION", "X_HTTP_AUTHORIZATION" };

        private string _authorizationKey;
        private string[] _parts;

        protected RequestBase(IDictionary<string, object> environment)
        {
            Environment = environment;
        }

        protected IDictionary<string, object> Environment { get; set; }

        public bool IsProvided()
        {
            return AuthorizationKey != null;
        }

        public string[] Parts
        {
            get
            {
                return _parts ??
                       (_parts = ((string)Environment[AuthorizationKey]).Split(new[] {' '}, 2));
            }
        }

        private string AuthorizationKey
        {
            get {
                return _authorizationKey ??
                       (_authorizationKey = AuthorizationKeys.Where(key => Environment.ContainsKey(key)).First());
            }
        }
    }
}