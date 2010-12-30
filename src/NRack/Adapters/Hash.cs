using System;
using System.Collections.Generic;

namespace NRack.Adapters
{
    public class Hash : Dictionary<string, dynamic>
    {
        public void Each(Action<dynamic> action)
        {
            foreach (var key in Keys)
            {
                action(new { Key = key, Value = this[key] });
            }
        }
    }
}