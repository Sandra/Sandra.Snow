---
layout: post
title: Not.LazyLoad - Eager Loading with NHibernate 3.0
category: NHibernate
---

A friend asked me about fetching relationships with NHibernate when something couldn't be lazy-loaded.

The reason it couldn't be lazy-loaded was because he uses Session-Per-Call, my preference is Session-Per-Request. (this is web based, I've never actually used NHibernate in a non-web scenario, yet)

The solution he's currently using is to turn off lazy-loading at the mapping. This is all kinds of bad.

I think it's best to demonstrate why turning off lazy-loading at the mapping is bad. I've come up with a, well what I consider pretty standard Blog.

![](/images/nhibernate-lazy-load-1.png)

Nothing special but needed something that has a few relationship"s. (I think you can click that image to make it bigger)

<!--excerpt-->

So as we can see, a Post has 4 relationships, and a Comment has a relationship back to the Post and User.

I've mapped it and created some test data:

[View Full Maps](/stuffz/NH-Blog-Mappings.cs.txt)

[View SQL Script](/stuffz/NH-Blog-Scripts.sql.txt)

Right, so the maps are set to Not LazyLoad the relationships.

### Post

    public class PostMap : ClassMap<Post>
    {
        public PostMap()
        {
            Id(x => x.Id);
            Map(x => x.Title);
            Map(x => x.Content);
            Map(x => x.PublishedAt);
            References(x => x.Author)
                .Column("UserId")
                .Not.LazyLoad();

            HasManyToMany(x => x.Tags)
                .Table("PostTags")
                .ParentKeyColumn("PostId")
                .ChildKeyColumn("TagId")
                .Not.LazyLoad();

            HasManyToMany(x => x.Categories)
                .Table("PostCategories")
                .ParentKeyColumn("PostId")
                .ChildKeyColumn("CategoryId")
                .Not.LazyLoad();

            HasMany(x => x.Comments)
                .KeyColumn("PostId")
                .Inverse()
                .Not.LazyLoad();
        }
    }


### Comment

    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Id(x => x.Id);
            Map(x => x.Content);
            References(x => x.Commenter)
                .Column("UserId")
                .Not.LazyLoad();

            References(x => x.Post)
                .Column("PostId")
                .Not.LazyLoad();
        }
    }

I haven't mapped any relationships the other way, keeping it simple, stupid. 

Right, so lets assume we wanted to load Post 1, and display the Title and the Categories the post was in. We didn't care about the User, or the Comments, or anything like that.

    var sessionFactory = new SessionFactoryManager().CreateSessionFactory();
    using (var session = sessionFactory.OpenSession())
    {
        var post = session.Query<Post>()
                            .SingleOrDefault(x => x.Id == 1);

        Console.WriteLine("Post: " + post.Title);
        Console.WriteLine("Category: ");

        post.Categories.ForEach(x => Console.WriteLine(x.Name));
    }

If we were writing the query ourselves, all we would want is the Post, and the Categories, well because we mapped all the references and collections as Not.LazyLoad, NHibernate is kind enough to go and grab that information for us. The SQL Generated results in:

![](/images/nhibernate-lazy-load-2.png)

(Image is screen shot from [NHProf](http://www.nhprof.com/))

AHHHHH Bad bad bad!!! It's loaded all that information we don't care about!

Right, lets set everything back to normal, and leave Lazy Loading on! Run the same query, and see what happens.

### Post

    public class PostMap : ClassMap<Post>
    {
        public PostMap()
        {
            Id(x => x.Id);
            Map(x => x.Title);
            Map(x => x.Content);
            Map(x => x.PublishedAt);
            References(x => x.Author)
                .Column("UserId");

            HasManyToMany(x => x.Tags)
                .Table("PostTags")
                .ParentKeyColumn("PostId")
                .ChildKeyColumn("TagId");

            HasManyToMany(x => x.Categories)
                .Table("PostCategories")
                .ParentKeyColumn("PostId")
                .ChildKeyColumn("CategoryId");

            HasMany(x => x.Comments)
                .KeyColumn("PostId")
                .Inverse();
        }
    }


### Comment

    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Id(x => x.Id);
            Map(x => x.Content);
            References(x => x.Commenter)
                .Column("UserId");

            References(x => x.Post)
                .Column("PostId");
        }
    }

