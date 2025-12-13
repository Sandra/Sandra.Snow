---
layout: post
title: Raygun.io - a few features that I love
category: Raygun
---

I've been using Raygun.io for a couple of months now and seen a whole heap of new features added.

### Email Spam
If you had multiple apps you would get spammed with 1 summary email each day for each app, now you get an aggregation of all apps which is much nicer. 

If you receive an email for an exception, and that exception keeps occurring, it will email you a little while later saying that you're still receiving the exception, the rate at which its occurring, and if it's happening more or less.

These changes are great! Still the same valuable information, but more smartly distributed to the user.


### Commenting
I had an error, that occurred a lot... 

![](/images/raygun-features-javascript-1.png)

So after fixing it, which took a little bit of effort to figure out, I commented on it!

<!--excerpt-->

![](/images/raygun-features-javascript-2.png)

This is awesome! This is really powerful if the exception reoccurs, and becomes even more powerful if they decide to implement [this suggestion](http://raygun.io/thinktank/suggestion/1012).

### The fact I get exceptions, when they happen!

I submitted my app to the Windows App Store, and it got declined, multiple times... But it allowed me to identity a bug in Microsoft Advertising SDK.

![](/images/raygun-features-javascript-3.png)

So it turns out there are random scenarios where the Advert Control, doesn't work and causes the app to crash hard. The dump that Microsoft supplies is absolute rubbish, it's massively time consuming and often doesn't give you enough information. 

With Raygun I was able to capture the errors as they occur and know where they were occurring. In the end I ended up removing the SDK from my app and submitting a ticket with MS to hopefully fix it.

### Conclusion

I don't think I've ever cared about exceptions in my applications in the past 8-9 years as much as I do now. Raygun just makes it so much easier to deal with that I care about whats happening in my application now.

I'm a little blown away that I care about something other than myself. :P