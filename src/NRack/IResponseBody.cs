using System;

namespace NRack
{
    public interface IResponseBody
    {
        void Each(Action<object> action);
    }
}