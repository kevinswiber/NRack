using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace NRack
{
    public class Utils
    {
        public static string Escape(string uri)
        {
            return HttpUtility.UrlEncode(uri);
        }

        public static string Unescape(string uri)
        {
            return HttpUtility.UrlDecode(uri);
        }

        public static NameValueCollection ParseQuery(string queryString)
        {
            return HttpUtility.ParseQueryString(queryString);
        }

        public static NameValueCollection ParseNestedQuery(string queryString)
        {
            return HttpUtility.ParseQueryString(queryString);
        }

        public static IEnumerable<IEnumerable<long>> ByteRanges(IDictionary<string, dynamic> env, long size)
        {
            string httpRange = env.ContainsKey("HTTP_RANGE") ? env["HTTP_RANGE"] : null;

            if (httpRange == null)
            {
                return null;
            }

            var ranges = new long[] { };

            var rangeSpecs = Regex.Split(httpRange, @",\s*");
            foreach (var rangeSpec in rangeSpecs)
            {
                var regex = new Regex(@"bytes=(\d*)-(\d*)");
                var matches = regex.Matches(rangeSpec);

                if (matches.Count == 0 || matches[0].Groups.Count == 0)
                {
                    return null;
                }

                var groups = matches[0].Groups;

                if (groups.Count <= 1)
                {
                    return null;
                }

                var r0 = groups[1].Value;
                var r1 = groups[2].Value;
                long r0Value;
                long r1Value;

                if (r0 == string.Empty)
                {
                    if (r1 == string.Empty)
                    {
                        return null;
                    }

                    // suffix-byte-range-spec, represents trailing suffix of file
                    r0Value = new long[] { size - long.Parse(r1), 0 }.Max<long>();
                    r1Value = size - 1;
                }
                else
                {
                    r0Value = long.Parse(r0);

                    if (r1 == string.Empty)
                    {
                        r1Value = size - 1;
                    }
                    else
                    {
                        r1Value = long.Parse(r1);

                        if (r1Value < r0Value)
                        {
                            // backwards range is syntactically invalid
                            return null;
                        }

                        if (r1Value >= size)
                        {
                            r1Value = size - 1;
                        }
                    }
                }

                if (r0Value <= r1Value)
                {
                    ranges = CreateRangeArray(r0Value, r1Value);
                }
            }

            return new[] { ranges };
        }


        public static long[] CreateRangeArray(long start, long end)
        {
            var rangeArray = new Collection<long>();
            
            for (var i = start; i <= end - start + 1; i++)
            {
                rangeArray.Add(i);
            }

            return rangeArray.ToArray();
        }
        
        //protected virtual void ParseRequestHeaderRanges(HttpContext context)
        //{
        //    HttpRequest Request = context.Request;
        //    HttpResponse Response = context.Response;

        //    string rangeHeader = RetrieveHeader(Request, HTTP_HEADER_RANGE, string.Empty);

        //    if (string.IsNullOrEmpty(rangeHeader))
        //    {
        //        // No Range HTTP Header supplied; send back entire contents
        //        this.StartRangeBytes = new long[] { 0 };
        //        this.EndRangeBytes = new long[] { this.InternalRequestedFileInfo.Length - 1 };
        //        this.IsRangeRequest = false;
        //        this.IsMultipartRequest = false;
        //    }
        //    else
        //    {
        //        // rangeHeader contains the value of the Range HTTP Header and can have values like:
        //        //      Range: bytes=0-1            * Get bytes 0 and 1, inclusive
        //        //      Range: bytes=0-500          * Get bytes 0 to 500 (the first 501 bytes), inclusive
        //        //      Range: bytes=400-1000       * Get bytes 500 to 1000 (501 bytes in total), inclusive
        //        //      Range: bytes=-200           * Get the last 200 bytes
        //        //      Range: bytes=500-           * Get all bytes from byte 500 to the end
        //        //
        //        // Can also have multiple ranges delimited by commas, as in:
        //        //      Range: bytes=0-500,600-1000 * Get bytes 0-500 (the first 501 bytes), inclusive plus bytes 600-1000 (401 bytes) inclusive

        //        // Remove "Ranges" and break up the ranges
        //        string[] ranges = rangeHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());

        //        this.StartRangeBytes = new long[ranges.Length];
        //        this.EndRangeBytes = new long[ranges.Length];

        //        this.IsRangeRequest = true;
        //        this.IsMultipartRequest = (this.StartRangeBytes.Length > 1);

        //        for (int i = 0; i < ranges.Length; i++)
        //        {
        //            const int START = 0, END = 1;

        //            // Get the START and END values for the current range
        //            string[] currentRange = ranges[i].Split("-".ToCharArray());

        //            if (string.IsNullOrEmpty(currentRange[END]))
        //                // No end specified
        //                this.EndRangeBytes[i] = this.InternalRequestedFileInfo.Length - 1;
        //            else
        //                // An end was specified
        //                this.EndRangeBytes[i] = long.Parse(currentRange[END]);

        //            if (string.IsNullOrEmpty(currentRange[START]))
        //            {
        //                // No beginning specified, get last n bytes of file
        //                this.StartRangeBytes[i] = this.InternalRequestedFileInfo.Length - 1 - this.EndRangeBytes[i];
        //                this.EndRangeBytes[i] = this.InternalRequestedFileInfo.Length - 1;
        //            }
        //            else
        //            {
        //                // A normal begin value
        //                this.StartRangeBytes[i] = long.Parse(currentRange[0]);
        //            }
        //        }
        //    }
        //}

        public static Dictionary<int, string> HttpStatusCodes = new Dictionary<int, string>
            {
                {100, "Continue"},
                {101, "Switching Protocols"},
                {102, "Processing"},
                {200, "OK"},
                {201, "Created"},
                {202, "Accepted"},
                {203, "Non-Authoritative Information"},
                {204, "No Content"},
                {205, "Reset Content"},
                {206, "Partial Content"},
                {207, "Multi-Status"},
                {208, "Already Reported"},
                {226, "IM Used"},
                {300, "Multiple Choices"},
                {301, "Moved Permanently"},
                {302, "Found"},
                {303, "See Other"},
                {304, "Not Modified"},
                {305, "Use Proxy"},
                {306, "Reserved"},
                {307, "Temporary Redirect"},
                {400, "Bad Request"},
                {401, "Unauthorized"},
                {402, "Payment Required"},
                {403, "Forbidden"},
                {404, "Not Found"},
                {405, "Method Not Allowed"},
                {406, "Not Acceptable"},
                {407, "Proxy Authentication Required"},
                {408, "Request Timeout"},
                {409, "Conflict"},
                {410, "Gone"},
                {411, "Length Required"},
                {412, "Precondition Failed"},
                {413, "Request Entity Too Large"},
                {414, "Request-URI Too Long"},
                {415, "Unsupported Media Type"},
                {416, "Requested Range Not Satisfiable"},
                {417, "Expectation Failed"},
                {422, "Unprocessable Entity"},
                {423, "Locked"},
                {424, "Failed Dependency"},
                {425, "Reserved for WebDAV advanced"},
                {426, "Upgrade Required"},
                {500, "Internal Server Error"},
                {501, "Not Implemented"},
                {502, "Bad Gateway"},
                {503, "Service Unavailable"},
                {504, "Gateway Timeout"},
                {505, "HTTP Version Not Supported"},
                {506, "Variant Also Negotiates"},
                {507, "Insufficient Storage"},
                {508, "Loop Detected"},
                {509, "Unassigned"},
                {510, "Not Extended"}
            };
    }
}