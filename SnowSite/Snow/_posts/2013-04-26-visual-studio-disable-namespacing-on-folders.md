---
layout: post
title: Disabling Namespaces on folders in Visual Studio
---

I just discovered this little trick I found in Visual Studio to turn off namespacing on a folder. Not sure how after 8 years I only just found this.

## The Problem
Lets assume we're working with Entity Framework... (shiver)... We create a Data Model, add all our Entities and away we go. 

Then we want to extend one of the Entities, maybe to add some methods and such, so we need to create some partial classes. So far we have:

![](/images/visual-studio-namespacing-1.png)

We create a new `Member.cs` class in the `Partials` folder;

<!--excerpt-->

	namespace SampleNamespacing.Models.Partials
	{
	    public partial class Member
	    {
	    }
	}

Then we delete the `Partials` to put it in the same namespace as the Entities defined in the Data Model

	namespace SampleNamespacing.Models
	{
	    public partial class Member
	    {
	    }
	}

But now we have a ReSharper warning

![](/images/visual-studio-namespacing-2.png)

Now we don't have a nice green file :(

![](/images/visual-studio-namespacing-3.png)

## The Solution!

This is so simple >< 

Right click the folder and go to Properties:

![](/images/visual-studio-namespacing-4.png)

BAM! Right there, `Namespace Provider`! Set that thing to `False` and we get:

![](/images/visual-studio-namespacing-5.png)

No more warnings! 

## Wheres this feature come from?

Turns out this is a ReSharper setting! This setting is stored in the `*project*.csproj.DotSettings` file. So if you're using team settings then this setting would be picked up by everyone on the team. 