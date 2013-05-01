---
layout: post
title: Running 2 queries is fine, you don't need to ALWAYS eager load!
category: Rant
---

Every now-n-then someone comes into JabbR or messages me asking about the best way to load a relationship. Often enough what the person is trying to do is Eager load some reference/relational data.

For example, Order / Customer.

If we want to load an Order and get the Customer information at the same time, we can eagerly fetch this information using an ORM, in RavenDB we can Include the results so that when we query them, the session already has the information and doesn't need to round-trip to the database.

### You don't need to! ###

There really is nothing wrong with executing two separate queries in this scenario.

    var order = Session.Load<Order>(123); 
    var customer = Session.Load<Customer>(order.CustomerId);

These queries are fast to execute, and there really is nothing wrong with it! You can add some complexity in your NHibernate mappings by creating a reference so you can Eagerly fetch the reference and have 1 trip to the database etc. But this really isn't where eager fetching is beneficial!

<!--excerpt-->

### Your database is located off-site ###

A good reason to want to eager fetch to avoid hitting the database again, is because your database server isn't sitting next to your web server.

Most website's are so small you have the database and website on the same server, these are small little hobby sites.

Then you grow and you have:

![](/images/running-two-queries-1.png)

These are sitting next to each other. Latency is low, you've probably got gigabit connection between the two, and sending 2 queries is very fast.

However if you're using something like RavenHQ where the database isn't right next to each other:

![](/images/running-two-queries-2.png)

Now we have to deal with hops and latency and all sorts of issues, being able to issue 1 query and get 2 results back is now beneficial!

### The problem with not eager fetching ###

If you're not careful, then you can end up in scenarios where you have SelectN+1. For example if I was listing ALL orders, and fetching the Customer for each Order I displayed to the screen.

This could end up issuing LOTS of queries.

Being able to say:

    var result = Session.Query<Order>().Fetch(x => x.Customer).Eager;

Means when we can issue 1 query, and avoid ending up in a SelectN+1 scenario that might severely hurt performance.

RavenDB allows you to include results, but it does one thing most (if not all) ORMs don't allow you to do. `Load<T>` has an overload which takes an array of Ids.

This allows you to do something like:

    var orders = Session.Query<Order>().ToList(); 
    var customers = Session.Load<Customer>(orders.Select(x => x.CustomerId).ToArray());

Now you can compose your results together on the screen.

### Note: ###

I'm not against eager loading, all I'm saying is you don't ALWAYS have to eager load, there are times when it's far more beneficial to eager load than others.
