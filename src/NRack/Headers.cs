using System;
using System.Collections.Specialized;

namespace NRack
{
    public class Headers : NameValueCollection
    {
        public void Each(Action<object> action)
        {
            foreach (var key in AllKeys)
            {
                action(new { Key = key, Value = this[key] });
            }
        }
    }
}