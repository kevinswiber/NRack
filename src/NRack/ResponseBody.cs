using System;
using System.Collections.ObjectModel;

namespace NRack
{
    public class ResponseBody : Collection<object>
    {
        public void Each(Action<object> action)
        {
            foreach (var item in this)
            {
                action(item);
            }
        }
    }
}