using System;
using System.Collections.Generic;

namespace NRack.Helpers
{
    public class DetachedApplication
    {
        public static dynamic Create(Func<IDictionary<string, dynamic>, dynamic[]> app)
        {
            return new CalledWithIterableResponseAdapter(new Proc(app));
        }
    }
}