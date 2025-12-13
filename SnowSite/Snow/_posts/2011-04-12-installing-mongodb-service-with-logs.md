---
layout: post
title: Installing MongoDB Service with logs
category: MongoDB
---

Just tried to install MongoDB as a service on Windows 7, but came up with issues trying to specify where to store the logs...

<http://www.mongodb.org/display/DOCS/Windows+Service>

The example on the website shows a directory path, but when you look at the help in the command windows it says:

![](/images/mongodb-install-1.png)

> all output going to: c:mongodblogs 
> logpath [c:mongodblogs] should be a file name not a directory

I'm not sure why, maybe the documentation isn't up to date, but specifying a physical file, like 'log.txt' the service installs fine.
