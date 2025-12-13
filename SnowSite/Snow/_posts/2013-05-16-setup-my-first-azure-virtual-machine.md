---
layout: post
title: Setup my first Azure Virtual Machine!
category: Azure
---

Every time I touch Azure I'm constantly amazed at how much simpler it is compared to when I tried it back when it had the crappy Silverlight management site.

Infact every time I touch the thing I over-complicate something only to realize it was dead simple to begin with.

I'm currently in the process of slowly building a VM to run a few small websites I'm building in my own time. Based on what I need Azure will cost me $60 less each month for roughly the same specs (with about 256mb less ram)

The first time creating a VM took me a couple of hours to setup and working, nuking it and starting again took about as long as it took to provision the VM (few minutes)

<!--excerpt-->

## Things I did wrong :(

 1. When I setup the DNS I used the Internal IP Address instead of the Public Virtual IP address. 
    
    ![](/images/azure-vm-1.png)

 2. After sorting out the DNS so ping hit the right IP Address, I needed to add an Endpoint for port 80. Luckily when IIS installs on Windows Server 2012 it configures the Firewall for you. I don't recall it doing that for you on 2008. Needless to say, once the Endpoint was in place, websites became visible!

	![](/images/azure-vm-2.png)

 3. I didn't want to store my websites, and images/videos on the OS Drive so I needed to create a 2nd drive. Initially I went fluffing around in Storage, creating a new storage and setting blob read/write etc etc... Turns out I wasted about 20 minutes of my time to find out...

    ![](/images/azure-vm-3.png)

    There's a button to add an empty disk to a selected virtual machine... Arg, well that turned out to be REALLY simple!

 4. I kind of expected the disk to automatically show up in the VM, but then I remembered watching some video on Azure VMs which said you need to enable/format the disk manually. /facepalm

    So once again jump into the VM, go to the disk management screen and enable/format, now I got storage ready to go!

## Conclusion

Weather I'm messing around with Serivces, Website, Virtual Machines. Azure has come a long way and its such a pleasure to work with! I was pretty negative towards Azure when it had the Silverlight management screen because it was slow and I couldn't figure out how to do anything. Now its a breeze!