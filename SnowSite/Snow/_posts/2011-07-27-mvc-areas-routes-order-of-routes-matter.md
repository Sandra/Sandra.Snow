---
layout: post
title: MVC +Areas + Routes... Order of Routes Matter!
category: ASP.NET MVC
---

The order in which routes are registered, really is pretty damn important, otherwise it can have really strange side-effects.

I have a site, which has 3 areas, and no default... 'non-area'.

- Admin
- Members
- Site

So rather than having the structure:
 
![](/images/mvc-routes-1.png)

Where the root/default site is in the main directory, with two areas. I wanted this structure:

![](/images/mvc-routes-2.png)

So no root/default, and just a 'Site' area which would be the default.

<span class="note">**Note:**The default route is removed from Global.asax... for now</span>

<!--excerpt-->

This works perfectly fine if all the URL's are accessed like:

    http://localhost:147/Site/Home/Index

    http://localhost:147/Admin/Home/Index

    http://localhost:147/Member/Home/Index

If we run this in a browser we get the following:

![](/images/mvc-routes-3.png)

These routes are registered automatically when you add an Area, in a *AreaName*AreaRegistration file.

![](/images/mvc-routes-4.png)

The routes generated look like the following:

    namespace RoutingIssue.Areas.Members
    {
        public class MembersAreaRegistration : AreaRegistration
        {
            public override string AreaName
            {
                get
                {
                    return "Members";
                }
            }
            public override void RegisterArea(AreaRegistrationContext context)
            {
                context.MapRoute(
                    "Members_default",
                    "Members/{controller}/{action}/{id}",
                    new { action = "Index", id = UrlParameter.Optional }
                );
            }
        }
    }

But with different AreaName's of course. You get the idea.

Now what we want to do is change the Site route, to work from the root directly like:

    http://localhost:147/

This is where my assumptions began to go wrong... I updated the route like so:

    public override void RegisterArea(AreaRegistrationContext context)
    {
        context.MapRoute(
            "Site_default",
            "{controller}/{action}/{id}",
            new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
    
So I've removed 'Site' from the path, and added the default controller.

This is where things started to get messy. At first, it worked, in my sample project. Then I added AutoFac, MiniProfiler, some Content, and a few other things. That's when it all went pear shaped.

![](/images/mvc-routes-5.png)

It seems to have lost the information about the routes for Admin/Member.

Creating a new project with it working, and with my current project. I wrote some 
Trace.Write("AreaName") code into each AreaRegistration. The working project outputted:

- Members 
- Admin 
- Site

However the second project, where it was failing, outputted:

- Site 
- Members 
- Admin

So what's happening is in my project, it registers the default route first without the Area:

    context.MapRoute(
        "Site_default",
        "{controller}/{action}/{id}",
        new { controller = "Home", action = "Index", id = UrlParameter.Optional }
    );

Took me hours to figure this out. That MVC cannot guarantee the order of which Area's are registered. The second two Area's didn't stand a chance.

The solution was to move the Site route to the global.asax file, and specify an Area on it. My Site registration looks like:

    public override void RegisterArea(AreaRegistrationContext context)
    {
        //context.MapRoute(
        //    "Site_default",
        //    "{controller}/{action}/{id}",
        //    new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        //);
        Trace.WriteLine("Site");
    }

While my global.asax file has:

    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        routes.MapRoute(
            "default",
            "{controller}/{action}/{id}",
            new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            new string[] { "RoutingIssue.Areas.Site.Controllers" }
        ).DataTokens.Add("Area", "Site");
    }

This means the default route is registered after my Areas, and everything works perfectly again :)

