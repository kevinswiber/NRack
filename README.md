NRack
=====
NRack is a port of Ruby's [Rack](http://rack.rubyforge.org/) framework to the .NET universe using C#.  **Note:  The master branch is still in an early, pre-release state subject to high volatility.**  Keep watching as new commits will roll in with some frequency.

"Hello World" Rack Application
-------------------
Here is a small example of an NRack application.

	public class MyApp : ICallable
	{
			public dynamic[] Call(IDictionary<string, dynamic> environment)
			{
					return new dynamic[] { 200, new Headers{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
			}
	}

Check out NRack.Example and NRack.Tests for more info.

Setup
-----

In the root of your Web Application, create a RackConfig file that inherits from NRack.Configuration.ConfigBase:

	public class Config : ConfigBase
	{
			public override void Start()
			{
					Run(environment =>
									new dynamic[] { 200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>" });
			}
	}

Here's a more complex configuration:
	public override void Start()
	{
			Use<BasicAuthHandler>("Lobster",
					(Func<string, string, bool>)((username, password) => password == "secret"))
			.Map("/app",
					rack =>
							rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"))
									.Run(new MyApp()))
			.Map("/env", rack => rack.Run(new EnvironmentOutput()));
	}


Add the following to your Web.config:

	<configuration>
		<system.web>
			<httpHandlers>
				<add verb="*" path="*" type="NRack.Hosting.AspNet.AspNetHandler"/>
			</httpHandlers>
			<compilation debug="true"/></system.web>
		<system.webServer>
			<validation validateIntegratedModeConfiguration="false"/>
			<modules runAllManagedModulesForAllRequests="true"/>
			<handlers>
				<add name="RackHandler" verb="*" path="*" type="NRack.Hosting.AspNet.AspNetHandler"/>
			</handlers>
		</system.webServer>
	</configuration>
