using System;
using System.Collections.Generic;
using System.IO;
using NRack.Helpers;
using NRack.Extensions;

namespace NRack
{
    public class File : ICallable
    {
        public string Root { get; set; }
        public string Path { get; set; }

        private string _pathInfo;
        public File(string root)
        {
            Root = root;
        }

        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            return InnerCall(environment);
        }

        #endregion

        private dynamic[] InnerCall(IDictionary<string, dynamic> environment)
        {
            _pathInfo = Utils.Unescape(environment["PATH_INFO"]);
            if (_pathInfo.Contains(".."))
            {
                return Fail(403, "Forbidden");
            }

            Path = System.IO.Path.Combine(Path, _pathInfo);

            if (!System.IO.File.Exists(Path))
            {
                return Fail(404, "File not found: " + _pathInfo);
            }

            try
            {
                using (var fileStream = System.IO.File.OpenRead(Path))
                {
                    return Serving(environment, fileStream);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Fail(404, "File not found: " + _pathInfo);
            }
        }

        private dynamic[] Fail(int status, string body)
        {
            return new dynamic[]
                       {
                           status,
                           new Hash
                               {
                                   {"Content-Type", "text/plain"},
                                   {"Content-Length", body.Length.ToString()},
                                   {"X-Cascade", "pass"}
                               },
                           new[] {body}
                       };
        }

        private dynamic[] Serving(IDictionary<string, dynamic> environment, FileStream fileStream)
        {
            var size = fileStream.Length;

            var response = new dynamic[]
                               {
                                   200, 
                                   new Hash
                                        {
                                            {"Last-Modified", System.IO.File.GetLastWriteTime(Path).Date.ToHttpDateString()},
                                            {"Content-Type", Mime.MimeType(new FileInfo(Path).Extension)}
                                        }, 
                                   this
                               };

            return null;
        }
    }
}