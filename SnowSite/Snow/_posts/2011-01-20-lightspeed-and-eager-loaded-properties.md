---
layout: post
title: LightSpeed and Eager Loaded properties
category: LightSpeed
---

Today [Mindscape](http://www.mindscapehq.com/), the makers of the ORM [LightSpeed](http://www.mindscapehq.com/products/lightspeed) and other awesome [products](http://www.mindscapehq.com/products/) posted a blog on [Lazy-Loading/Eager-Loading properties](http://www.mindscapehq.com/blog/index.php/2011/01/19/controlling-lightspeed-entity-loading-with-named-aggregates/) with LightSpeed.

Why can't NHibernate do this! And when I say why can't NHibernate do this, I mean, why can't NHibernate do this with Query/QueryOver.

Ayende detailed this time last year, the ability to [lazy-load properties](http://ayende.com/Blog/archive/2010/01/27/nhibernate-new-feature-lazy-properties.aspx), but currently Query/QueryOver only support One-To-Many and One-To-One relationships.

NHibernate will support the ability to Lazy-Load a property but you can't specify it to be eager loaded.

So you do

    Map(x => x.FullDescription).CustomSqlType("NTEXT") 
                               .LazyLoad();

Running a query would result in:

![](/images/lightspeed-1.png)
 
Great... But you currently cannot do:

<!--excerpt-->

    var video = session.QueryOver<Video>()
                       .Fetch(x => x.FullDescription).Eager
                       .List();
 
This will execute, the exact same query.

I do have one gripe with the way LightSpeed works tho, and that is that you have to give your aggregate as a string name, I really think they should give the ability to specify the property(ies) via a lambda.

Without giving it a test drive, I feel if I changed the name of the aggregate and forgot to update everywhere it was referenced, I'm going to end up with a maintenance nightmare.

Anyway checkout LightSpeed, it's awesome, it's everything Entity Framework should have been.

<http://www.mindscapehq.com/products/lightspeed>
