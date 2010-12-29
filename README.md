NRack
=====
NRack is a port of Ruby's [Rack](http://rack.rubyforge.org/) framework to the .NET universe.

"Hello World" Rack Application
-------------------
Here is a small example of an NRack application.

	public class MyApp : IApplication
	{
			public dynamic[] Call(IDictionary<string, dynamic> environment)
			{
					return new dynamic[] { 200, new Headers{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
			}
	}

Check out NRack.Example and the NRack.Tests for more info.

Setup
-----
Add the following to your Web.config:

	<system.web>
	<compilation debug="true" targetFramework="4.0">
		<assemblies>
			<add assembly="NRack, Version=1.0.0.0, Culture=neutral"/>
		</assemblies>
	</compilation>
	<httpModules>
		<add name="RackModule" type="NRack.Hosting.AspNet.AspNetHttpModule, NRack, Version=1.0.0.0, Culture=neutral"/>
	</httpModules>
	<httpHandlers>
		<clear/>
		<add verb="**" path="**" type="NRack.Hosting.AspNet.AspNetHandler, NRack, Version=1.0.0.0, Culture=neutral" validate="true"/>
		<add verb="**" path="Default.aspx" type="NRack.Hosting.AspNet.AspNetHandler, NRack, Version=1.0.0.0, Culture=neutral" validate="true"/>
	</httpHandlers>
	</system.web>

In the root of your Web Application, create a RackConfig file that inherits from RackConfigBase:

	public class RackConfig : RackConfigBase
	{
			public override void RackUp()
			{
					Map("/app", 
							rack =>
									rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"))
											.Run(new MyApp()));
	
					Map("/env", rack => rack.Run(new EnvironmentOutput()));
			}
	}
