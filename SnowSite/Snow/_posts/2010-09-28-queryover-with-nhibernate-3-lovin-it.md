---
layout: post
title: QueryOver with NHibernate 3... Lovin it!
category: NHibernate
---

Working on an old website I built back in 2004, back then I coded all the data access by hand, wrote stored products and populated the data into objects, lazy loaded relationships etc. 2005 roles around and I migrated it to .Net 2.0, but with all the old data access stuff intact.

Move forward 6 years and I have to make some changes, so I figured I would update the data access to use NH3.0 so I could play around with it.

One of the thing's I just wrote was a sub query using exists. If I wrote the SQL it would probably look something along the lines of:

    SELECT * FROM ProductType
    WHERE EXISTS (SELECT 1 FROM Product WHERE Active = 1 AND ProductTypeID = ProductType.ProductTypeID)
    
With some more criteria, since the data comes from MYOB it's sometimes out of date, I check for status, active, quantity,price,etc.

Originally this stuff was in a stored procedure, and I needed to write this using QueryOver.

<!--excerpt-->

I broken the query into two pieces, the query against the Product with criteria, and the query against the ProductType, using the Product query as  a Subquery for the ProductType criteria.

    var products = QueryOver.Of<Product>()
        .Where(x => x.RetailPrice > 0)
        .And(x => x.Quantity > 0)
        .And(x => x.Active)
        .And(x => x.LastUpdate > DateTime.Now.AddDays(-3))
        .WhereRestrictionOn(x => x.StatusId).IsIn(new int[] {1, 2, 9})
        .Select(x => x.ProductType.Id);
        
    var result = Session.QueryOver<ProductType>()
        .Where(x => x.Active)
        .WithSubquery.WhereProperty(x => x.Id).In(products)
        .List();
        
    return result;

Running profiler aga... SQL Profiler (not NHProf, tho that would be handy if i could afford it) produces the following:

*image missing - old post* 

    SELECT this_.ProductTypeID as ProductT1_3_0_,
           this_.Name as Name3_0_,
           this_.Active as Active3_0_
    FROM [ProductType] this_
    WHERE this_.Active = @p0
        and this_.ProductTypeID in (
            SELECT this_0_.ProductTypeId as y0_
            FROM [Product] this_0_
            WHERE this_0_.RetailPrice > @p1
                and this_0_.Quantity > @p2
                and this_0_.Active = @p3
                and this_0_.LastUpdate > @p4
                and this_0_.StatusId in (@p5, @p6, @p7))
                
I removed the parameters since you don't need those :) but it achieves the same thing my old stored proc query did. And it's much easier to maintain too! No more hunting around in stored procs to update queries when columns are dropped or added.

So far I'm loving the QueryOver syntax.













































