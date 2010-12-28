using System;
using System.Collections;
using System.Collections.Generic;

namespace NRack.Adapters
{
    public class EnumerableBodyAdapter : IResponseBody
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

        public override string ToString()
        {
            var returnList = new List<string>();

            Each(x => returnList.Add(x.ToString()));

            return string.Join(string.Empty, returnList.ToArray());
        }
    }
}