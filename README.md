NRack
=====
NRack is a port of Ruby's Rack framework to the .NET universe.

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

		using System.Web;
		using NRack.Configuration;

		namespace NRack.Example
		{
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
		}


Check out NRack.Example and the NRack.Tests for more info.
