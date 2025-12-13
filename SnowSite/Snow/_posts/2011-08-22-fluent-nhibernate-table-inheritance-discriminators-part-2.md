---
layout: post
title: Fluent NHibernate - Table Inheritance - Discriminators (Part 2)
category: NHibernate
---

This is part two, to my post about [Table Inheritance using Discriminators](/2011/08/fluent-nhibernate-table-inheritance-discriminators/), in this post I just want to demonstrate the outcome when the sub-classes have their own properties, or possibly a property that maps to the same column.

First thing however is Mark Perry pointed out in the comments that specifying a value for the baseClassDiscriminator will force it to store the value in the database as an INT rather than a a VARCHAR.

    DiscriminateSubClassesOnColumn("PostType", 0);
    
This will create the table with an INT like so:

    create table WallPost (
      Id         UNIQUEIDENTIFIER   not null,
      PostType   INT   not null,
      DatePosted DATETIME   null,
      Title      NVARCHAR(255)   null,
      Content    NVARCHAR(255)   null,
        primary key ( Id ))
        
Maybe INT is too big however, maybe we only want a SMALLINT? That will give us 32k sub-classes...

    DiscriminateSubClassesOnColumn("PostType", (short)0);
    
<!--excerpt-->
<!-- -->

    PostType   SMALLINT   not null,
    
But even that is too many, so maybe we need TINYINT, that gives us 0-255. I doubt you would ever have 255 sub-classes, so we can specify the discriminator as a byte.

    DiscriminateSubClassesOnColumn("PostType", (byte)0);

And that gives us:

    create table WallPost (
      Id         UNIQUEIDENTIFIER   not null,
      PostType   TINYINT   not null,
      DatePosted DATETIME   null,
      Title      NVARCHAR(255)   null,
      Content    NVARCHAR(255)   null,
        primary key ( Id ))

Nice, much better.

Not to mention when querying now, it uses the INT value rather than the number being used as a string like before:

    SELECT this_.Id         as Id0_0_,
           this_.DatePosted as DatePosted0_0_,
           this_.Title      as Title0_0_,
           this_.Content    as Content0_0_
    FROM   WallPost this_
    WHERE  this_.PostType = 1
    
So the next thing I want to show is what happens when we add a property to 1 sub-class and not the other. (or properties than exist in 1, and properties that exist in the other)

Give the same example as my previous post, I've added one property to the class, a 'Url' property.

![](/images/fnh-table-inheritance-1.png)

This will get created as a normal column in the database.

    create table WallPost (
      Id         UNIQUEIDENTIFIER   not null,
      PostType   TINYINT   not null,
      DatePosted DATETIME   null,
      Title      NVARCHAR(255)   null,
      Content    NVARCHAR(255)   null,
      Url        NVARCHAR(255)   null,
        primary key ( Id ))
        
When we insert now, a LinkShare and a Text wallpost:

    var wallPost = new TextWallPost
    {
        DatePosted = DateTime.Now,
        Title = "My First Wall Post",
        Content = "Is Awesome!"
    };
    
    var linkPost = new LinkShareWallPost()
    {
        DatePosted = DateTime.Now,
        Title = "My First Link Share",
        Content = "Is Awesome!",
        Url = "http://www.philliphaydon.com/"
    };
    
    session.Save(wallPost);
    session.Save(linkPost);

The link share one will include the Url.

    - statement #1
    INSERT INTO WallPost
               (DatePosted,
                Title,
                Content,
                PostType,
                Id)
    VALUES     ('2011-08-21T23:53:10.00' /* @p0 */,
                'My First Wall Post' /* @p1 */,
                'Is Awesome!' /* @p2 */,
                1,
                '2dc7981b-507b-4d36-8ecc-9f460189a27d' /* @p3 */)
                
    - statement #2
    INSERT INTO WallPost
               (DatePosted,
                Title,
                Content,
                Url,
                PostType,
                Id)
    VALUES     ('2011-08-21T23:53:10.00' /* @p0 */,
                'My First Link Share' /* @p1 */,
                'Is Awesome!' /* @p2 */,
                'http://www.philliphaydon.com/' /* @p3 */,
                2,
                '5edfe0f2-e179-4615-88e4-9f460189a284' /* @p4 */)

The 'Url' column must be null, or have a default value assigned to it so that when inserting a Text wallpost, the column doesn't need to be specified.

If we query for the base class:

    var result = session.QueryOver<WallPost>().List();

This will query for all columns:

    SELECT this_.Id         as Id0_0_,
           this_.DatePosted as DatePosted0_0_,
           this_.Title      as Title0_0_,
           this_.Content    as Content0_0_,
           this_.Url        as Url0_0_,
           this_.PostType   as PostType0_0_
    FROM   WallPost this_

Just like it did before. No changes, likewise if we query for just the Text wallpost, it will not include the 'Url' column:

    var result = session.QueryOver<TextWallPost>().List();

