using NRack.Adapters;
using NRack.Configuration;

namespace NRack.Example.Kayak
{
    public class Config : ConfigBase
    {
        #region Overrides of ConfigBase

        public override void RackUp()
        {
            Run(env => new dynamic[] {200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"});
        }

        #endregion
    }
}