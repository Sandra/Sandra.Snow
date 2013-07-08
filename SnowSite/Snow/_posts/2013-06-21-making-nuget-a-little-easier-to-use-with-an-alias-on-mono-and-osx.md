---
layout: post
category: OSX and Mono
title: Making NuGet a little easier to use with an Alias
---

So I've been in hospital all week with Mycoplasma Infection (or AIDs, doctor says otherwise tho), and figured it would be a good time to mess around with Mono. 

So I began by testing WorldDomination.Web.Authentication

While it works GREAT! (Future post maybe) I did run into one issue early on, that is restoring packages. 

I found a [great post](http://orientman.wordpress.com/2012/12/29/for-the-record-how-to-run-nuget-exe-on-os-x-mountain-lion/) by [&#64;orientman](https://twitter.com/orientman) about using NuGet on OSX.

<http://orientman.wordpress.com/2012/12/29/for-the-record-how-to-run-nuget-exe-on-os-x-mountain-lion/>

But I didn't like having to type the following out each time, since I needed to install a few different project packages.

<!--excerpt-->

	mono --runtime=v4.0.30319 NuGet.exe install ./Code/WorldDomination.Web.Authentication/packages.config -OutputDirectory ./packages

All the time, it seemed tedious. So what I did was, moved the NuGet.exe to `~/Tools/`

![](/images/mono-osx-alias-1.png)

Next in [iTerm 2](http://www.iterm2.com/) (I prefer it over Terminal) I set an alias for the common stuff!

	alias nuget='mono --runtime=v4.0.30319 /Users/phillip/Tools/NuGet.exe'

Now rather than having to type out all that stuff each time, I can just type out `nuget`

	nuget install ./Code/WorldDomination.Web.Authentication/packages.config -OutputDirectory ./packages

![](/images/mono-osx-alias-2.png)

It also means I can forget about the NuGet.exe file or where I've put it :D

If you want to check what aliases you have you can run `alias -p`, and if you want to remove the alias you can call `unalias *name*`, e.g `unalias nuget`

