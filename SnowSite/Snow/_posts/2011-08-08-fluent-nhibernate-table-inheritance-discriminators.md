---
layout: post
title: Fluent NHibernate - Table Inheritance - Discriminators
category: NHibernate
---

So a long time ago [James Kovacs](http://jameskovacs.com/) posted a article about [get/load polymorphism](http://nhforge.org/blogs/nhibernate/archive/2011/02/16/get-load-polymorphism-in-nhibernate-3.aspx) with NHibernate, which was cool and all but I always wanted to know how to map it all in Fluent NHibernate. I worked it out at the time but I guess it's taken me 7 months to write it down.

First up is using a single table, mapping them to multiple classes, this is done using a discriminator. Fluent NHibernate calls this 'table-per-class-hierarchy strategy', which doesn't make sense to me. But meh.

So I'm going to begin with the following classes to demonstrate this:

![](/images/fnh-descrim-1.png)

So if I was to select all WallPost's it would give me instances of LinkShare, and Text wall posts.

These classes are really basic at the moment.

    public class WallPost
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime DatePosted { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
    }
    
    public class TextWallPost : WallPost
    {
    }

    public class LinkShareWallPost : WallPost
    {
    }
    
<!--excerpt-->

First we map the Wall Post:

    public class WallPostMap : ClassMap<WallPost>
    {
        public WallPostMap()
        {
            Table("WallPost");
            Id(x => x.Id).GeneratedBy.GuidComb();

            DiscriminateSubClassesOnColumn("PostType");

            Map(x => x.DatePosted);
            Map(x => x.Title);
            Map(x => x.Content);
        }
    }

As you see, there is a 'DescriminateSubClassesOnColumn' method which specifies a column. This is what NHibernate uses to figure out which type of Sub Class to create.

Next we need to map the Sub Classes.

    public class TextWallPostMap : SubclassMap<TextWallPost>
    {
        public TextWallPostMap()
        {
            DiscriminatorValue(1);
        }
    }
    
    public class LinkShareWallPostMap : SubclassMap<LinkShareWallPost>
    {
        public LinkShareWallPostMap()
        {
            DiscriminatorValue(2);
        }
    }

Here I have specified the Discriminator Value for each sub class. If I save a TextWallPost, it will save the value '1' to the column 'PostType'. Then when it pulls it from the database, it uses this value to decide the SubClass to create.

The generated table looks like:

![](/images/fnh-descrim-2.png)

Now if I insert a couple of posts:

    using (var tx = session.BeginTransaction())
    {
        var wallPost = new TextWallPost
        {
            DatePosted = DateTime.Now,
            Title = "My First Wall Post",
            Content = "Is Awesome!"
        };
        var linkPost = new LinkShareWallPost()
        {
            DatePosted = DateTime.Now,
            Title = "My First Link Share",
            Content = "Is Awesome!"
        };
        session.Save(wallPost);
        session.Save(linkPost);

        tx.Commit();
    }

I've done nothing more than create instances of the classes I want, and commit them to the database, the SQL that is generated looks like:

![](/images/fnh-descrim-3.png)

So when it generates the SQL it puts the discriminator value in for us.

If I select all 'WallPost' from the database:

    var result = session.QueryOver<WallPost>().List();
    
    foreach (var wallPost in result)
    {
        Console.WriteLine(wallPost.Title);
    }

It just does a normal select:

![](/images/fnh-descrim-4.png)

Nice right? What's cool is if we look at the list of results we get, we get instances of WallPost, and instances of LinkShareWallPost back:

![](/images/fnh-descrim-5.png)

Now suppose we wanted to select JUST Link Shares.

    var result = session.QueryOver<LinkShareWallPost>().List();
    
    foreach (var wallPost in result)
    {
        Console.WriteLine(wallPost.Title);
    }

NHibernate will actually write the query to select only WallPosts that specify the Discriminator type defined in our Sub Class mapping.

![](/images/fnh-descrim-6.png)

So if we look at the results in VS:

![](/images/fnh-descrim-7.png)

Very nice stuff indeed.

There's future posts on this stuff to come :)
