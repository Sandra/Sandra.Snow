---
layout: post
title: Windows Store App - Tiled Background using Mindscape Metro Elements
category: Windows 8 App
---

Apparently WinRT XAML lacks the ability to tile a background image, and surfing the net seems to come to the conclusion that the only way is to just create a large tiled image in photoshop...

I want to pick a Tile image from <http://subtlepatterns.com/> and just use it, but I *can't*. So I tried photoshop... (creating a benehoth of an image that is) I tried that, and it... crashed the Simulator, over and over... So I flagged it, instead I decided to put in a feature request over at Mindscape for their Metro Elements:

<http://www.mindscapehq.com/thinktank/suggestion/505262>

I asked them to include a TiledBackground, and they delivered! 

Currently in the nightly build is the makings of a TiledBackground element.

(To get access to the Nightly builds you will need to purchase a license for Metro Elements, which is well worth it, so go buy a license!)

After installing the Metro Elements, in your app you want to add a reference to Metro Elements:

![](/images/windows-app-tiled-bg-1.png)

Now in the page attributes you need to include an alias for the MetroElements:

<!--excerpt-->

    xmlns:ms="using:Mindscape.MetroElements"

Then you can add the TileBackground element:

    <ms:TileBackground ImageSource="/Assets/debut_dark.png" />
    
And that's all there is to it! How simple is that?

Now when you run the app, you should end up with the Hello World looking like:

![](/images/windows-app-tiled-bg-2.png)

It may be a little hard to see on the screen shot, but we now have a tiled background. The background used here is this image:

![](/images/windows-app-tiled-bg-3.png)

<span class="note">**Note:** Currently the tiled-background is not viewable in the designer, it can only be viewed when debugging. This may be fixed in the future, but it's a minor issue.</span>

Thanks a lot to the Mindscape team for implementing this feature so quickly. I love you guys!
