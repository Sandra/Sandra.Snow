---
layout: post
title: NancyFX - Hosting with OWIN
category: NancyFX
---

By now you've probably heard of OWIN, its slowly becoming more and more popular, hell even ThoughtWorks mentioned them on their [Radar](http://www.thoughtworks.com/radar)

If you want to know what OWIN is, head on over to Paul Glavich's blog post on [Owin, Katana, and getting started](http://weblogs.asp.net/pglavich/)

The question of running NancyFX with Owin has been popping up more often lately so I figured I would show you how to get setup. 

## Codez - Project Setup

Lets start off by creating a brand new Empty Web Application:

![](/images/nancyfx-owin-1.png)

Once created you should get a semi long list of References...

<!--excerpt-->

![](/images/nancyfx-owin-2.png)

First things first, we want to trim this back to almost NOTHING! That's right, we're gonna kill more references! In fact I'm going to remove EVERYTHING except the bare minimum, so that you can add references only as you need them.

![](/images/nancyfx-owin-3.png)

:D looks beautiful doesn't it!

Next we're going to add the Nugets

 - Nancy
 - Nancy.Owin

As well as the following... **BUT**

 - Microsoft.Owin.Host.SystemWeb
 - Microsoft.Web.Infrastructure
 - Owin

At the time of writing this, these Nugets are not available on Nuget yet. You will need to get them from the Katana CI Builds from MyGet.

The URL for Katana CI build is ***http://www.myget.org/F/katana/*** 

If you don't know how to add this to Nuget, you can do this 1 of 2 ways. If you've never done it, just go `Tools > Options > Packager Manager > Package Sources`

![](/images/nancyfx-owin-4.png)

Click the PLUS sign, enter the `name` and `source`, and press `ok`

Now you can install the package `Microsoft.Owin.Host.SystemWeb` by entering:

> Install-Package Microsoft.Owin.Host.SystemWeb **-pre**

Make sure you have `-pre` on the end so that it pulls the pre-release packages. This will automatically install all 3 requires packages.

## Codez - Startup

Next we need to create a Startup file, this is where we tell Owin to use the Nancy.Owin middleware, this is a assembly the Nancy Team has created which does all the hard lifting to wire up Nancy to the Owin interfaces. 

I guess you could say this is like adding the Nancy Hanlder to the web.config file...

	<handlers>
	    <add name="Nancy" verb="*" type="Nancy.Hosting.Aspnet.NancyHttpRequestHandler" path="*" />
	</handlers>

Except now we don't need any of that! *(so don't go adding that to your web.config!)*

If you want to dive into what the middleware is doing you can take a look at [Prabir's](https://twitter.com/PrabirShrestha) repository here:

[https://github.com/prabirshrestha/simple-owin](https://github.com/prabirshrestha/simple-owin)

So we need to create a new class called `Startup` which contains a single method.

	namespace NancyOwinWeb
	{
	    using Owin;
	
	    public class Startup
	    {
	        public void Configuration(IAppBuilder app)
	        {
	            app.UseNancy();
	        }
	    }
	}

I opt to put this in a folder called App_Start

![](/images/nancyfx-owin-5.png)

That's really all that's required to setup Nancy in an Owin project. In the same configuration file you would obviously wire up other middleware, maybe some logging, possibly authentication, maybe... *shudder* you might even consider putting WebAPI in there. BUT please don't ruin your project :)

The namespace is pretty important, if you don't use the default namespace, the Microsoft.Owin host can't find the startup. If for example, we added the App_Start namespace to the class:

	namespace NancyOwinWeb.App_Start
	{
	    using Owin;
	
	    public class Startup
	    {
	        public void Configuration(IAppBuilder app)
	        {
	            app.UseNancy();
	        }
	    }
	}

We will get an exception thrown...

![](/images/nancyfx-owin-8.png)

Luckily if you run into this scenario, you can either fix the namespace, or add a appSetting to your web.config like so:

	<add key="owin:AppStartup" value="NancyOwinWeb.App_Start.Startup, NancyOwinWeb" />

## Codez - Module

Now we just need a module, so lets create a nice simple module

	namespace NancyOwinWeb.Modules
	{
	    using Nancy;
	
	    public class HomeModule : NancyModule
	    {
	        public HomeModule()
	        {
	            Get["/"] = _ => "Hello from Owin!";
	        }
	    }
	}

If we ran the app now this would happen...

![](/images/nancyfx-owin-6.png)

Nice! What about my Web.config file, that file that gets so messy that we all dread...

	<?xml version="1.0"?>
	
	<!--
	  For more information on how to configure your ASP.NET application, please visit
	  http://go.microsoft.com/fwlink/?LinkId=169433
    -->
	
	<configuration>
	    <system.web>
	      <compilation debug="true" targetFramework="4.5" />
	      <httpRuntime targetFramework="4.5" />
	    </system.web>	
	</configuration>

That's it, believe it or not, I haven't changed a single line of this file at all!

Best of all our references are next to nothing!

![](/images/nancyfx-owin-7.png)

So awesome...