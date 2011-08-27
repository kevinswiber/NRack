NRack
=====
NRack is a port of Ruby's [Rack](http://rack.rubyforge.org/) framework to the .NET universe using C#.  

NRack currently supports running on ASP.NET and OWIN compatible servers.  This includes IIS, Cassini, IIS Express, and Kayak.

"Hello World" Rack Application
-------------------
Here is a small example of an NRack application.

```c#
public class MyApp : ICallable
{
  public dynamic[] Call(IDictionary<string, dynamic> environment)
  {
    return new dynamic[] { 200, new Hash{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
  }
}
```

Check out NRack.Example and NRack.Specs for more info.

Setup
-----

In the root of your Web Application, create a RackConfig file that inherits from NRack.Configuration.ConfigBase:

```c#
public class Config : ConfigBase
{
  public override void Start()
  {
    Run(environment =>
      new dynamic[] { 200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>" });
  }
}
```

Here's a more complex configuration:

```c#
public override void Start()
{
  Use<BasicAuthHandler>("Lobster",
    (Func<string, string, bool>)((username, password) => password == "p4ssw0rd!"))
  .Map("/app",
    rack =>
      rack.Use<YuiCompressor>(AppDomain.CurrentDomain.BaseDirectory + "Scripts\\")
        .Run(new MyApp()))
  .Map("/env", rack => rack.Run(new EnvironmentOutput()));
}
```

Add the following to your Web.config:

```xml
<configuration>
  <system.web>
    <httpHandlers>
      <add verb="*" path="*" type="NRack.Hosting.AspNet.AspNetHandler"/>
    </httpHandlers>
    <compilation debug="true"/>
  </system.web>
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false"/>
    <modules runAllManagedModulesForAllRequests="true"/>
    <handlers>
      <add name="RackHandler" verb="*" path="*" type="NRack.Hosting.AspNet.AspNetHandler"/>
    </handlers>
  </system.webServer>
</configuration>
```
