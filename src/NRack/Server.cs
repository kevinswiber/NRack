using System;
using System.Collections.Generic;
using System.Linq;
using NRack.Helpers;
using NRack.ServerHelpers;

namespace NRack
{
    public class Server
    {
        private readonly Hash _options;

        public Server() : this(null)
        {}

        public Server(Hash options)
        {
            _options = options;
            
            MergeWithDefaultOptions(_options);
            SetDefaults(_options);
        }

        private void SetDefaults(Hash options)
        {
            if (options != null && options.ContainsKey(ServerOptionKeys.App) 
                && options[ServerOptionKeys.App] != null)
            {
                App = options[ServerOptionKeys.App];
            }
            else if (options != null && options.ContainsKey(ServerOptionKeys.Config) &&
                     options[ServerOptionKeys.Config] != null)
            {
                var config = ConfigFactory.NewInstance((Type) options[ServerOptionKeys.Config]);
                Func<IDictionary<string, object>, object> builderInContextFunc =
                    env => new Builder(config.ExecuteStart).Call(env);

                App = new Proc(builderInContextFunc);
            }
        }

        private void MergeWithDefaultOptions(Hash options)
        {
            foreach (var key in DefaultOptions.Keys.Where(key => !options.ContainsKey(key) || options[key] == null))
            {
                options[key] = DefaultOptions[key];
            }
        }

        public static void Start(Hash options = null)
        {
            new Server(options).StartInstance();
        }

        public static void Start(string[] args)
        {
            var commandLineOptions = new ServerOptions();

            var options = ServerOptionsBuilder.BuildFromArgs(args, commandLineOptions);

            Start(options ?? new Hash());
        }

        public void StartInstance()
        {
            GetServer().Run(App, _options);
        }

        public Hash DefaultOptions
        {
            get
            {
                return new Hash
                           {
                               {ServerOptionKeys.Environment, "development"},
                               {ServerOptionKeys.Port, 9292},
                               {ServerOptionKeys.Host, "127.0.0.1"},
                               {ServerOptionKeys.AccessLog, new List<dynamic>()},
                               {ServerOptionKeys.Config, ConfigFinder.FindType()}
                           };
            }
        }

        public dynamic App { get; private set; }

        public dynamic GetServer()
        {
            Type serverType;
            if (_options.ContainsKey(ServerOptionKeys.Server) && _options[ServerOptionKeys.Server] != null)
            {
                serverType = HandlerRegistry.Get(_options[ServerOptionKeys.Server]);
            }
            else
            {
                serverType = HandlerRegistry.Default();   
            }

            return Activator.CreateInstance(serverType);
        }
    }
}