using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using NRack.Adapters;

namespace NRack.ServerHelpers
{
    public class ServerOptionsBuilder
    {
        public static Hash BuildFromArgs(string[] args, ServerOptions options)
        {
            var parser = new CommandLineParser();
            parser.ParseArguments(args, options);

            if (args.Count() > 0)
            {
                var lastArg = args.Last();
                options.Config = Type.GetType(lastArg, false);
            }
            
            return CreateDictionaryFromOptions(options);
        }

        private static Hash CreateDictionaryFromOptions(ServerOptions options)
        {
            var config = new Hash
                             {
                                 {ServerOptionKeys.Debug, options.Debug},
                                 {ServerOptionKeys.Warn, options.Warn},
                                 {ServerOptionKeys.Include, options.Include != null 
                                     ? options.Include.Split(new[] {':'}) : null},
                                 {ServerOptionKeys.Require, options.Require},
                                 {ServerOptionKeys.Server, options.Server},
                                 {ServerOptionKeys.Host, options.Host},
                                 {ServerOptionKeys.Port, options.Port},
                                 {ServerOptionKeys.Environment, options.Environment},
                                 {ServerOptionKeys.Daemonize, options.Daemonize},
                                 {ServerOptionKeys.Config, options.Config}
                             };

            return config;
        }
    }
}