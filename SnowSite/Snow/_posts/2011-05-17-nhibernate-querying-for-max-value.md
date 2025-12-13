---
layout: post
title: NHibernate Querying for Max value
category: NHibernate
---

Browsing the net today for something to do with Fluent NHibernate I came across a blog post.

<http://frankmao.com/2011/01/14/nhibernate-subquery/>

The blog post is to do with Subquery, but I got a little bit confused since the post itself doesn't have anything to do with Subqueries.

About the actual post tho, NHibernate.Linq does actually support Min/Max operators.

Infact I just wrote a quick test to see the SQL it generated, the following code:

    var result = session.Linq<TestProduct>().Max(x => x.Value);
 
Generates the following SQL.

    SELECT max(this_.Value) as y0_
    FROM   [TestProduct] this_
 
NHibernate.Linq has basically been deprecated however since NH3.0 has it's own built in Linq provider, rewriting that query in NH3.0 would look like:

    var result = session.Query<BaseClass>().Max(x => x.Id);

