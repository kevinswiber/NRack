using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using NRack.Helpers;


namespace NRack
{
    public class Lobster : ICallable
    {
        private const string EncodedLobster = @"eJx9kEEOwyAMBO99xd7MAcytUhPlJyj2
    P6jy9i4k9EQyGAnBarEXeCBqSkntNXsi/ZCvC48zGQoZKikGrFMZvgS5ZHd+aGWVuWwhVF0
    t1drVmiR42HcWNz5w3QanT+2gIvTVCiE1lm1Y0eU4JGmIIbaKwextKn8rvW+p5PIwFl8ZWJ
    I8jyiTlhTcYXkekJAzTyYN6E08A+dk8voBkAVTJQ==";

        public static string LobsterString = GetLobsterString();
        public static dynamic LambdaLobster = GetLambdaLobster();

        private static string GetLobsterString()
        {
            var lobster64 = EncodedLobster.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
            var lobsterBytes = Convert.FromBase64String(lobster64);
            var lobsterStream = new MemoryStream(lobsterBytes);
            var inflater = new InflaterInputStream(lobsterStream);
            var lobsterOut = new MemoryStream();

            var b = new byte[1];

            int n;
            while ((n = inflater.Read(b, 0, b.Length)) > 0)
            {
                lobsterOut.Write(b, 0, n);
            }

            inflater.Close();
            var lobsterString = System.Text.Encoding.UTF8.GetString(lobsterOut.ToArray());
            lobsterOut.Close();

            return lobsterString;
        }

        private static object GetLambdaLobster()
        {
            return DetachedApplication.Create(env =>
            {
                var lobster = string.Empty;
                string href;

                if (env.ContainsKey("QUERY_STRING") &&
                    env["QUERY_STRING"].ToString().Contains("flip"))
                {
                    var lobsterArray = LobsterString.Split('\n').Select(x => new string(x.Reverse().ToArray())).ToArray();

                    lobster = string.Join("\n", lobsterArray);
                    href = "?";
                }
                else
                {
                    lobster = LobsterString;
                    href = "?flip";
                }

                var content = "<title>Lobstericious!</title><pre>" +
                              lobster + "</pre><a href=\"" + href +
                              "\">flip!</a>";

                return new dynamic[]
                           {
                               200, 
                               new Hash {{"Content-Length", content.Length.ToString()}, 
                                    {"Content-Type", "text/html"}}, 
                               content
                           };
            });
        }

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var request = new Request(environment);
            string lobster;
            string href;

            if (request.GET.ContainsKey("flip") && request.GET["flip"] == "left")
            {
                lobster = string.Join("\n", LobsterString.Split('\n').Select(line => new string(line.PadRight(42).Reverse().ToArray())));
                href = "?flip=right";
            }
            else if (request.GET.ContainsKey("flip") && request.GET["flip"] == "crash")
            {
                throw new InvalidOperationException("Lobster crashed!");
            }
            else
            {
                lobster = LobsterString;
                href = "?flip=left";
            }

            var response = new Response();
            response.Write("<title>Lobstericious!</title>");
            response.Write("<pre>");
            response.Write(lobster);
            response.Write("</pre>");
            response.Write("<p><a href='" + href + "'>flip!</a></p>");
            response.Write("<p><a href='?flip=crash'>crash!</a></p>");

            return response.Finish();
        }
    }
}