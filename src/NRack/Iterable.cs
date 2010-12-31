using System;
using System.Collections.ObjectModel;

namespace NRack
{
    public class Iterable : Collection<dynamic>, IIterable
    {
        public void Each(Action<dynamic> action)
        {
            foreach (var item in this)
            {
                action(item);
            }
        }
    }
}