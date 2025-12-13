---
layout: post
title: Windows Store App with Caliburn.Micro - Getting Started (Updated)
category: Windows 8 App
---

<span class="note">**Note:** This is an update post for [Windows Store App with Caliburn.Micro - Getting Started](/2012/12/windows-store-app-with-caliburn-micro-getting-started/)</span>

Back in December I blogged about [getting started](/2012/12/windows-store-app-with-caliburn-micro-getting-started/) with Caliburn.Micro, not long after I created the post a [new version](http://devlicio.us/blogs/rob_eisenberg/archive/2013/01/20/caliburn-micro-v1-4-1-released.aspx) (v1.4.1) was released.

The changes in this release break my previous blog post so I'm updating here.

<span class="note">**Note:** The original post still applies, the change is only to the setup of the App</span>

## App Setup

In the previous post when we configured the container, all we needed to do was Register the WinRT Services.

<!--excerpt-->

    protected override void Configure()
    {
        container = new WinRTContainer();
        container.RegisterWinRTServices();
    }

However, the default container no longer auto creates concrete types, which causes View Models to not auto create. To fix this we do one of two things.

## Manually Register Types 

In the Configure method, we can manually register the View Model like so

    protected override void Configure()
    {
        container = new WinRTContainer();
        container.RegisterWinRTServices();
        container.PerRequest<MainViewModel>();
    }

Running the app, the View Model should now be created when you navigate to the View. The downside to this approach is that you need to do this for every view model which can be tedious.

## Automate the Registration

<span class="note">**Note:** This approach came from the samples.</span>

Instead of manually registering, we can automate it by over-riding the `GetInstance` method and manually registering the type if it doesn't exist.

    private static bool IsConcrete(Type service)
    {
        var serviceInfo = service.GetTypeInfo();
        return !serviceInfo.IsAbstract && !serviceInfo.IsInterface;
    }

    protected override object GetInstance(Type service, string key)
    {
        var obj = container.GetInstance(service, key);

        // mimic previous behaviour of WinRT SimpleContainer
        if (obj == null && IsConcrete(service))
        {
            container.RegisterPerRequest(service, key, service);
            obj = container.GetInstance(service, key);
        }

        return obj;
    }

In this example we attempt to get the instance, if it doesn't exist, but it's a concrete type, then we register it and then Get/Return it. 

And that's it :) 

