using System;
using System.Collections;

namespace Rack.Hosting.AspNet
{
    public class EnumerableBodyAdapter
    {
        private Action<Action<object>> _forEachAction;

        public EnumerableBodyAdapter(dynamic body)
        {
            body = EnsureEnumerable(body);
            SetEachAction(body);
        }

        private void SetEachAction(dynamic body)
        {
            _forEachAction = action => body.Each(action);
        }
        
        public void Each(Action<object> action)
        {
            _forEachAction(action);
        }

        private static dynamic EnsureEnumerable(dynamic body)
        {
            if (body is IEnumerable)
            {
                var rackBody = new ResponseBody();

                foreach (var item in body)
                {
                    rackBody.Add(item);
                }

                body = rackBody;
            }

            return body;
        }
    }
}