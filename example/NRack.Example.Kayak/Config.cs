using System;
using NRack.Configuration;
using NRack.Helpers;

namespace NRack.Example.Kayak
{
    public class Config : ConfigBase
    {
        #region Overrides of ConfigBase

        public override void Start()
        {

            Map("/assets", rack =>
                                rack.Run(new File(AppDomain.CurrentDomain.BaseDirectory + @"Files\")))
            .Map("/", rack =>
                rack.Run(env => new dynamic[] { 200, new Hash { { "Content-Type", "text/html" } }, "<h1>Hello, World!</h1>" }));
        }

        #endregion
    }
}