using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NRack.Adapters;

namespace NRack
{
    public class UrlMap
    {
        // Anonymous Type: Host, Location, Match, App
        private List<dynamic> _mapping;

        public UrlMap(Dictionary<string, dynamic> map = null)
        {
            if (map == null)
            {
                map = new Dictionary<string, dynamic>();
            }

            ReMap(map);
        }

        public void ReMap(Dictionary<string, dynamic> map)
        {
            _mapping = new List<object>();

            foreach (var key in map.Keys)
            {
                string host = null;
                string location = key;

                var fullUrlLocationRegex = new Regex(@"^https?://(.*?)(/.*)", RegexOptions.IgnoreCase);
                var matchingParts = fullUrlLocationRegex.Match(location);
                if (matchingParts.Success)
                {
                    var groups = matchingParts.Groups;

                    host = groups[1].Value;
                    location = groups[2].Value;
                }

                if (!location.StartsWith("/"))
                {
                    throw new ArgumentException("Path needs to start with '/'");
                }

                if (location.EndsWith("/"))
                {
                    location = location.Remove(location.Length - 1);
                }

                var escapedLocation = Regex.Escape(location).Replace("/", "/+");
                var match = new Regex("^" + escapedLocation + "(.*)", RegexOptions.CultureInvariant);
                var app = map[key];

                _mapping.Add(new {Host = host, Location = location, Match = match, App = app});
            }

            _mapping = Enumerable.OrderByDescending(_mapping, mapping => ((mapping.Host ?? string.Empty) + mapping.Location).Length).ToList();
        }

        public dynamic[] Call(IDictionary<string, dynamic> env)
        {
            var pathInfo = env["PATH_INFO"];
            var scriptName = env["SCRIPT_NAME"];
            var httpHost = env.ContainsKey("HTTP_HOST") ? env["HTTP_HOST"] : null;
            var serverName = env["SERVER_NAME"];
            var serverPort = env["SERVER_PORT"];

            try
            {
                foreach (var mapping in _mapping)
                {
                    if (httpHost != mapping.Host &&
                        serverName != mapping.Host &&
                        !(string.IsNullOrEmpty(mapping.Host as string) &&
                          (httpHost == serverName || httpHost == serverName + ":" + serverPort)))
                    {
                        continue;
                    }

                    var match = (Regex) mapping.Match;
                    string rest = null;

                    var matched = match.Match(pathInfo);

                    if (matched.Success)
                    {
                        if (matched.Groups.Count > 1)
                        {
                            rest = matched.Groups[1].Value;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (!(string.IsNullOrEmpty(rest) || rest.StartsWith("/")))
                    {
                        continue;
                    }


                    if (!env.ContainsKey("SCRIPT_NAME"))
                    {
                        env.Add("SCRIPT_NAME", string.Empty);
                    }
                    if (!env.ContainsKey("PATH_INFO"))
                    {
                        env.Add("PATH_INFO", string.Empty);
                    }

                    env["SCRIPT_NAME"] = scriptName + mapping.Location;
                    env["PATH_INFO"] = rest;

                    return mapping.App.Call(env);
                }

            return new dynamic[]
                       {
                           404, new Headers {{"Content-Type", "text/plain"}, {"X-Cascade", "pass"}},
                           new[] {"Not Found: " + pathInfo}
                       };
            }
            finally
            {
                if (!env.ContainsKey("SCRIPT_NAME"))
                {
                    env.Add("SCRIPT_NAME", string.Empty);
                }
                if (!env.ContainsKey("PATH_INFO"))
                {
                    env.Add("PATH_INFO", string.Empty);
                }

                env["SCRIPT_NAME"] = scriptName;
                env["PATH_INFO"] = pathInfo;
            }
        }
    }
}