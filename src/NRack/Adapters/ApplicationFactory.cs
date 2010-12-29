using System;
using System.Collections.Generic;

namespace NRack.Adapters
{
    public class ApplicationFactory
    {
        public static Proc Create(Func<IDictionary<string, dynamic>, dynamic[]> app)
        {
            return new Proc(app);
        }
    }
}