Results in:

    SELECT this_.Id         as Id0_0_,
           this_.DatePosted as DatePosted0_0_,
           this_.Title      as Title0_0_,
           this_.Content    as Content0_0_
    FROM   WallPost this_
    WHERE  this_.PostType = 1

If we query for the LinkShare wall post:

    var result = session.QueryOver<LinkShareWallPost>().List();

This results in the 'Url' column being selected:

    SELECT this_.Id         as Id0_0_,
           this_.DatePosted as DatePosted0_0_,
           this_.Title      as Title0_0_,
           this_.Content    as Content0_0_,
           this_.Url        as Url0_0_
    FROM   WallPost this_
    WHERE  this_.PostType = 2
    
So NHibernate is efficient in that it only queries for what it actually needs. If you extend your sub-classes out to have a couple of properties each then they will only query for the required fields for that sub-class.

It is possible for sub-classes to share properties. For example if introduced a new sub-class, MovieShare, which has a VideoUrl, as well as a SiteUrl property:

![](/images/fnh-table-inheritance-2.png)

We can map the classes like so:

    public class TextWallPostMap : SubclassMap<TextWallPost>
    {
        public TextWallPostMap()
        {
            DiscriminatorValue(1);
        }
    }
    
    public class LinkShareWallPostMap : SubclassMap<LinkShareWallPost>
    {
        public LinkShareWallPostMap()
        {
            DiscriminatorValue(2);

            Map(x => x.Url).Column("Url");
        }
    }

    public class MovieShareWallPostMap : SubclassMap<MovieShareWallPost>
    {
        public MovieShareWallPostMap()
        {
            DiscriminatorValue(3);

            Map(x => x.SiteUrl).Column("Url");
            Map(x => x.VideoUrl).Column("VideoUrl");
        }
    }

When the table is created, 'Url' column is only created once:

    create table WallPost (
      Id         UNIQUEIDENTIFIER   not null,
      PostType   TINYINT   not null,
      DatePosted DATETIME   null,
      Title      NVARCHAR(255)   null,
      Content    NVARCHAR(255)   null,
      Url        NVARCHAR(255)   null,
      VideoUrl   NVARCHAR(255)   null,
        primary key ( Id ))
    
Now when we insert:

    var wallPost = new TextWallPost
    {
        DatePosted = DateTime.Now,
        Title = "My First Wall Post",
        Content = "Is Awesome!"
    };

    var linkPost = new LinkShareWallPost()
    {
        DatePosted = DateTime.Now,
        Title = "My First Link Share",
        Content = "Is Awesome!",
        Url = "http://www.philliphaydon.com/"
    };

    var moviePost = new MovieShareWallPost()
    {
        DatePosted = DateTime.Now,
        Title = "My First Movie Share",
        Content = "Is Awesome!",
        SiteUrl = "http://www.philliphaydon.com/",
        VideoUrl = "http://www.youtube.com/watch?v=GaoLU6zKaws"
    };

    session.Save(wallPost);
    session.Save(linkPost);
    session.Save(moviePost);

The insert will share the same 'Url' column for both the LinkShare, and the MovieShare:

    - statement #1
    INSERT INTO WallPost
               (DatePosted,
                Title,
                Content,
                PostType,
                Id)
    VALUES     ('2011-08-22T00:22:05.00' /* @p0 */,
                'My First Wall Post' /* @p1 */,
                'Is Awesome!' /* @p2 */,
                1,
                '0e9cef50-d609-4a62-8909-9f47000611cb' /* @p3 */)
                
    - statement #2
    INSERT INTO WallPost
               (DatePosted,
                Title,
                Content,
                Url,
                PostType,
                Id)
    VALUES     ('2011-08-22T00:22:05.00' /* @p0 */,
                'My First Link Share' /* @p1 */,
                'Is Awesome!' /* @p2 */,
                'http://www.philliphaydon.com/' /* @p3 */,
                2,
                '8deb343e-941f-4ae1-aba7-9f47000611d0' /* @p4 */)

    - statement #3
    INSERT INTO WallPost
               (DatePosted,
                Title,
                Content,
                Url,
                VideoUrl,
                PostType,
                Id)
    VALUES     ('2011-08-22T00:22:05.00' /* @p0 */,
                'My First Movie Share' /* @p1 */,
                'Is Awesome!' /* @p2 */,
                'http://www.philliphaydon.com/' /* @p3 */,
                'http://www.youtube.com/watch?v=GaoLU6zKaws' /* @p4 */,
                3,
                '6bc80750-3fb2-4830-bccf-9f47000611d0' /* @p5 */)

And querying is still as it was before. No changes.

One really important thing to remember is. You cannot change a type from 1 type to another, meaning you cannot change a LinkShare to a MovieShare. Any sub-class you create should never have any reason to change, if for some reason it DOES change, you should delete it, and create a new one.

By that I mean delete the object, and insert a new one of the specified sub-class. While it is possible to use native SQL to change the discriminator value, there's no way to do it in HQL, Criteria, LINQ, or QueryOver, because it's just wrong. If it needs to change, you probably need to re-think your domain and persistence.

Next post will be about Table per Sub-Class mapping.

