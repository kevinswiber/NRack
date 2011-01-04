using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using NRack.Adapters;


namespace NRack
{
    public class Lobster : IApplication
    {
        private const string EncodedLobster = @"eJx9kEEOwyAMBO99xd7MAcytUhPlJyj2
    P6jy9i4k9EQyGAnBarEXeCBqSkntNXsi/ZCvC48zGQoZKikGrFMZvgS5ZHd+aGWVuWwhVF0
    t1drVmiR42HcWNz5w3QanT+2gIvTVCiE1lm1Y0eU4JGmIIbaKwextKn8rvW+p5PIwFl8ZWJ
    I8jyiTlhTcYXkekJAzTyYN6E08A+dk8voBkAVTJQ==";

        private string _lobsterString;
        private dynamic _lambdaLobster;

        public Lobster()
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
            _lobsterString = System.Text.Encoding.UTF8.GetString(lobsterOut.ToArray());
            lobsterOut.Close();

            _lambdaLobster = DetachedApplication.Create(env =>
                                                            {
                                                                var lobster = string.Empty;
                                                                var href = string.Empty;

                                                                if (env.ContainsKey("QUERY_STRING") &&
                                                                    env["QUERY_STRING"].ToString().Contains("flip"))
                                                                {
                                                                    // reverse lobster
                                                                    href = "?";
                                                                }
                                                                else
                                                                {
                                                                    lobster = _lobsterString;
                                                                    href = "?flip";
                                                                }

                                                                var content = "<title>Lobstericious!</title><pre>" +
                                                                              lobster + "</pre><a href=\"" + href +
                                                                              "\">flip!</a>";

                                                                return null;
                                                            });
        }

        #region Implementation of IApplication

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}