---
layout: post
title: Let Keyword in ORMs
---

A while ago [MindScape](http://www.mindscapehq.com/) posted a new feature called [Ninja Properties](http://www.mindscapehq.com/blog/index.php/2010/09/14/ninja-domain-properties-in-lightspeed/) for their ORM, LightSpeed. Their post happened to be a scenario I used to solve with LINQ 2 SQL using the 'let' keyword to create a composite type to query against.

This would allow you to do something along the lines of:

    var poo = from s in ctx.Members
              let fullname = s.FirstName + " " + s.LastName
              where fullname == "Robert Williams"
              select s;
                
This nifty query would generate SQL like so:

    SELECT [t1].[Id], [t1].[FirstName], [t1].[LastName]
    FROM (
        SELECT [t0].[Id], [t0].[FirstName], 
               [t0].[LastName], 
               ([t0].[FirstName] + ' ') + [t0].[LastName] AS [value]
        FROM [Member] AS [t0]
        ) AS [t1]
    WHERE [t1].[value] = 'Robert Williams'
    
After commenting in their blog post about it they implemented it and put it into the nightly build. So after some months I finally got around to testing it, for a couple of reasons. Back when NHibernate 3.0 was in Alpha, this technique didn't work! So I thought I would test it out and see if it does, as well as checkout the SQL that's generated.

<!--excerpt-->

I'm using the following ORM's

- LightSpeed 3.11 Nightly Build (3.1 it doesn't work)
- NHibernate 3.0
- Linq 2 Sql
- Entity Framework 4

The data model is simple, but I have two scenarios I want to test. I created a Member and Task. The Member has a One-to-Many with Tasks.The test data:

    insert into member (firstname, lastname) values ('Robert', 'Williams')
    insert into member (firstname, lastname) values ('Michael', 'Jones')
    insert into member (firstname, lastname) values ('William', 'Brown')
    insert into member (firstname, lastname) values ('David', 'Davis')
    insert into member (firstname, lastname) values ('Richard', 'Miller')
    insert into member (firstname, lastname) values ('Charles', 'Wilson')
    insert into task (MemberId, Name) values(1, 'Task 1')
    insert into task (MemberId, Name) values(1, 'Task 2')
    insert into task (MemberId, Name) values(1, 'Task 3')
    insert into task (MemberId, Name) values(1, 'Task 4')
    insert into task (MemberId, Name) values(2, 'Task 1')
    insert into task (MemberId, Name) values(2, 'Task 2')
    insert into task (MemberId, Name) values(2, 'Task 3')
    insert into task (MemberId, Name) values(3, 'Task 4')


**Scenario 1:** Find all Members where fullname contains 'w', case insensitive (lowercase the fullname)

**Scenario 2:** Find all Members who have more than 2 tasks.

<span class="note">**Note:** These tests are not to say one ORM is better than another, I did this purely out of curiosity and am sharing my results.</span> 

## Scenario 1

I ran the same query against each ORM (to see the full script click here), the query is:

    var poo = from s in ctx.Members
                let fullname = s.FirstName + " " + s.LastName
                where fullname.ToLower().Contains("w")
                select s;
                
All queries are identical with the exception of the 'in' part, varying between ORMs.

LightSpeed : `from s in uow.Members`  
NHibernate 3.0 : `from s in session.Query<NHibernate.Member>()`  
Linq 2 Sql : `from s in ctx.Members`  
Entity Framework 4: `from s in mmc.Members`

The output was as follows:

    ----
    LINQ 2 SQL
    Robert
    William
    Charles
    ----
    LightSpeed
    Robert
    William
    Charles
    ----
    NHibernate 3.0
    Robert
    William
    Charles
    ----
    EntityFramework 4
    Robert
    William
    Charles
 

All ORM's worked perfectly. How about the SQL Generated from these?

### Linq 2 SQL:

    SELECT [t1].[Id], [t1].[FirstName], [t1].[LastName]
    FROM (
        SELECT [t0].[Id], [t0].[FirstName], 
               [t0].[LastName], 
               ([t0].[FirstName] + ' ') + [t0].[LastName] AS [value]
        FROM [Member] AS [t0]
        ) AS [t1]
    WHERE LOWER([t1].[value]) LIKE '%w%'

### LightSpeed:

    SELECT
      Member.Id AS [Member.Id],
      Member.FirstName AS [Member.FirstName],
      Member.LastName AS [Member.LastName]
    FROM
      Member
    WHERE
      LOWER(((Member.FirstName + ' ') + Member.LastName)) LIKE '%w%'

### NHibernate:

    select member0_.Id as Id0_, 
           member0_.FirstName as FirstName0_, 
           member0_.LastName as LastName0_ 
    from [Member] member0_ 
    where lower(member0_.FirstName+' '+member0_.LastName) like ('%'+'w'+'%')

### Entity Framework 4.0:

    SELECT 
        [Extent1].[Id] AS [Id], 
        [Extent1].[FirstName] AS [FirstName], 
        [Extent1].[LastName] AS [LastName]
    FROM [dbo].[Member] AS [Extent1]
    WHERE LOWER([Extent1].[FirstName] + N' ' + [Extent1].[LastName]) 
          LIKE N'%w%'

Note: The first 3 queries are RPC's and show in SQL Profiler with parameters, I've stripped the parameters out to show the readable query.

Great, so all 3 work fine. Tho I don't like the SQL generated by Linq 2 Sql. Hard to believe Stack Overflow runs off it.

 

## Scenario 2

The second scenario is again, the exact same query, you could write this multiple ways, however I found it cleaner to use 'let' back when I was using Linq 2 Sql. (to see the full script click here), the query is:

    var poo = from s in ctx.Members
                let tasks = s.Tasks.COUNT() as Computed
                where tasks > 2
                select s;
                
Again, the only difference is the 'in' part of the query.

The output was as follows:

    ----
    LINQ 2 SQL
    Robert
    Michael
    ----
    LightSpeed
    Exception: Specified method is not supported.
    ----
    NHibernate 3.0
    Robert
    Michael
    ----
    EntityFramework 4
    Robert
    Michael

All achieved the same results, except for LightSpeed. However I didn't think NHibernate would support this?!? Seems they have done quite a bit of work implementing LINQ since Alpha.

How about the SQL generated?

### Linq 2 Sql:

    SELECT [t2].[Id], [t2].[FirstName], [t2].[LastName]
    FROM (
        SELECT [t0].[Id], [t0].[FirstName], [t0].[LastName], (
            SELECT COUNT(*)
            FROM [Task] AS [t1]
            WHERE [t1].[MemberId] = [t0].[Id]
            ) AS [value]
        FROM [Member] AS [t0]
        ) AS [t2]
    WHERE [t2].[value] > 2

### NHibernate:

    select member0_.Id as Id0_, 
           member0_.FirstName as FirstName0_, 
           member0_.LastName as LastName0_ 
    from [Member] member0_ 
    where (select cast(count(*) as INT) 
            from [Task] tasks1_ 
            where member0_.Id=tasks1_.MemberId) > 2

### Entity Framework:

    SELECT 
    [Project1].[Id] AS [Id], 
    [Project1].[FirstName] AS [FirstName], 
    [Project1].[LastName] AS [LastName]
    FROM ( SELECT 
        [Extent1].[Id] AS [Id], 
        [Extent1].[FirstName] AS [FirstName], 
        [Extent1].[LastName] AS [LastName], 
        (SELECT 
            COUNT(1) AS [A1]
            FROM [dbo].[Task] AS [Extent2]
            WHERE [Extent1].[Id] = [Extent2].[MemberId]) AS [C1]
        FROM [dbo].[Member] AS [Extent1]
    )  AS [Project1]
    WHERE [Project1].[C1] > 2
 

I get the feeling that NHibernate generates more efficient queries than L2S/EF. Might be worth doing some performance testing against them oneday. :)

Anyway as you can see, 'let' keyword allows you to create some nice queries that are easy to ready instead of trying to complicate things in your 'where' clause, when writing LINQ.

