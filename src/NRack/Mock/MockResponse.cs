using System;
using System.Collections.Generic;
using System.IO;
using NRack.Helpers;

namespace NRack.Mock
{
    public class MockResponse
    {
        public MockResponse(dynamic[] responseArray)
        {
            Status = Convert.ToInt32(responseArray[0]);
            OriginalHeaders = responseArray[1];
            Headers = (Hash) responseArray[1];

            var bodyList = new List<string>();
            var body = responseArray[2] as IIterable;

            if (body == null)
            {
                body = new IterableAdapter(responseArray[2]);
            }


            body.Each(part => bodyList.Add(part.ToString()));

            Body = new IterableAdapter(bodyList);

            var errors = responseArray.Length > 3 ? responseArray[3] : new MemoryStream();
            if (errors is MemoryStream)
            {
                var errorStream = (MemoryStream) errors;

                var pos = errorStream.Position;
                errorStream.Position = 0;

                var reader = new StreamReader(errorStream);
                var errorString = reader.ReadToEnd();

                errorStream.Position = pos;

                Errors = errorString;
            }
        }

        public string this[string name]
        {
            get { return Headers[name]; } 
        }

        public int Status { get; private set; }
        public Hash OriginalHeaders { get; private set; }
        public Hash Headers { get; private set; }
        public IIterable Body { get; private set; }
        public string Errors { get; set; }
    }
}