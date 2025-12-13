---
layout: post
title: Windows Store App and Remote Debugging on a Surface RT
category: Windows 8 App
---

In order to run your app from Visual Studio on your remote device, in my case I want to run it on my Surface... but you can do this with any remote device such as another desktop, a laptop, a tablet running Windows Pro or RT etc, all you really need to do is install the Remote Debugging tools.

<a href="http://www.microsoft.com/visualstudio/eng/downloads#remote-tools">http://www.microsoft.com/visualstudio/eng/downloads#remote-tools</a>

For the Surface you just need to download and install:

<a href="http://www.microsoft.com/visualstudio/eng/downloads#remote-tools"><img src="/images/image17.png" /></a>

Once this is installed you will get a little icon on your Start Screen:

<a href="/images/image7.png"><img src="/images/image7.png" /></a>

You can see the Green Arrow:

<!--excerpt-->

<img src="/images/image18.png" />

When you run this for the first time it will ask you to configure some stuff, mainly for the firewall:

<img src="/images/image19.png" />

You only need to do this once. Once this is done you will get the following screen:

<img src="/images/image20.png" />

Now you're all ready for some awesome remote debugging!

In Visual Studio you need to select 'Remote Machine' in the debug list:

<img src="/images/image21.png" />

This opens up a remote debug connection:

<img src="/images/image22.png" />

Enter in the IP for the Surface. (You can get this by calling 'ipconfig' in command line) After pressing select the window will close, and you will be ready!

Now you just need to debug the app. Using the same app from the previous post I've put a break point on the MainView constructor:

<img src="/images/image23.png" />

When I begin debugging makes a request to the device and authenticates:

<img src="/images/image24.png" />

<span class="note"><strong>Note:</strong> If you're debugging on a device that is logged in with a different user, then Visual Studio will prompt you for credentials. Because I'm using the same LiveID on both my Desktop and Surface, it doesn't prompt me to login.</strong>

Once connected, it will install the app just like it does on your desktop, and then automatically runs and debugs the app:

<img src="/images/image25.png" />

And as you can see:

<img src="/images/image26.png" />

My shinny Surface screen running the Hello World sample from the previous post.

Debugging allows you to step through code just the same as doing it locally :)

Hopefully this helps anyone who has had difficulty or confusion debugging on remote devices.
