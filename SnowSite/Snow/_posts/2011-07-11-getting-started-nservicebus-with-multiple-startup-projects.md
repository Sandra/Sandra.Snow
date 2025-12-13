---
layout: post
title: Getting Started - NServiceBus with Multiple Startup Projects
category: NServiceBus
---

I never actually realized this, but one of the things that baffled me was how Udi Dahan, when giving his training, or when playing with the NServiceBus demo's... How multiple projected were started.

It actually made implementing NServiceBus somewhat difficult for me to begin with, because I never knew Visual Studio had a feature to specify multiple projects as startup projects.

![](/images/getting-started-nservicebus-1.png)

If you right click a Solution and go to 'Properties' you are presented with the above window.

In "Startup Project" there's an option to select multiple projects, this solution which is a NServiceBus demo, shows 3 projects set to start.

<!--excerpt-->

Ahhh it all makes sense now. 7 years of using Visual Studio and I didn't know this feature existed. All this time I had being starting projects manually from the bin directories thinking "there must be an easier way".

Yet if I Googled "visual studio multiple startup projects" the first link is:

[How to: Set Multiple Startup Projects](http://msdn.microsoft.com/en-us/library/ms165413(v=vs.80).aspx)

Ah we live and we learn :)

