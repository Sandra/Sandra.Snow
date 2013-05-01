---
layout: post
title: Using Partial Renders with Nancy rather than Render Actions like MVC
category: NancyFX
---

Today on twitter, <a href="http://blog.orangelightning.co.uk/">Phil Jones</a> (<a href="http://twitter.com/philjones88">@philjones88</a>) asked how you would do RenderAction (ASP.NET MVC extension)

I personally don't like RenderAction in MVC, that's not to say it's bad, I just think it hides away important implementation of a page render. I think RenderAction is a bad abstraction. But this is my personal opinion. So don't hate me for it :)

Rather than using RenderAction I prefer to use Partial views.

Lets look at a youtube site for an example:

<img src="/images/partial-renders-nancy-1.png" />

On the left hand side is main content, while on the right hand side is related videos. In ASP.NET MVC we might use something like:

    @Html.RenderAction("Related", "Videos");

Or something similar, I forget the syntax :)

<!--excerpt-->

This would use the Query String to get the Video Id and pull all related videos and display them.

In Nancy if I was building the same page, I would push down a dynamic model:

    @inherits Nancy.ViewEngines.Razor.NancyRazorViewBase<dynamic>

Then in the Module, query for the video + the recommended data:

    public class VideosModule : RavenModule
    {
        public VideosModule(IDocumentStore documentStore) : base(documentStore)
        {
            Get["/videos/{id}"] = _ =>
            {
                Model.Videos = DocumentSession.Load<Video>(_.id);
                Model.Recommended = DocumentSession.Query<Video, Videos_Recommended>().Where(x => x.VideoId == _.id);
                return View["display-video", Model];
            };
        }
    }

Model is implemented as an ExpandoObject on the RavenModule:

    public abstract class RavenModule : NancyModule
    {
        protected dynamic Model = new ExpandoObject();

Now on my display-video view, I add:

    @Html.Partial("recommended", Model.Recommended)

This 'recommended' view is the same as the 'display-video' view, in that it uses dynamic also.

That's all there is to it.

In regards to information displayed on the Master Page (or master layout, what ever you want to call it) Because the 'Model' is defined on the base module, you can implement properties for menu, footer, member details, etc. And still use Partials to render those also.

I'll add a Git Repo for this project in the near future.