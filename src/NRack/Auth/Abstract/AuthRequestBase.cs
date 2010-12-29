using System.Collections.Generic;
using System.Linq;

namespace NRack.Auth.Abstract
{
    public abstract class AuthRequestBase
    {
        private static readonly string[] AuthorizationKeys =
            new[] { "HTTP_AUTHORIZATION", "X-HTTP_AUTHORIZATION", "X_HTTP_AUTHORIZATION" };

        private string _authorizationKey;
        private IEnumerable<string> _parts;
        private string _scheme;
        private string _params;

        protected AuthRequestBase(IDictionary<string, object> environment)
        {
            Environment = environment;
        }

        protected IDictionary<string, object> Environment { get; set; }

        public bool IsProvided()
        {
            return AuthorizationKey != null;
        }

        public IEnumerable<string> Parts
        {
            get
            {
                return _parts ??
                       (_parts = ((string)Environment[AuthorizationKey]).Split(new[] {' '}, 2));
            }
        }

        public string Scheme
        {
            get { return _scheme ?? (_scheme = Parts.First().ToLower()); }
        }

        public string Params { get { return _params ?? (_params = Parts.Last()); } }

        private string AuthorizationKey
        {
            get {
                return _authorizationKey ??
                       (_authorizationKey = GetAuthorizationKey());
            }
        }

        private string GetAuthorizationKey()
        {
            var authKeys = AuthorizationKeys.Where(key => Environment.ContainsKey(key));
            return authKeys.Any() ? authKeys.First() : null;
        }
    }
}