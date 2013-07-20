---
layout: post
category: Azure
title: Setting up a ServiceStack Service
---

* [Part 1 - Setting up the Virtual Machine and nginx](/2013/06/setting-up-ubuntu-and-nginx-on-azure/)
* [Part 2 - Setting up new Website and Domain on nginx](/2013/06/setting-up-a-new-website-and-domain-on-nginx)
* [Part 3 - Setting up Mono on nginx](/2013/06/setting-up-mono-on-nginx/)
* [Part 4 - Setting up a NancyFX website](/2013/07/setting-up-a-nancyfx-website/)
* Part 5 - Setting up a ServiceStack web service

Time to setup a ServiceStack service!

## Prelude! ##

ServiceStack is great, specially when you need to support .NET 3.5 and don't have the pleasure of being able to use .NET 4.0/4.5, you're not limited to .NET 3.5, ServiceStack works great with .NET 4.0/4.5 as well!

God forbid we get subjected to having to use WCF...

Setting this up should be super easy, we will use all the same settings as NancyFX, the only difference is we will be running ServiceStack instead. 

The ServiceStack team take real care to ensure that it works on Mono, enough so that their website; <http://www.servicestack.net>, runs on Linux!

<!--excerpt-->

## Creating the Sample Project ##

One of the great things about ServiceStack is it has a bunch of awesome examples. Head on over to <https://github.com/ServiceStack/ServiceStack.Examples> and download the samples. Either by downloading the repository, or cloning it. 

![](/images/setup-mono-on-ubuntu-part-5-1.png)

After downloading the ServiceStack Examples, navigate to `src` and open the `ServiceStack.Hello` project. 

![](/images/setup-mono-on-ubuntu-part-5-2.png)

Running this project we should end up with a small Service Stack sample website like so:

![](/images/setup-mono-on-ubuntu-part-5-3.png)

## Quick local test ##

Navigating to the URL; `http://localhost:62577/servicestack/metadata` we should get the nice meta data screen:

![](/images/setup-mono-on-ubuntu-part-5-4.png)

And navigating to the URL: 

`http://localhost:62577/servicestack/hello/phillip`

We end up with a response:

![](/images/setup-mono-on-ubuntu-part-5-5.png)

We do need to make one minor change. In the root project of the folder, rename the `default.htm` file to `index.html`. This is only so we don't need to modify nginx to look for the default file `default.htm` :) #lazyme

## Lets deploy it! ##

So like the Nancy project, we can publish the project to a folder. The good thing about this sample is it doesn't need any database or anything like that. It's just a straight website with a Hello World service.

![](/images/setup-mono-on-ubuntu-part-5-6.png)

Once deployed we should end up with the ServiceStack site again!

![](/images/setup-mono-on-ubuntu-part-5-7.png)

Navigating to the metadata page we end up with the same screen we had locally!

![](/images/setup-mono-on-ubuntu-part-5-8.png)

And navigating to the test URL we end up with the fancy ServiceStack response! 

![](/images/setup-mono-on-ubuntu-part-5-9.png)

Hello, with love from nginx!

That's it, using the best web frameworks, NancyFX and ServiceStack, is super duper happy path easy with Mono!

:)