---
layout: post
title: Finding the correct Process Id for a website for DotTrace
---

Finding which process belonged to which application pool was simple in IIS6, it was a simple case of running up a Command Prompt and typing in `iisapp.vbs`.

Unfortunately the script wasn't bought across to IIS7. So when it came to profiling a site I was baffled as to why the DotTrace results were showing information for threads completely unrelated to the website I was looking at.

Turns out I was looking at the wrong Process Id...

![](/images/process-1.png)

So I needed to figure out which Process Id belonged to which site.

<!--excerpt-->

IIS7 ships with a little command line tool called `AppCmd`. Which is handy, simply open up a Command Prompt and navigate to:

    C:\Windows\System32\inetsrv>

Then type in the command:

    appcmd list wp

This will list out all the sites and their Process Id:

![](/images/process-2.png)

Problem solved!

More information about the tool can be found here:

<http://learn.iis.net/page.aspx/114/getting-started-with-appcmdexe/>
