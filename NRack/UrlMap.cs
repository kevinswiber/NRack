using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NRack
{
    public class UrlMap
    {
        // Anonymous Type: Host, Location, Match, App
        private List<dynamic> _mapping;

        public UrlMap(Dictionary<string, object> map = null)
        {
            if (map == null)
            {
                map = new Dictionary<string, object>();
            }

            ReMap(map);
        }

        public void ReMap(Dictionary<string, object> map)
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
                    var captures = matchingParts.Captures;

                    host = captures[0].Value;
                    location = captures[1].Value;
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

            Enumerable.OrderByDescending(_mapping, mapping => ((mapping.Host ?? string.Empty) + mapping.Location).Length);
        }

        public dynamic[] Call(IDictionary<string, object> env)
        {
            var pathInfo = env["PATH_INFO"].ToString();
            var scriptName = env["SCRIPT_NAME"].ToString();
            var httpHost = env["HTTP_HOST"].ToString();
            var serverName = env["SERVER_NAME"].ToString();
            var serverPort = env["SERVER_PORT"].ToString();

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

                    if (!string.IsNullOrEmpty(rest) && !rest.StartsWith("/"))
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