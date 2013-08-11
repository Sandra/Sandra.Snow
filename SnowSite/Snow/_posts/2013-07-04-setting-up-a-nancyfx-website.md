---
layout: post
category: Azure
title: Setting up a NancyFX website
series:
	id: AzureMono
	current: 3
	part: Part 1 - Setting up the Virtual Machine and nginx
	part: Part 2 - Setting up new Website and Domain on nginx
	part: Part 3 - Setting up Mono on nginx
	part: Part 4 - Setting up a NancyFX website
	part: Part 5 - Setting up a ServiceStack web service
---

Time to setup NancyFX on Mono.

## Prelude! ##

This series is done using Mono 2.10 and .NET 4.0. This wont work with a 4.5 project since we need Mono 3.0 for that, but I plan to do another series on building Mono from source since there's no package available yet.

Also, this post assumes you've setup FTP to upload the files, I'm not going to go into detail, but you can install `vsftpd` and Google the setup. If you're new to Linux and followed Parts 1-3 so far, it should be easy enough to setup and install. All you need to do is authenticate using sshftp or sftp, rather than normal ftp. 

## Creating a Nancy test project ##

The easiest way to create a test project is to grab the [Nancy Templates](http://visualstudiogallery.msdn.microsoft.com/f1e29f61-4dff-4b1e-a14b-6bd0d307611a) from the Visual Studio Gallery. 

Using this method, we can create a new project in Visual Studio and select Nancy Application.

<!--excerpt-->

![](/images/setup-mono-on-ubuntu-part-4-1.png)

Don't forget to make this a .NET 4.0 project, NOT a .NET 4.5 project. When running Mono 3.0 you can choose .NET 4.5.

Once created press F5 and you should end up with a screen like so:

![](/images/setup-mono-on-ubuntu-part-4-2.png)

BUT, before we can publish, we need to update Nancy to the beta version (version 0.18.0) because 0.17.1 doesn't work on Mono 2.10. Hopefully if you're reading this shortly after its being published 0.18.0 will be out and you can.

-----

Updating the Nuget Packages. We need to add the Nancy CI builds to out options.

![](/images/setup-mono-on-ubuntu-part-4-4.png)

And we can remove the references from the project to `Nancy` and `Nancy.Hosting.Aspnet`

![](/images/setup-mono-on-ubuntu-part-4-5.png)

And now you can run the commands:

`install-package Nancy`

`install-package Nancy.Hosting.Aspnet`

![](/images/setup-mono-on-ubuntu-part-4-6.png)

-----

Now we can publish the website. I'm just going to publish to the file system since this is a learning exercise. 

## Deployment ##

Now that we've created and updated our Nancy project, published it. Now we can deploy it! 

![](/images/setup-mono-on-ubuntu-part-4-7.png)

So we can remove the html files we manually created in the previous posts, and deploy our Nancy website to the FTP. 

## Does it work? ##

If we visit the site now, most likely we will end up with:

![](/images/setup-mono-on-ubuntu-part-4-8.png)

This is because we originally configured our fastcgi index to be `index.aspx`

## Configure the default page ##

We can update our nginx configuration now, call `nano /etc/nginx/sites-available/default`

![](/images/setup-mono-on-ubuntu-part-4-9.png)

Remove the `index.aspx` part so the index is just `/` and nothing else.

Now we can restart nginx to make sure the changes are picked up... `service nginx restart`

## Now does it work?!? ##

Now we can give it a restart anddddddddddd.....

![](/images/setup-mono-on-ubuntu-part-4-10.png)

BAM! Now we have Nancy... Running on Mono... on nginx... Ubuntu... on Azure...

How awesome is that! 
 
