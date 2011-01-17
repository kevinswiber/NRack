using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NRack.Extensions;
using NRack.Helpers;

namespace NRack
{
    public class File : ICallable, IPathConvertible, IIterable
    {
        public string Root { get; set; }
        public string Path { get; set; }

        private string _pathInfo;
        private IEnumerable<long> _range;

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

            if (_pathInfo.StartsWith("/"))
            {
                _pathInfo = _pathInfo.Substring(1);
            }

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

            var ranges = Utils.ByteRanges(environment, size);

            if (ranges == null || ranges.Count() > 1)
            {
                // No ranges or multiple ranges (which we don't support):
                // TODO: Support multiple ranges.
                response[0] = 200;
                _range = Utils.CreateRangeArray(0, size - 1);
            }
            else if (!ranges.Any())
            {
                // Unsatisfiable.  Return error and file size.
                response = Fail(416, "Byte range unsatisfiable");
                response[1]["Content-Range"] = "bytes */" + size;
                return response;
            }
            else
            {
                // Partial content
                _range = ranges.First();
                response[0] = 206;
                response[1]["Content-Range"] = "bytes " + _range.First() + "-" + _range.Last() + "/" + size;
                size = _range.Last() - _range.First() + 1;
            }

            response[1]["Content-Length"] = size.ToString();

            return response;
        }

        #region Implementation of IIterable

        public void Each(Action<dynamic> action)
        {
            using (var fileStream = System.IO.File.OpenRead(Path))
            {
                fileStream.Seek(_range.First(), SeekOrigin.Begin);
                var remainingLength = _range.Last() - _range.First() + 1;

                while (remainingLength > 0)
                {
                    var bufferSize = remainingLength < 8192 ? remainingLength : 8192;
                    var part = new byte[bufferSize];
                    var bytesRead = fileStream.Read(part, (int)_range.First(), (int)bufferSize);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    remainingLength -= bytesRead;

                    action(part);
                }
            }
        }

        #endregion
    }
}