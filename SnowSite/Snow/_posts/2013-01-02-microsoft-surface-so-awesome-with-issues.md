---
layout: post
title: Microsoft Surface... So awesome, with issues...
category: Rant
---

Shortly after the Microsoft Surface was released I got my hands on one, which wasn't exactly easy since they aren't available in Singapore yet. I had to pay $240 extra to get mine shipped from Hong Kong via <http://www.expansys.com.sg> and then an extra $70 for import tax... 

But it was 110% worth it. I use the thing ALL the damn time, and dispite it being limited in functionality due to Windows RT, I don't regret the purchase at all. 

There's HEAPS of good reviews on the internet about it so I'm not going to bore you with how amazingly awesome the thing is. 

What I want to discuss is 3 issues I have.

## Surface Charger

![](/images/surface-issues-1.png)

The Surface Charger has a Magnetic Power Plug, similar to the old L-shaped MagSafe 

![](/images/surface-issues-2.png)

<!--excerpt-->

The problem is, just like the old L-shaped MagSafe, it's a PILE OF SHIT. It's the most fiddly thing to try and connect up, and this isn't helped by the fact the Surface has a tappered edge so you need to angle it right for it to snap in. 

Unlike the keyboard cover which snaps on and positions itself, the charger tries to snap itself out of the hole, then you end up wriggling it around to try get it in.

With the amazing build quality, how they cheaped out on the charger, I have no idea.

This leads me onto #2

## Windows Store - Unavailable to certain regions.

Ok so I completely understand that MS doesn't want to ship Surfaces to some countries like Singapore. So on their website I can't purchase a Surface...

But they could atleast allow me to ship a new charger to Singapore. I can't walk into a store, I can't ship one here. I have to use a 3rd party to order it from the MS website, have it shipped to a US address and forwarded to my address.

Just ship it to me directly. It's 2013 and we still can't ship stuff anywhere. 

Last up is my biggest peeve. 

## Microsoft restricting drivers...

Over Christmas I went to Penang in Malaysia, this country is so 3rd world it's the first time I've ever stayed in a hotel that did not have wifi. 

The hotel room DID have ethernet however, but the Surface lacks a LAN port. So in order to setup the internet so I could surf the net while my friends sleep. (I don't sleep much) I decided to buy a USB to LAN adapter. 

![](/images/surface-issues-3.png)

I own both of these, the first one (black) is an Asus USB Lan Adapter, it came with my Asus UX31 Laptop, the 2nd one is some weird brand called VZTEC.

The thing you can't tell about both these is they are IDENTICAL. When you plug the devices in:

![](/images/surface-issues-4.png)

![](/images/surface-issues-5.png)

The first image is using the White, random brand adapter. The second image is using the Asus one, both have the exact same chipset. So the drivers can be used for both. From googling it seems there's a lot of USB to LAN Adapters that sport this chipset.

However, if you plug these into Windows 8 (Not Windows RT) windows will automatically find and install the drivers for you. If you attempt to do the same on Windows RT, it fails to install and you are unable to use it.

Apparently these drivers are removed from ASIX's website at the request of Microsoft. 

This REALLY pisses me off, seriously what the fuck is Microsoft thinking, asking for drivers to be REMOVED so principles don't work on Windows Surface / Windows RT? 

One has to wonder what the hell goes on in Redmond, Microsoft constantly makes really stupid choices. Look at their fragmentated Windows Live ID, lack of support for multiple Live accounts on devices like Windows 8 / Windows Phone which prevent you from signing out of Metro Skype or messaging contacts that exist on different Windows Live account.

I'm so sick of Microsoft doing stupid things like this, soooo here's the drivers for [AX88772B / 772A / 772 for Windows RT](/stuffz/AX88772B_772A_772_WinRT_Driver_v3.16.0.1807.zip)

[Right Click and Save](/stuffz/AX88772B_772A_772_WinRT_Driver_v3.16.0.1807.zip)

To use the drivers, unzip the above file onto your desktop or somewhere.

Plugin the adapter, and go into Device Manager, find the device shown in the pictures above. (will have a weird icon since it's not working) go into properties / drivers and then manually browse to the driver folder. 

This will install the Windows RT driver and the adapter will now be usable.

****

This concludes my rant with Microsoft Surface.

It truely is an amazing product. Just wish Microsoft would stop doing such silly things.
