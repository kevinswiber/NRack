using System;

namespace NRack
{
    public interface IIterable
    {
        void Each(Action<dynamic> action);
    }
}