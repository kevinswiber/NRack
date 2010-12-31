using System.Collections.Generic;
using NRack.Adapters;
using NRack.Hosting.Kayak;

namespace NRack
{
    public class Server
    {
        private Hash _options;
        private dynamic _app;

        public Server() : this(null)
        {}

        public Server(Hash options)
        {
            if (options != null && options.ContainsKey(ServerOptions.App) 
                && options[ServerOptions.App] != null)
            {
                _app = options[ServerOptions.App];
            }

            _options = DefaultOptions.Merge(options ?? new Hash());
        }

        public static void Start(Hash options = null)
        {
            new Server(options).InnerStart();
        }

        public void InnerStart()
        {
            GetServer().Run(_app, _options);
        }

        public Hash DefaultOptions
        {
            get
            {
                return new Hash
                           {
                               {ServerOptions.Environment, "development"},
                               {ServerOptions.Pid, null},
                               {ServerOptions.Port, 9292},
                               {ServerOptions.Host, "127.0.0.1"},
                               {ServerOptions.AccessLog, new List<dynamic>()},
                               {ServerOptions.Config, "Config"}
                           };
            }
        }

        public dynamic GetServer()
        {
            if (_options.ContainsKey(ServerOptions.Server) && _options[ServerOptions.Server] != null)
            {
                return _options[ServerOptions.Server];
            }

            return new KayakHandler();
        }
    }

    public static class ServerOptions
    {
        public const string App = "app";
        public const string Config = "config";
        public const string Environment = "environment";
        public const string Server = "server";
        public const string Daemonize = "daemonize";
        public const string Pid = "pid";
        public const string Host = "Host";
        public const string Port = "Port";
        public const string AccessLog = "AccessLog";
        public const string Debug = "debug";
        public const string Warn = "warn";
        public const string Include = "include";
        public const string Require = "require";
    }
}