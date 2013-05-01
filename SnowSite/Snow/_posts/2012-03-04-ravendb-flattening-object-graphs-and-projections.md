---
layout: post
title: RavenDB - Flattening Object Graphs and Projections
category: RavenDB
---

One of the guys in [JabbR RavenDB](http://jabbr.net/#/rooms/RavenDB) chat room had a pretty interesting problem that took a while to solve. The problem was that he wasn't trying to return anything to do with the original document, but get a flattened view of some information inside the document.

The scenario was a **Game Server** which a collection of **Connected Users**.

    public class GameServer
    {
        public string Id { get; set; }
        public string ServerName { get; set; }
        public IEnumerable<User> ConnectedUser { get; set; }
        public class User
        {
            public string UserId { get; set; }
            public string Name { get; set; }
            public DateTimeOffset DateConnected { get; set; }
        }
    }

Given say 3 servers with a bunch of users on each server, and searching for a user who's name begins with 'b' the expected result was along the lines of:

<!--excerpt-->

<table>
  <tbody>
    <tr>
      <th valign="top" width="100"><strong>UserId</strong></th>
      <th valign="top" width="100"><strong>Name</strong></th>
      <th valign="top" width="144"><strong>DateConnected</strong></th>
      <th valign="top" width="254"><strong>ServerName</strong></th>
    </tr>
    <tr>
      <td valign="top" width="100">users/3</td>
      <td valign="top" width="100">Bob</td>
      <td valign="top" width="144">15/03/2012 12:44</td>
      <td valign="top" width="254">iPGN CS #01 Iceworld</td>
    </tr>
    <tr>
      <td valign="top" width="100">users/2</td>
      <td valign="top" width="100">Bill</td>
      <td valign="top" width="144">15/03/2012 1:23</td>
      <td valign="top" width="254">3FL CS #4</td>
    </tr>
    <tr>
      <td valign="top" width="100">users/8</td>
      <td valign="top" width="100">Benny</td>
      <td valign="top" width="144">15/03/2012 1:18</td>
      <td valign="top" width="254">3FL CS #4</td>
    </tr>
  </tbody>
</table>

So basically for each user you get the server he's connected to.

We tried everything under the sun, Reduce, Transform, etc. But couldn't figure out how to get the results. Infact using a Transform we could get the # of results returned, except they were all NULL. :(

Turns out it's rather easy using just a Map and `AsProjection<T>`

## Setup Data ##

Test data can be viewed here: <http://pastie.org/3516113>

## Index ##

The index is really easy, it's basically a Select Many map, but we also need to store the results from the Map.

    public class GameServers_ConnectedUsers :
        AbstractIndexCreationTask<GameServer, GameServers_ConnectedUsers.IndexResult>
    {
        public GameServers_ConnectedUsers()
        {
            Map = servers => from s in servers
                                from y in s.ConnectedUsers
                                select new
                                {
                                    ServerName = s.ServerName,
                                    UserName = y.Name,
                                    DateConnected = y.DateConnected,
                                    UserId = y.UserId
                                };
            Store(x => x.ServerName, FieldStorage.Yes);
            Store(x => x.UserName, FieldStorage.Yes);
            Store(x => x.DateConnected, FieldStorage.Yes);
            Store(x => x.UserId, FieldStorage.Yes);
        }

        public class IndexResult
        {
            public string ServerName { get; set; }
            public string UserName { get; set; }
            public DateTimeOffset DateConnected { get; set; }
            public string UserId { get; set; }
        }
    }

The reason for storing the results from the Map is because if we don't, we don't actually get any results back. (will show you soon)

## Querying ##

So querying is the same as always, only we need to provide the 'AsProjection<T>' to the query, like so:

    var results = 
        session.Query<GameServers_ConnectedUsers.IndexResult, GameServers_ConnectedUsers>()
               .Where(x => x.UserName.StartsWith("b"))
               .AsProjection<GameServers_ConnectedUsers.IndexResult>()
               .ToList();
               
The Projection just needs to be an object that matches the result, otherwise it will attempt to return the original documents.

If we run the query without the `AsProjection<T>` we end up with an exception because the result (the original document) doesn't match the object we were querying against `IndexResult`

![](/images/ravendb-flattern-1.png)

If we set the `AsProjection` to `dynamic`, or `GameServer`, we get the original documents. BUT the funny thing is if you use `GameServer` you end up with as many results as the projection has. In this case 3.

![](/images/ravendb-flattern-2.png)

If we expand them out, we see we actually get a duplicate.

![](/images/ravendb-flattern-3.png)

But if we use `dynamic` we get unique documents:

![](/images/ravendb-flattern-4.png)

Using the `IndexResult` (or an object that matches the projection) we get the projected results that we wanted at the start of this post:

![](/images/ravendb-flattern-5.png)

Three results, and all the correct data:

![](/images/ravendb-flattern-6.png)

## Storing the results ##

I showed in the index that I was storing the results I wanted in the projection. This is because if we don't, we end up with this:

![](/images/ravendb-flattern-7.png)

Not only do we end up with the incorrect number of results, they are missing data that isn't found in the original document.

You can see `GameServer` is there, that's because it's found on the document. We need to `store` the data so that RavenDB can return it, that means we use more disk space, but without it, RavenDB would have to query and assume the data that you wanted to return. By storing it RavenDB just returns what matches the query, and doesn't do any extra work.

So if you want to return the projected results, then you need to Store them.

I've put a gist here for anyone interested.

<https://gist.github.com/1972646>
