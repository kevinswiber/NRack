using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NRack.Helpers;

namespace NRack
{
    public class Response : IIterable
    {

        public Response(dynamic body = null, int status = 200, Hash header = null, Action<dynamic> block = null)
        {
            if (body == null)
            {
                body = new List<dynamic>();
            }

            if (header == null)
            {
                header = new Hash();
            }

            Headers = new HeaderHash(new Hash { { "Content-Type", "text/html" } }).Merge(header);
            Status = status;
            Body = new List<dynamic>();
            Writer = str => Body.Add(str);

            if (Headers.ContainsKey("Transfer-Encoding") && Headers["Transfer-Encoding"] == "chunked")
            {
                Chunked = true;
            }

            if (body is string)
            {
                Write(body);
            }
            else if (body is IEnumerable)
            {
                new IterableAdapter(body).Each(part => Write(part.ToString()));
            }
            else
            {
                throw new ArgumentException("Must be iterable.", "body");
            }

            if (block != null)
            {
                block(this);
            }
        }

        public int Length { get; private set; }
        public int Status { get; set; }
        public Hash Headers { get; private set; }
        public dynamic Body { get; set; }
        public bool Chunked { get; set; }
        private Action<string> Writer { get; set; }
        private Action<dynamic> Block { get; set; }

        public dynamic[] Finish(Action<dynamic> block = null)
        {
            Block = block;
            if (Status == 204 || Status == 304)
            {
                Headers.Remove("Content-Type");
                return new dynamic[] { Status, Headers, new IterableAdapter(new dynamic[0]) };
            }

            return new dynamic[] { Status, Headers, this };
        }

        public void Each(Action<dynamic> action)
        {
            new IterableAdapter(Body).Each(action);
            Writer = action;

            if (Block != null)
            {
                Block(this);
            }
        }

        public string Write(string content)
        {
            Length += Encoding.UTF8.GetByteCount(content);
            Writer(content);

            if (!Chunked)
            {
                Headers["Content-Length"] = Length.ToString();
            }

            return content;
        }
    }
}