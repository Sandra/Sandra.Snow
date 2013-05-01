---
layout: post
title: Introducing Raygun.io and Nancy.Raygun
category: NancyFX
---

So not long ago I [posted](/2012/10/keep-an-eye-on-raygun-to-zap-all-your-errors-away/) about an up and coming service from <http://www.mindscapehq.com> called [Raygun](http://www.raygun.io), well last week they went into beta, and I was invited, so since early last week I've been giving their system the run around and provided a little bit of feedback. 

I say a little bit, because besides being a super early beta tester, and a super early product that will grow over time, the beta is quite polished. There was only a few minor issues which they corrected within a day or less. 

![](/images/raygun-beta-robby-1.png)

Raygun is super easy to setup and implement into your application, in-fact it's really nothing more than creating an app, install the nuget package, and set the key, and away you go. 

There's a simple guide over <http://www.pieterg.com/2013/1/raygunio-has-launched>, so I won't bore you with the same content.

## Nancy.Raygun

Currently the [offical Nuget package](https://www.nuget.org/packages/Mindscape.Raygun4Net/) has a dependency on System.Web which isn't very nice for Nancy, so I've created [Nancy.Raygun](https://www.nuget.org/packages/Nancy.Raygun/)

It has no dependency on `System.Web`, and uses the `NancyContext` rather than `HttpContext.Current`, it also implements `IApplicationStartup` so that it automatically wires itself up for handling errors for you.

Nancy.Raygun is on [Github](https://github.com/phillip-haydon/Nancy.Raygun), installing the nuget package will add the web.config for you so all you need to do is add the key and away you go.

## Why Raygun?

I've had a few people ask me why I would use Raygun over ELMAH, or AppFail. 

<!--excerpt-->

ELMAH means I have to manage all the exceptions myself, I have to specify where I log it, navigate a clunky unstyled website, besides being an awesome alternative to something like log4net or nlog, it really is out-dated by today's standards.

I have no personal experience with AppFail, and their service is similar, but the main difference is I know Mindscape, they are an awesome team that produce an awesome set of tools like LightSpeed, Web Workbench, and even went out of their way to add a control to their Metro Elements for me. I know what I'm getting is an awesome and will only get more awesome as time goes on.

So right now, I've ripped out ELMAH from all my projects and replaced it with Raygun, and I'm not looking back at all. 