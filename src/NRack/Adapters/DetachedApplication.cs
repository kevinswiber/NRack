using System;
using System.Collections.Generic;

namespace NRack.Adapters
{
    public class DetachedApplication
    {
        public static dynamic Create(Func<IDictionary<string, dynamic>, dynamic[]> app)
        {
            return new IterableApplicationAdapter(new Proc(app));
        }
    }
}