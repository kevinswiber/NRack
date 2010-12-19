using System;

namespace NRack
{
    /// <summary>
    /// The Proc class wraps a delegate and adds a Call method to dynamically invoke 
    /// the delegate with arguments.
    /// 
    /// This is a workaround for the limitation of unsupported extension methods
    /// during runtime binding.
    /// </summary>
    public class Proc
    {
        private readonly Delegate _wrappedFunction;

        public Proc(Delegate wrappedFunction)
        {
            _wrappedFunction = wrappedFunction;
        }

        /// <summary>
        /// Dynamically invoke the delegate with parameters.
        /// </summary>
        /// <param name="args">A list of method arguments.</param>
        /// <returns>Passthrough of the delegate return.</returns>
        public dynamic Call(params dynamic[] args)
        {

            return _wrappedFunction.DynamicInvoke(args);
        }
    }
}