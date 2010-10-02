using System;
using System.Collections.ObjectModel;

namespace Rack
{
    public class ResponseBody : Collection<object>
    {
        public void Each(Action<dynamic> action)
        {
            foreach (var item in this)
            {
                action(item);
            }
        }
    }
}