using System;
using CommandLine;

namespace NRack.ServerHelpers
{
    public class ServerOptions
    {
        [Option("d","debug", HelpText = "set debugging flags")]
        public bool Debug;

        [Option("w", "warn", HelpText="turn warnings on")]
        public bool Warn;

        [Option("I", "include", HelpText="specify assemblies to load")]
        public string Include;

        [Option("r", "require", HelpText="require the library before executing")]
        public string Require;

        [Option("s", "server", HelpText="serve using SERVER (kayak)")]
        public string Server;

        [Option("o","host",HelpText="listen on HOST (default 0.0.0.0)")]
        public string Host;

        [Option("p", "port", HelpText="use PORT (default: 9292)")]
        public string Port;

        [Option("E", "env", HelpText="use ENVIRONMENT for defaults (default: development)")]
        public string Environment;

        [Option("D", "daemonize", HelpText="Run as a Windows service in the background.")]
        public bool Daemonize;

        [Option(null, "version", HelpText="Show version")]
        public string Version;

        public Type Config { get; set; }
    }
}