Lets see what happens now.

![](/images/nhibernate-lazy-load-3.png)

Great! Much nicer, we haven't got all that information we didn't want. The only problem is, what about this scenario?

    var sessionFactory = new SessionFactoryManager().CreateSessionFactory();
    Post post;

    using (var session = sessionFactory.OpenSession())
    {
        post = session.Query<Post>()
                      .SingleOrDefault(x => x.Id == 1);

        Console.WriteLine("Post: " + post.Title);
    }

    Console.WriteLine("Category: ");
    post.Categories.ForEach(x => Console.WriteLine(x.Name));

Lets assume that the 'using' block was a call to a repository, to get a particular post, and we needed to iterate over the categories a little bit later?

![](/images/nhibernate-lazy-load-4.png)

Oh no, not what we wanted  because the object is now disconnected from the session, it can't lazy load the categories. This I suspect is the issue my friend got.

So the solution? To Eager Load!

    Post post;
    using (var session = sessionFactory.OpenSession())
    {
        post = session.Query<Post>()
                        .Where(x => x.Id == 1)
                        .Fetch(x => x.Categories)
                        .SingleOrDefault();

        Console.WriteLine("Post: " + post.Title);
    }

    Console.WriteLine("Category: ");
    post.Categories.ForEach(x => Console.WriteLine(x.Name));

This results in a single query being issued:

![](/images/nhibernate-lazy-load-5.png)

What's this query look like?

![](/images/nhibernate-lazy-load-6.png)

Not bad! Definitely saves a second trip to the database to grab the categories. But there's a problem, there types of queries result in cartesian product result-set. (Think that's what it's called)

If we issue that query our-self we end up with this:

![](/images/nhibernate-lazy-load-7.png)

We actually get Post back twice, because it's got two categories. It's only because we told NHibernate we wanted a single result that it knew the Root aggregate is distinct.

So what happens if we wanted more than 1 post, and eager load the relationships?

    var sessionFactory = new SessionFactoryManager().CreateSessionFactory();
    IList<Post> posts;

    using (var session = sessionFactory.OpenSession())
    {
        posts = session.QueryOver<Post>()
                        .Fetch(x => x.Categories).Eager
                        .List();
    }

    foreach (var post in posts)
    {
        Console.WriteLine("Post: " + post.Title);

        Console.WriteLine("Categories:");
                    
        foreach (var category in post.Categories)
        {
            Console.WriteLine(" - " + category.Name);
        }

        Console.WriteLine("--");
    }

Nothing special, right? Except... because we get duplicate posts in the last query, imagine what we get when we actually iterate over this result.

    Post: Post 1
    Categories:
    - Category 1
    - Category 3
    --
    Post: Post 1
    Categories:
    - Category 1
    - Category 3
    --
    Post: Post 2
    Categories:
    - Category 2
    --
    Post: Post 3
    Categories:
    - Category 1
    - Category 2
    - Category 3
    --
    Post: Post 3
    Categories:
    - Category 1
    - Category 2
    - Category 3
    --
    Post: Post 3
    Categories:
    - Category 1
    - Category 2
    - Category 3
    --

Scary right? We can fix this using NH Transformations.

    using (var session = sessionFactory.OpenSession())
    {
        posts = session.QueryOver<Post>()
                        .Fetch(x => x.Categories).Eager
                        .TransformUsing(new DistinctRootEntityResultTransformer())
                        .List();
    }
    
This formats the result to be distinct posts with the related Categories like so:

    Post: Post 1
    Categories:
    - Category 1
    - Category 3
    --
    Post: Post 2
    Categories:
    - Category 2
    --
    Post: Post 3
    Categories:
    - Category 1
    - Category 2
    - Category 3
    --
    
Perfect!

As you can imagine, eager loading is a pretty powerful feature, but we still have to be careful to not load too many relationships. Because eager loading doesn't break the relationships into different result-set queries, we can end up with inefficient queries because there are too many, or possibly too complex joins.

We could over-come some of these BY we, Not.LazyLoadby creating the object graph ourselves by using NHibernates `Future<T>` feature, to batch queries together to reduce the number of database trips. That's a post for another day.
