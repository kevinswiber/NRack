using System;
using System.Collections.Generic;
using System.IO;
using NRack.Helpers;
using NRack.Extensions;

namespace NRack
{
    public class File : ICallable, IPathConvertible
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

        #region Implementation of IPathConvertible

        public string ToPath()
        {
            return Path;
        }

        #endregion

        private dynamic[] InnerCall(IDictionary<string, dynamic> environment)
        {
            _pathInfo = Utils.Unescape(environment["PATH_INFO"]);
            if (_pathInfo.Contains(".."))
            {
                return Fail(403, "Forbidden");
            }

            Path = System.IO.Path.Combine(Root ?? string.Empty, _pathInfo);

            if (!System.IO.File.Exists(Path))
            {
                return Fail(404, "File not found: " + _pathInfo);
            }

            try
            {
                return Serving(environment, Path);
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

        private dynamic[] Serving(IDictionary<string, dynamic> environment, string path)
        {
            var fileInfo = new FileInfo(path);
            var size = fileInfo.Length;

            var response = new dynamic[]
                               {
                                   200, 
                                   new Hash
                                        {
                                            {"Last-Modified", fileInfo.LastWriteTimeUtc.ToHttpDateString()},
                                            {"Content-Type", Mime.MimeType(fileInfo.Extension, "text/plain")}
                                        }, 
                                   this
                               };

            return null;
        }
    }
}