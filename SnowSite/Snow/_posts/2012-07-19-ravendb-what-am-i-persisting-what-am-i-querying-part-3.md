---
layout: post
title: RavenDB... What am I persisting, what am I querying? (part 3)
category: RavenDB
---

[Part 1](/2012/07/ravendb-what-am-i-persisting-what-am-i-querying/)   
[Part 2](/2012/07/ravendb-what-am-i-persisting-what-am-i-querying-part-2/)    

In part 3 I want to show you how to query References.

In the previous post I showed you three basic classes that demonstrate a Relationship between Order and OrderLine, and a Reference to User from Order using the UserId.

I've setup some really basic test data:

    using (var session = store.OpenSession())
    {
        session.Store(new User
        {
            FirstName = "Phillip",
            Surname = "Haydon",
            Username = "phillip.haydon",
            Password = "somepassword"
        });
        session.Store(new User
        {
            FirstName = "Edward",
            Surname = "Norton",
            Username = "edward.norton",
            Password = "somepassword"
        });
        session.Store(new Order
        {
            UserId = "users/1",
            DateOrdered = DateTime.Now,
            DateUpdated = DateTime.Now,
            Status = "Ordered",
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Discount = 0m,
                    PricePerUnit = 13.95m,
                    Quantity = 5,
                    SkuCode = "SN78"
                },
                new OrderLine
                {
                    Discount = 0m,
                    PricePerUnit = 13.95m,
                    Quantity = 5,
                    SkuCode = "SN78"
                }
            }
        });
        session.SaveChanges();
    }

This creates two collections:

<!--excerpt-->

![](/images/ravendb-what-am-i-persisting-part-3-1.png)

With our Order document looking like:

    {
      "UserId": "users/1",
      "DateOrdered": "2012-07-13T23:34:40.5542680",
      "DateUpdated": "2012-07-13T23:34:40.5542680",
      "Status": "Ordered",
      "Lines": [
        {
          "PricePerUnit": 13.95,
          "Quantity": 5,
          "Discount": 0.0,
          "SkuCode": "SN78"
        },
        {
          "PricePerUnit": 13.95,
          "Quantity": 5,
          "Discount": 0.0,
          "SkuCode": "SN78"
        }
      ]
    }

What's cool about RavenDB Studio is that it shows us that the UserId is a reference.

![](/images/ravendb-what-am-i-persisting-part-3-2.png)

RavenDB links the reference up for you as well so you can click it and it will navigate you directly to the document that is being referenced.

There are three ways that we can load this data in code.

- Roundtrip to the store
- Include
- Transform

## Roundtrip to the store ##

This method is easy peasy, and it's pretty similar to something you would do when working with a relational database.

We do this by calling Load on the Order, then using the value from `Order.UserId` to load the User.

    using (var session = store.OpenSession())
    {
        var order = session.Load<Order>("orders/1");
        var user = session.Load<User>(order.UserId);
        Console.WriteLine("Lines: " + order.Lines.Count());
        Console.WriteLine("FirstName: " + user.FirstName);
    }

When we run this we get an output like so:

![](/images/ravendb-what-am-i-persisting-part-3-3.png)

The problem with this approach is that we have to go to RavenDB twice, shown here:

![](/images/ravendb-what-am-i-persisting-part-3-4.png)

But it achieves the desired result.

## Include ##

The include method is very similar, the only difference is we tell RavenDB to include the User when we fetch the Order.

This can be done like so:

    using (var session = store.OpenSession())
    {
        var order = session.Include<Order>(x => x.UserId)
                           .Load<Order>("orders/1");
        var user = session.Load<User>(order.UserId);
        Console.WriteLine("Lines: " + order.Lines.Count());
        Console.WriteLine("FirstName: " + user.FirstName);
    }

As you can see all we have done is add the Include method to our initial query.

When we run this we get the exact same output, the difference this time is that RavenDB will issue 1 query for data.

![](/images/ravendb-what-am-i-persisting-part-3-5.png)

So the first query for Order, includes the User. This User object is now part of the current RavenDB Session, so now when we load the User on the next line it doesn't need to go to RavenDB to fetch it, it already has it.

## Transform ##

This last method is quite different to the last two, it involves writing an index and implementing TransformResults.

First we start by defining an index, which basically just grabs the UserId in the map, then looks up the user in the transform.

    public class Order_WithUser : AbstractIndexCreationTask<Order>
    {
        public Order_WithUser()
        {
            Map = o => from s in o
                       select new
                       {
                           s.UserId
                       };
            TransformResults = (database, results) =>
                from s in results
                let user = database.Load<User>(s.UserId)
                select new
                {
                    s.Id,
                    s.UserId,
                    s.DateOrdered,
                    s.DateUpdated,
                    s.Status,
                    s.Lines,
                    User = user
                };
        }
    }

Then we can query it and return it as a new model that includes the User *(this could also be added to the Order and not persisted but I've made it a separate model for demonstration)*

    public class OrderResult
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime DateOrdered { get; set; }
        public DateTime DateUpdated { get; set; }
        public string Status { get; set; }
        public IEnumerable<OrderLine> Lines { get; set; }
        public User User { get; set; }
    }
 
    using (var session = store.OpenSession())
    {
        var order = session.Query<Order, Order_WithUser>()
                           .Where(x => x.Id == "orders/1")
                           .AsProjection<OrderResult>()
                           .SingleOrDefault();
        Console.WriteLine("Lines: " + order.Lines.Count());
        Console.WriteLine("FirstName: " + order.User.FirstName);
    }

When we run this again we get the same results, however in RavenDB we have had to query against an index, rather than just grabbing the document as is.

![](/images/ravendb-what-am-i-persisting-part-3-6.png)

That concludes part 3. Any questions, leave a comment or join the [JabbR RavenDB](http://jabbr.net/#/rooms/RavenDB) chat room.
