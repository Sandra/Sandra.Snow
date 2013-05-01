---
layout: post
title: MVC Routing with Attributes makes routing awesome
category: ASP.NET MVC
---

I evaluated HEAPS of code/libraries and threw in the towel and decided that getting lower-case routes, while still having Area's, was a complete and utter waste of time and effort because they were all crappy, and broke stuff, or didn't work.

That was until my mate Brad grabbed [AttributeRouting](https://github.com/mccalltd/AttributeRouting) from [Nuget](http://nuget.org/List/Packages/AttributeRouting). Oh well you know what, this is the best thing sliced bread.

First and foremost, it solved the number 1 issue I had with routing, lowercase Urls. This is how simple is it to make Routes lowercase.

    routes.MapAttributeRoutes(config =>
    {
        config.UseLowercaseRoutes = true;
    });
    
Wow... Yet you bing for lowercase routes, and you end up with strange solutions... Here's a couple.

<http://stackoverflow.com/questions/878578/how-can-i-have-lowercase-routes-in-asp-net-mvc>

This question links to:

<http://coderjournal.com/2008/03/force-mvc-route-url-lowercase/>

This breaks Area's, why? Because to handle an Area it appends `?Area=areaname` to the Url. FAIL. Probably worked great for MVC 1,but considering I've seen it linked in MVC 3 questions,I consider it fail.

<!--excerpt-->

<http://stackoverflow.com/questions/878578/how-can-i-have-lowercase-routes-in-asp-net-mvc/1731652#1731652>

Writing them in lowercase? Really?

Pass, I'll stick with AR, it's too simple, it's ridiculously simple. I love it!

The next thing I love, is that instead of writing Routes, and then writing `HttpGet` `HttpPost` on all the actions, I just have to write `GET` and `POST` on the actions, and even define the URL along with it, and bam, it's clear and explicit how you get to that action.

Such as:

    [POST("Auth/Login")]
    public ActionResult Login(LoginViewModel loginViewModel, string returnUrl)
    
It's obvious to me, that I can access this Action by navigating to www.mysite.com/auth/login

    [GET("Content/{slug}")]
    public ActionResult Index(string slug)
    
Page content can be accessed via www.mysite.com/content/my-slug

And when controllers are divided into Area's, I can just define the area with the controller class:

    [RouteArea("Admin")]
    public class HomeController : BaseController

I could define the namespace to use for Area's in the Area Registration, but it just seems... hacky... AR is definitely worth checking out.
