---
layout: post
title: NHibernate - Querying relationships at depth!
category: NHibernate
---

The title may be a bit leading, this isn't about querying one-to-many-to-many, that is simply not possible. Well it may be possible, but it's not practical by any means.

It's not practical because when querying for one-to-many you end up with a cartesian product. With a one-to-many it's easy to transform the data knowing the root is a single result while the relationship is a collection of all the results.

If you have a many-to-many it works too since it's the distinct root and the relating collection of all results for a single root item.

But to select a one-to-many-to-many it's far too complicated to work out what the hell is going on.  Well in my opinion it's far too complicated.

<!--excerpt-->

However if you have a one-to-many-to-one, so three levels deep... Lets say... A blog, with many posts, each with an author.

![](/images/nhibernate-depth-1.png)

We can query the 3rd level with the posts relatively easily.

If we take a relatively simple query, to fetch a blog with posts and display the title/author like so:

    var sessionFactory = new SessionFactoryManager().CreateSessionFactory();
    
    Blog blog = null;
    
    using (var session = sessionFactory.OpenSession())
    {
        blog = session.QueryOver<Blog>()
                      .Where(x => x.Id == 1)
                      .Fetch(x => x.Posts).Eager
                      .SingleOrDefault();

        Console.WriteLine(blog.Name);
        Console.WriteLine("--");

        foreach (var post in blog.Posts)
        {
            Console.WriteLine("Title: " + post.Title);
            Console.WriteLine("Author: " + post.Author.FirstName);
            Console.WriteLine("--");
        }
    }

We can eager load the posts...

![](/images/nhibernate-depth-2.png)

No problems. Except when we grab the Author, we end up with a select+n scenario...

![](/images/nhibernate-depth-3.png)

This is bad bad bad! So the idea would be to eager load the Author with the post, the problem is .Fetch() doesn't allow you to specify a property of a collection...

That's where 'JoinAlias' comes in handy, I assume this stuff comes from HQL/Criteria, but I prefer not to use that stuff.

Instead of using 'Fetch' we are going to replace it with a 'JoinAlias' like so:

    Post posts = null;
    Author author = null;
    blog = session.QueryOver<Blog>()
                  .Where(x => x.Id == 1)
                  .JoinAlias(x => x.Posts, () => posts)
                  .JoinAlias(() => posts.Author, () => author)
                  .SingleOrDefault();

Opps...

![](/images/nhibernate-depth-4.png)

It's executed two queries, the first query looks fine...

![](/images/nhibernate-depth-5.png)

The data returned is correct too, except NH doesn't seem to think so, it actually queries for the Posts a second time.

However, updating the query to specify a join type:

    Post posts = null;
    Author author = null;
    blog = session.QueryOver<Blog>()
                  .Where(x => x.Id == 1)
                  .JoinAlias(x => x.Posts, () => posts, JoinType.LeftOuterJoin)
                  .JoinAlias(() => posts.Author, () => author)
                  .SingleOrDefault();

We end up with a single query, with all the correct results.

![](/images/nhibernate-depth-6.png)

![](/images/nhibernate-depth-7.png)

