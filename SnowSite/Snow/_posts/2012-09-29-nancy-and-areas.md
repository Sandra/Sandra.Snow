---
layout: post
title: NancyFX and Areas
category: NancyFX
---

A while back <a href="https://twitter.com/ayende">@ayende</a> wrote a <a href="http://ayende.com/blog/156609/reviewing-dinner-party-ndash-nerd-dinner-ported-to-ravendb-on-ravenhq">blog post</a> about <a href="https://github.com/NancyFx/DinnerParty">DinnerParty</a> in which he mentioned <a href="http://nancyfx.org/">NancyFX</a>, a lightweight web framework. <a href="https://twitter.com/jchannon">@jchannon</a> recently wrote about a <a href="http://blog.jonathanchannon.com/2012/09/21/nancyfx-ravendb-nerddinner-and-me/">blog post about NancyFX RavenDB and himself</a>.

So I've spent the better part of the last few months learning it, and needless to say, it's awesome. So I got asked a question, how would I handle 'Areas' with Nancy, like we would with ASP.NET MVC?

Well it's pretty simple, all it really consists of is... a module containing a root path.

Lets begin with the following folder structure defining a 'HomeModule' for both the Root Website, and the Admin section.

<img src="/images/nancy-areas-1.png" />

Both these HomeModule classes are identical, with the small exception that the Admin one specifies a Root Path.

<!--excerpt-->

### Root > HomeModule ###

    namespace NancyAreasDemo.Modules
    {
        public class HomeModule : NancyModule
        {
            public HomeModule()
            {
                Get["/"] = _ =>
                {
                    return "Website Home";
                };
            }
        }
    }

### Root > Admin > HomeModule ###

    namespace NancyAreasDemo.Modules.Admin
    {
        public class HomeModule : NancyModule  
        {
            public HomeModule() : base("admin")
            {
                Get["/"] = _ =>
                {
                    return "Admin Home";
                };
            }
        }
    }

When we run this in a browser now we can get both the Main Website and the Admin section:

<img src="/images/nancy-areas-2.png" />

Now let's add some Views.

<img src="/images/nancy-areas-3.png" />

So for the Area 'Admin' has it's own folder under the Views folder, with it's own 'Home' folder matching the Module name.

This is very similar to using Area's in ASP.NET MVC.

Now we need to add some ViewLocationConventions:

    protected override void ConfigureConventions(NancyConventions nancyConventions)
    {
        nancyConventions.ViewLocationConventions.Insert(0, (viewName, model, context) =>
        {
            if (string.IsNullOrWhiteSpace(context.ModulePath))
                return null;
            return string.Concat("views/", context.ModulePath, "/", context.ModuleName, "/", viewName);
        });

        base.ConfigureConventions(nancyConventions);
    }

This is configured in the Bootstrapper (<a href="https://github.com/NancyFx/Nancy/wiki/Bootstrapper">read Nancy Bootstrapper docs here</a>) (<a href="https://github.com/NancyFx/Nancy/wiki/View-location-conventions">read the Nancy View Location Convention docs here</a>)

<span class="note"><strong>Note:</strong> Hoping to get this convention included into Nancy :) if <a href="https://twitter.com/thecodejunkie">@TheCodeJunkie</a> will accept the PR</strong>

<span class="note"><strong>Note 2:</strong> The PR was accepted into 0.13, but you can still create your own conventions.</strong>

This convention basically tells Nancy to look firstly, for the View in:

views/***module path***/***module name***/***view name***

If you don't use this convention then you will need to remove the 'Home' directory from the 'Admin' folder and place all your views in there. Which isn't convenient when you have multiple modules in that area.

Now in the views we can put some sample content:

### Admin HTML Page ###

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title></title>
    </head>
    <body>
        This comes from Admin view.
    </body>
    </html>

### Root HTML Page ###

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title></title>
    </head>
    <body>
        This comes from the root view.
    </body>
    </html>

Now when we run the pages:

<img src="/images/nancy-areas-4.png" />

And that's it. Nice quick simple Area's using Nancy!

You can view the <a href="https://github.com/phillip-haydon/NancyAreasDemo">GitHub repository</a> for the demo used to write this blog post.