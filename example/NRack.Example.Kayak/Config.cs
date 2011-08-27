using System;
using NRack.Configuration;
using NRack.Helpers;

namespace NRack.Example.Kayak
{
    public class Config : ConfigBase
    {
        public override void Start()
        {
            Map("/files", builder => builder.Run(new File(AppDomain.CurrentDomain.BaseDirectory + @"Files\")))
            .Map("/", builder => builder.Run(env => new dynamic[] {200, new Hash {{"Content-Type", "text/plain"}}, "Hello, world!"}));
            //Run(env => new dynamic[] {200, new Hash {{"Content-Type", "text/plain"}}, "Hello, world!"});
        }
    }
}