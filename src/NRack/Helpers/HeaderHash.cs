using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NRack.Helpers
{
    public class HeaderHash : Hash
    {
        private Dictionary<string, string> _names;

        public HeaderHash(Hash hash = null)
        {
            _names = new Dictionary<string, string>();

            hash = hash ?? new Hash();
            hash.Each(pair => this[pair.Key] = pair.Value);
        }

        public static HeaderHash Create(Hash hash = null)
        {
            return hash is HeaderHash ? (HeaderHash) hash : new HeaderHash(hash);
        }

        public void Each(Action<string, string> action)
        {
            Each(pair =>
                 action(pair.Key, GetHeaderValue(pair.Value)));
        }

        public new void Add(string key, dynamic value)
        {
            this[key] = value;
        }

        public new dynamic this[string key]
        {
            get
            {
                if (_names.ContainsKey(key))
                {
                    return base[_names[key]];
                }

                return base[_names[key.ToLower()]];
            }

            set
            {
                Remove(key);

                _names[key] = _names[key.ToLower()] = key;
                base[key] = value;
            }
        }

        public new dynamic Remove(string key)
        {
            var canonical = key.ToLower();
            dynamic result = null;
            
            if (_names.ContainsKey(canonical))
            {
                result = base[_names[canonical]];

                base.Remove(_names[canonical]);
                _names.Remove(canonical);
            }

            var caseInsensitiveKey = Keys.Where(findKey => findKey == canonical.ToLower()).FirstOrDefault();

            if (caseInsensitiveKey != null)
            {
                _names.Remove(caseInsensitiveKey);
            }

            return result;
        }

        public dynamic Delete(string key)
        {
            return Remove(key);
        }

        public new bool ContainsKey(string key)
        {
            return _names.ContainsKey(key) || _names.ContainsKey(key.ToLower());
        }

        public HeaderHash Merge(Hash other)
        {
            var mergedHash = new HeaderHash();
            Each(pair => mergedHash[pair.Key] = pair.Value);
            other.Each(pair => mergedHash[pair.Key] = pair.Value);

            return mergedHash;
        }

        public HeaderHash Replace(Hash other)
        {
            Clear();
            other.Each(pair => this[pair.Key] = pair.Value);

            return this;
        }

        public Hash ToHash()
        {
            var convertedHash = new Hash();
            Each(pair => convertedHash[pair.Key] = GetHeaderValue(pair.Value));

            return convertedHash;
        }

        private static string GetHeaderValue(dynamic value)
        {
            if (value is IEnumerable && !(value is Array) && !(value is string))
            {
                value = value.ToArray();
            }

            value = string.Join("\n", value);

            return value;
        }
    }
}