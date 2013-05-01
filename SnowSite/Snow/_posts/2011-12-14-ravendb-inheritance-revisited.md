---
layout: post
title: RavenDB Inheritance-Revisited
category: RavenDB
---

So after my initial post on [RavenDB Inheritance](/2011/12/ravendb-inheritance/), and the issue I had with polymorphic queries, and seeking help from the guys in [JabbR](http://jabbr.net/) and the (RavenDB Google Group)[http://groups.google.com/group/ravendb/browse_thread/thread/c71df8f1cd92e04c], [Ayende](http://ayende.com/blog/) ended up doing a screen cast with me where he solved all my problems.

One of the things he asked me was what I was trying to achieve by having a polymorphic query, which was a very good question, something I hadn't really thought about.

The problem I was trying to solve was actually displaying search results.

## The Problem

So I'm working on a personal project, and I need to display a few things which are similar, but different. There's 3 different types but I'll use two to keep it simple. I've also cut out most of the properties.

So I have an abstract class Content, with two derived classes, Article and Video.

    public abstract class Content
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DatePublished { get; set; }
    }
    
    public class Article : Content
    {
        public string HtmlContent { get; set; }
    }

    public class Video : Content
    {
        public string Description { get; set; }
        public string VideoUrl { get; set; }
    }

<!--excerpt-->

Then I initialize the DocumentStore and store a couple of documents.

    var documentStore =
        (new DocumentStore()
                {
                    Url = "http://localhost:8080"
                }).Initialize();
            
    using (var session = documentStore.OpenSession())
    {
        session.Store(new Video
        {
            DatePublished = DateTime.Now,
            Description = "Test Description for a Video",
            Title = "Test Title for a Video",
            VideoUrl = "http://www.youtube.com/watch?v=PGz9GokDkkg"
        });

        session.Store(new Article
        {
            DatePublished = DateTime.Now,
            Title = "Test Title for an Article",
            HtmlContent = "Some content for the article..."
        });

        session.SaveChanges();
    }

This time I'm not using the Convention to store the two documents as 'Content', rather I'm allowing it to store them as what they are. This gives me a result in Raven like:

![](/images/ravendb-inheritance-revisited-1.png)

Now if I query for Video:

    using (var session = documentStore.OpenSession())
    {
        var result = session.Query<Video>().ToList();
        
        foreach (var content in result)
        {
            Console.WriteLine(content.Id);
            Console.WriteLine(content.Title);
        }
    }

I get the output of the first Document.

![](/images/ravendb-inheritance-revisited-2.png)

Likewise if I select 'Article' I get the Article document that I previously stored.

So how do I get a list of Content?

## The Solution

So, the solution is really, really easy, it's an index.

The first thing Ayende showed me was creating the index in RavenDB Management Studio, then he showed me doing it in code. I'm just going to show it done in code.

I created a class called 'All_Content' (with an underscore) like so:

    public class All_Content : AbstractMultiMapIndexCreationTask
    {
        public All_Content()
        {
            AddMap<Article>(articles => from article in articles
                                        select new
                                        {
                                            article.Id,
                                            article.Title,
                                            article.DatePublished
                                        });
                                        
            AddMap<Video>(videos => from video in videos
                                    select new
                                    {
                                        video.Id,
                                        video.Title,
                                        video.DatePublished
                                    });
        }
    }
    
*It reminds me of writing a Union View in SQL Server in some ways.* It basically maps to the Articles and Videos, but only selects the things I need. Those of which would actually be displayed to the screen or that are common between the two document types.

Then I create the index right after I initialize the DocumentStore:

    IndexCreation.CreateIndexes(typeof(All_Content).Assembly, documentStore);

This creates the index in RavenDB for me.

![](/images/ravendb-inheritance-revisited-3.png)

As you can see, even tho I specified the class index with an underscore, it converts it to All/Content, that's a really nice way of presenting it. I think it will go well for being able to create descriptive indexes in the future.

And the index itself:

![](/images/ravendb-inheritance-revisited-4.png)

Now I need to actually query against the index. That's also really really easy. When I specify the type, I can specify the index with it:

    using (var session = documentStore.OpenSession())
    {
        var result = session.Query<Content, All_Content>().ToList();
        
        foreach (var content in result)
        {
            Console.WriteLine(content.Id);
            Console.WriteLine(content.Title);
        }
    }

Now when I run this I get the output:

![](/images/ravendb-inheritance-revisited-5.png)

Awesome!

The really interesting thing I found is that if I look at what's returned:

![](/images/ravendb-inheritance-revisited-6.png)

Are the correct CLR types that I originally defined. So I haven't lost all the additional fields by not defining them. I'm still learning but for now I assume it allows those fields to be searchable.

## Extras

One of the additional things Ayende showed me was that you can include other documents that don't inherit from the base type. You can include those in the index map, and then rather than returning a concrete type, you can specify object, or dynamic.

    var result = session.Query<dynamic, All_Content>().ToList();
    
RavenDB is really powerful. It's truly amazing, and so much nicer to work with in .NET than other document databases like MongoDB.

