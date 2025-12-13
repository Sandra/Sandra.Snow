---
layout: post
title: NHibernate Work-Around is not really a Work-Around...
category: NHibernate
---

Over the weekend I came across a blog post about NHibernate, and an apparent work-around for a feature not supported by NHibernate.

The original post can be found here: [nHibernate LINQ workaround for System.NotSupportedException](http://www.arrangeactassert.com/nhibernate-linq-workaround-for-system-notsupportedexception/)

Basically the author wanted to write something along the lines of:

    var fruitIds = new List<int> { 5, 8, 13 };
    
    using (var session = factory.OpenSession())
    {
        var result = from f in session.Query<NHFruit>()
                     join i in fruitIds
                         on f.Id equals i
                     select f;

        foreach (var fruit in result)
            Console.WriteLine(fruit.Name);
    }

Where the query joins to a list of Ids to filter the results out. This however, happens to throw an exception:

<!--excerpt-->

![](/images/nhibernate-workaround-1.png)

Normally this sort of query would be written like so:

    var fruitIds = new List<int> { 5, 8, 13 };
    
    using (var session = factory.OpenSession())
    {
        var result = from f in session.Query<NHFruit>()
                     where fruitIds.Contains(f.Id)
                     select f;

        foreach (var fruit in result)
            Console.WriteLine(fruit.Name);
    }

This generates an 'in' clause in the SQL.

    select nhfruit0_.Id   as Id0_,
           nhfruit0_.Name as Name0_
    from   Fruit nhfruit0_
    where  nhfruit0_.Id in (5 /* @p0 */,8 /* @p1 */,13 /* @p2 */)
    Nice and clean, pretty standard SQL.

Now I must admit when I first read his code I made the assumption that it would actually pull ALL results into memory, and do the join in memory.

I commented on his blog with the following comment:

> I don"t see how this is a "work-around" your first attempt at the query is attempting to join something in memory, to something still in the database. Thats clearly not, and will never be a supported feature of NHibernate... The second query is passing a list of ID"s into the database to use for comparison.

At the time of writing the comment, I was a bit annoyed. The author replied:

> Never be supported in NHibernate... well it works using Entity Framework and that's why I consider it to be a workaround.

So lets give it a go.

Give the following query:

    var fruitIds = new List<int> { 5, 8, 13 };
    
    using (var ctx = new EFTestEntities())
    {
        var result = from f in ctx.Fruits
                     join i in fruitIds
                         on f.Id equals i
                     select f;

        foreach (var fruit in result)
            Console.WriteLine(fruit.Name);
    }

The exact same as the attempt in NHibernate, it generates the following SQL...

    SELECT [Extent1].[Id]   AS [Id],
           [Extent1].[Name] AS [Name]
    FROM   [dbo].[Fruit] AS [Extent1]
           INNER JOIN (SELECT [UnionAll1].[C1] AS [C1]
                       FROM   (SELECT 5 AS [C1]
                               FROM   (SELECT 1 AS X) AS [SingleRowTable1]
                               UNION ALL
                               SELECT 8 AS [C1]
                               FROM   (SELECT 1 AS X) AS [SingleRowTable2]) AS [UnionAll1]
                       UNION ALL

                       SELECT 13 AS [C1]
                       FROM   (SELECT 1 AS X) AS [SingleRowTable3]) AS [UnionAll2]
             ON [Extent1].[Id] = [UnionAll2].[C1]

WTF.

![](/images/nhibernate-workaround-2.png)

It creates a select statement, for each value that you want to select... I thought "oh ok, i wonder what the execution plan looks like for that, compared to a normal in-clause from NHibernate.

Using both the queries already shown above, showing the actual execution plans, I get the following:

![](/images/nhibernate-workaround-3.png)

Wow... But it gets better, I have 20 records in my database, and I thought, what happens if my list of Ids contains 10 different ids:

Same queries, but with:

    var fruitIds = new List<int> { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

Entity Framework (I don't even want to paste this in here...):

    SELECT [Extent1].[Id]   AS [Id],
           [Extent1].[Name] AS [Name]
    FROM   [dbo].[Fruit] AS [Extent1]
           INNER JOIN (SELECT [UnionAll8].[C1] AS [C1]
                       FROM   (SELECT [UnionAll7].[C1] AS [C1]
                               FROM   (SELECT [UnionAll6].[C1] AS [C1]
                                       FROM   (SELECT [UnionAll5].[C1] AS [C1]
                                               FROM   (SELECT [UnionAll4].[C1] AS [C1]
                                                       FROM   (SELECT [UnionAll3].[C1] AS [C1]
                                                               FROM   (SELECT [UnionAll2].[C1] AS [C1]
                                                                       FROM   (SELECT [UnionAll1].[C1] AS [C1]
                                                                               FROM   (SELECT 2 AS [C1]
                                                                                       FROM   (SELECT 1 AS X) AS [SingleRowTable1]
                                                                                       UNION ALL
                                                                                       SELECT 4 AS [C1]
                                                                                       FROM   (SELECT 1 AS X) AS [SingleRowTable2]) AS [UnionAll1]
                                                                               UNION ALL

                                                                               SELECT 6 AS [C1]
                                                                               FROM   (SELECT 1 AS X) AS [SingleRowTable3]) AS [UnionAll2]
                                                                       UNION ALL

                                                                       SELECT 8 AS [C1]
                                                                       FROM   (SELECT 1 AS X) AS [SingleRowTable4]) AS [UnionAll3]
                                                               UNION ALL

                                                               SELECT 10 AS [C1]
                                                               FROM   (SELECT 1 AS X) AS [SingleRowTable5]) AS [UnionAll4]
                                                       UNION ALL

                                                       SELECT 12 AS [C1]
                                                       FROM   (SELECT 1 AS X) AS [SingleRowTable6]) AS [UnionAll5]
                                               UNION ALL

                                               SELECT 14 AS [C1]
                                               FROM   (SELECT 1 AS X) AS [SingleRowTable7]) AS [UnionAll6]
                                       UNION ALL

                                       SELECT 16 AS [C1]
                                       FROM   (SELECT 1 AS X) AS [SingleRowTable8]) AS [UnionAll7]
                               UNION ALL

                               SELECT 18 AS [C1]
                               FROM   (SELECT 1 AS X) AS [SingleRowTable9]) AS [UnionAll8]
                       UNION ALL

                       SELECT 20 AS [C1]
                       FROM   (SELECT 1 AS X) AS [SingleRowTable10]) AS [UnionAll9]
             ON [Extent1].[Id] = [UnionAll9].[C1]

NHibernate:

    select nhfruit0_.Id   as Id0_,
           nhfruit0_.Name as Name0_
    from   Fruit nhfruit0_
    where  nhfruit0_.Id in (2 /* @p0 */,4 /* @p1 */,6 /* @p2 */,8 /* @p3 */,
                            10 /* @p4 */,12 /* @p5 */,14 /* @p6 */,16 /* @p7 */,
                            18 /* @p8 */,20 /* @p9 */)
                        
Now if you run with the execution plans, (I pressed execute 10 times before taking this screenshot)

![](/images/nhibernate-workaround-4.png)

The more parameters you add, the slower it gets.

You can't blame Entity Framework though. EF supports the same query as NHibernate!

    var fruitIds = new List<int> { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
    
    using (var ctx = new EFTestEntities())
    {
        var result = from f in ctx.Fruits
                     where fruitIds.Contains(f.Id)
                     select f;

        foreach (var fruit in result)
            Console.WriteLine(fruit.Name);
    }

Entity Framework generates the following query:

    SELECT [Extent1].[Id]   AS [Id],
           [Extent1].[Name] AS [Name]
    FROM   [dbo].[Fruit] AS [Extent1]
    WHERE  [Extent1].[Id] IN (2,4,6,8,
                              10,12,14,16,
                              18,20)
                          
So I personally think the original approach, to do a join on an in-memory list, is bad practice, regardless of weather it works or not. It's bad.

It's also bad to blindly write LINQ queries without ever looking at the SQL they produce. Sure it's great to not have to worry about writing SQL, but you should still always be aware of the SQL that is been run, sometimes slightly changing the LINQ can massively alter the SQL that is produced.

