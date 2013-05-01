---
layout: post
title: RavenDB... What am I persisting, what am I querying? (part 1)
category: RavenDB
---

[Part 1](/2012/07/ravendb-what-am-i-persisting-what-am-i-querying/)  
[Part 2](/2012/07/ravendb-what-am-i-persisting-what-am-i-querying-part-2/)  
[Part 3](/2012/07/ravendb-what-am-i-persisting-what-am-i-querying-part-3/)  

A couple of questions that pop's up a lot in the [#RavenDB](https://jabbr.net/#/rooms/RavenDB) [JabbR](https://jabbr.net/) chat room by people picking up RavenDB for the first time are; *what am I persisting?, and how do I query relationships?.*

When we use relational databases we often de-normalize our data into multiple tables, usually this is done to get rid of duplication of data. We do this by adding 100's of foreign keys to our tables relating things all over the place, we had a CountryId to our Address, a UserId to our Order, an OrderId to our OrderLine.

There's many reasons why this was done, some of which Oren describes in his [Embracing RavenDB post.](http://ayende.com/blog/153026/embracing-ravendb)

Then when we go to query those relationships we have to join data, when we have an entity with multiple relationships we end up getting into complex queries with cartesian joins, performance starts to degrade, and things just get messy.

When working with Document Databases we throw all that out the window and we deal with Root Aggregates. These are objects that are responsible for their child objects, you don't load the child objects individually, they are loaded with the root or parent object.

<!--excerpt-->

The most common example I see is Blog/Posts/Comments, but I'm going to explain an easier scenario.

## Order/OrderLine ##

The Order/Orderline is much easier to understand since it's a scenario would probably always end up being the same in every system.

It also easier to understand because when displaying an OrderLine in any system, it's always displayed with the Order details, and never by itself. So when we query for the Order it makes sense to always get the OrderLine at the same time.

When working with Business Rules applied to an Order, they almost always apply to the OrderLines also, so again you're working with the entire Order, not a portion of it.

When starting out it's hard to imagine, but the OrderLine is actually part of the Order, it's not a separate entity, it's just when we persist it in two tables since that makes more sense in a relational database, and it ends up feeling like two separate things, when in reality, it's still the same object.

    public class Order
    {
        public string Id { get; set; }
        // Other properties...

        public IEnumerable<OrderLine> Lines { get; set; }
    }

    public class OrderLine
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string SkuCode { get; set; }

        // Other Properties
    }

So when we persist this with a Relational Database these would go into two different tables. Order and OrderLine tables, joined by a foreign key.

But now that we are thinking about the Root Aggregate, the Order, when we persist this with RavenDB we persist just the Order. When we persist 'just the order' that means we persist the ENTIRE Order object, *including the OrderLines, since they are the Order*.

When persisted to RavenDB we end up with a JSON document that looks similar to:

    {
        Id: 'orders/123',
        Lines: [
            { Quantity: 1, Price: 12.95, Discount: null, SkuCode: 'N1C3' },
            { Quantity: 3, Price: 6.23, Discount: null, SkuCode: 'F4K21' }
        ]
    }

<span class="note">**Note:** I purposely left out other properties for now.</span>

As you can see we are persisting the entire root object itself. We don't put OrderLines into a separate document or collection.

<span class="note">**Note:** I do realise I've mentioned persisting the entire object multiple times, but it's something that some people find hard to wrap their head around at first. It confused me when I first started messing around with MongoDB.</span>

When we query for the Order: `session.Load<Order>("orders/123");` we end up fetching all the OrderLines at the same time. No joins, no separate queries, just the entire order.

In a relational database we would have had to issue 2 separate queries, or join the tables together, like:

    SELECT * FROM [Order] o
    LEFT JOIN [OrderLine] ol
        ON o.Id = Ol.OrderId

This makes querying the database more complicated than it needs to be. There are other ways around this in a relational database, you can [blob the OrderLines](/2012/03/ormlite-blobbing-done-with-nhibernate-and-serialized-json/). But then you lose the ability to search against OrderLines.

## Why this example and not Post/Comments? ##

I don't think Post/Comments is a good example to work from, Comment's can be displayed with a Post, and without a Post, they can be paged, displayed on an individual page, in a 'latest comments' column on your blog, etc.

Some of these scenarios may justify putting Comments into their own collection.

However, more often than not, non-popular blogs such as my own only occur a few comments, so there's no real reason to put them in their own collection, you can easily get away with putting them on the Post document.

I think this comes down to personal preference and the business problem you're trying to solve, but for a learning exercise it makes it harder to understand. My personal preference is to store Comment's in a separate collection, because you click through from the post listing screen to the post and load the comment's, and if there are > x number of comments then I would page them and only display the latest comments, or high rated comments if they were rated voted up/down.

I hope that clear's up what's being persisted.

**In part 2 I'm going to go over References *(Relationships)*, and in part 3 MapReduce *(doing all those fancy SQL queries inside RavenDB and what is happening)***
