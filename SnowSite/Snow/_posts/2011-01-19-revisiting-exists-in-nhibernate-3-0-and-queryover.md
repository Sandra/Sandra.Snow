---
layout: post
title: Revisiting 'Exists' in NHibernate 3.0 and QueryOver
category: NHibernate
---

A while ago I wrote about [my experience with NHibernate 3.0 and Query over](http://www.philliphaydon.com/2010/09/queryover-with-nhibernate-3-lovin-it/) by having a go at converting an application to NH3.0, and faced the challenge of writing a query which contained an 'exists' where clause. At the time my solution resulted in an 'in' clause since I couldn't work out how to do an exists.

Today at work I faced the same challenge, except this time I solved it  WOO Yay for solving things.

## The Problem

We have a many-to-many relationship between Jobs and Roles. Which looks something along the lines of:

![](/images/nhibernate-queryover-1.png)

I cut a bunch of stuff out but you get the idea.

<!--excerpt-->

So what was required was on the screen was all the Jobs, you expand the Jobs and it lists all the Roles for that Job, sweet. When you add a new Role, it brings up a Drop Down List with all the Roles that are not assigned to the Job.

The existing query was along the lines of:

    SELECT   r.RoleId,
             r.Name,
             r.IsEnabled,
             r.RoleType
    FROM     Role r WITH (NOLOCK)
    WHERE    [IsEnabled] = 1
             AND r.RoleId NOT IN (SELECT RoleId
                                  FROM   JobRole j
                                  WHERE  j.JobId = 2
                                         AND j.RoleId != 1)
                                         
This worked great, except it was hard coded in XML and someone added a new column,and it brokeded stuff. Good thing is we are migrating to Fluent NHibernate.

Now I could have written this query to be identical to the original,except I decided I wanted to get that damn exists working.

## The Solution

The first thing I noticed, which I did NOT see when i originally tried this back on Alpha was 'WhereExists'

My first attempt (***which did NOT work***) was as follows:

    var existing = QueryOver.Of<JobRole>()
                            .Where(x => x.Job.JobId == jobId)
                            .And(x => x.Role.RoleId != roleId)
                            .Select(x => x.Role);
                            
    result = Session.QueryOver<Role>()
                    .Where(x => x.IsEnabled)
                    .WithSubquery.WhereNotExists(existing)
                    .OrderBy(x => x.Name).Asc
                    .List();

This produced SQL that was relatively close! But the outer query wasn't filtering the sub query.

    SELECT   this_.RoleId          as RoleId4_0_,
             this_.Name            as Name4_0_,
             this_.IsEnabled       as IsEnabled4_0_,
             this_.RoleType        as RoleType4_0_
    FROM     Role this_
    WHERE    this_.IsEnabled = 1 /* @p0 */
             and not exists (SELECT this_0_.RoleId as y0_
                             FROM   JobRole this_0_
                             WHERE  this_0_.JobId = 4 /* @p1 */
                                    and not (this_0_.RoleId = 0 /* @p2 */))
 
So I created an alias (***which did work***) like so:

    Role roleAlias = null;
    var existing = QueryOver.Of<JobRole>()
                            .Where(x => x.Job.JobId == jobId)
                            .And(x => x.Role.RoleId != roleId)
                            .And(x => x.Role.RoleId == roleAlias.RoleId)
                            .Select(x => x.Role);

    result = Session.QueryOver<Role>(() => roleAlias)
                    .Where(x => x.IsEnabled)
                    .WithSubquery.WhereNotExists(existing)
                    .OrderBy(x => x.Name).Asc
                    .List();

The alias is null to begin with, and I use it in the detached query (the 2nd And), then when I create the outer query which will be my results, I create the alias by specifying it in the QueryOver. This wires up the SQL to use the outer query to filter the sub query like so:

    SELECT   this_.RoleId          as RoleId4_0_,
             this_.Name            as Name4_0_,
             this_.IsEnabled       as IsEnabled4_0_,
             this_.RoleType        as RoleType4_0_
    FROM     Role this_
    WHERE    this_.IsEnabled = 1 /* @p0 */
             and not exists (SELECT this_0_.RoleId as y0_
                             FROM   JobRole this_0_
                             WHERE  this_0_.JobId = 4 /* @p1 */
                                    and not (this_0_.RoleId = 0 /* @p2 */)
                                    and this_0_.RoleId = this_.RoleId)
 
So it turns out it's actually really easy, I just wish there was more documentation on this stuff since books like [NHibernate 3.0 Cookbook](https://www.packtpub.com/nhibernate-3-0-cookbook/book) fail at giving any real detail on NH3.0... No offense to [Jason](http://jasondentler.com/blog) because I think the book is great, but it could have been much better if it focused on NH3.0 and using it rather than having so much non-3.0 related stuff in it.

