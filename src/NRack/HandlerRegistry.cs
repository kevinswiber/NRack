using System;
using System.Collections.Generic;
using NRack.Hosting.Kayak;

namespace NRack
{
    public class HandlerRegistry
    {
        private static IDictionary<string, dynamic> _handlers;

        static HandlerRegistry()
        {
            if (_handlers == null)
            {
                _handlers = new Dictionary<string, dynamic>();
                Register("kayak", typeof (KayakHandler));
            }
        }

        public static Type Default()
        {
            return _handlers["kayak"];
        }

        public static Type Get(string serverKey)
        {
            if (string.IsNullOrEmpty(serverKey) || !_handlers.ContainsKey(serverKey))
            {
                throw new ArgumentException("Handler '" + serverKey ?? string.Empty + "' does not exist.");
            }

            return _handlers[serverKey];
        }

        public static void Register(string server, Type type)
        {
            _handlers[server] = type;
        }
    }
}