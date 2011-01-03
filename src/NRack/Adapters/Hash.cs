using System;
using System.Collections.Generic;

namespace NRack.Adapters
{
    public class Hash : Dictionary<string, dynamic>
    {
        public Hash() {}

        public Hash(IDictionary<string, dynamic> dictionary) : base(dictionary)
        {
        }

        public void Each(Action<dynamic> action)
        {
            foreach (var key in Keys)
            {
                action(new { Key = key, Value = this[key] });
            }
        }

        public Hash Merge(Hash other)
        {
            var mergedHash = new Hash();
            Each(pair => mergedHash[pair.Key] = pair.Value);
            other.Each(pair => mergedHash[pair.Key] = pair.Value);

            return mergedHash;
        }
    }
}