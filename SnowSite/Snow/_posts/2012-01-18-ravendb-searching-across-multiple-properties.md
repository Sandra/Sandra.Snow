---
layout: post
title: RavenDB - Searching across multiple properties
category: RavenDB
---

Ayende recently posted about <a href="http://ayende.com/blog/152833/orders-search-in-ravendb">Orders Search</a> in RavenDB which got me a little bit excited, since I was pondering how I would do searching in RavenDB without having Full Text Searching from SQL Server.

So digging into it I wanted to try it out for myself how to use it. Given the model:

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTime DatePosted { get; set; }
    }
    
I've setup 10 posts (<a href="http://pastie.org/3200462">click here for the insert pastie</a>) just with some really basic data.

So I'm going to detail here all the data that I've setup.

<!--excerpt-->

## Tags ##

<table border="0" cellspacing="0" cellpadding="2" width="400">
  <tbody>
    <tr>
      <td valign="top" width="200"><strong>Tag Name</strong></td>
      <td valign="top" width="200"><strong># of Posts Containing Tag</strong></td>
    </tr>
    <tr>
      <td valign="top" width="200">html</td>
      <td valign="top" width="200">3</td>
    </tr>
    <tr>
      <td valign="top" width="200">c#</td>
      <td valign="top" width="200">6</td>
    </tr>
    <tr>
      <td valign="top" width="200">ravendb</td>
      <td valign="top" width="200">4</td>
    </tr>
    <tr>
      <td valign="top" width="200">nhibernate</td>
      <td valign="top" width="200">3</td>
    </tr>
    <tr>
      <td valign="top" width="200">javascript</td>
      <td valign="top" width="200">1</td>
    </tr>
    <tr>
      <td valign="top" width="200">coffeescript</td>
      <td valign="top" width="200">2</td>
    </tr>
    <tr>
      <td valign="top" width="200">less</td>
      <td valign="top" width="200">3</td>
    </tr>
    <tr>
      <td valign="top" width="200">search</td>
      <td valign="top" width="200">6</td>
    </tr>
    <tr>
      <td valign="top" width="200">closures</td>
      <td valign="top" width="200">1</td>
    </tr>
    <tr>
      <td valign="top" width="200">jquery</td>
      <td valign="top" width="200">2</td>
    </tr>
    <tr>
      <td valign="top" width="200">css</td>
      <td valign="top" width="200">1</td>
    </tr>
    <tr>
      <td valign="top" width="200">queryover</td>
      <td valign="top" width="200">2</td>
    </tr>
    <tr>
      <td valign="top" width="200">mapreduce</td>
      <td valign="top" width="200">4</td>
    </tr>
  </tbody>
</table>

## Titles ##

Nothing interesting, just 'Test Post X' for each one to identify them.

## Description ##

Basically for this testing I've taken the blog post names of a few things from Google Reader, that some-way relate to the tags above. Take a look at the script mentioned above to see the data.

## Creating the Index ##

So the first thing I want to do is create a Map with a Reduce Result, but we aren't going to add the Reduce to the index, since we don't need it to store that data or do anything with it. We purely want the Reduce Result that matches the map, so that we can query against it.

    public class Post_Search : AbstractIndexCreationTask<Post, Post_Search.ReduceResult>
    {
        public class ReduceResult
        {
            public object[] SearchQuery { get; set; }
            public DateTime DatePosted { get; set; }
        }
        
        public Post_Search()
        {
            Map = posts => 
                  from post in posts
                  select new 
                  {
                      SearchQuery = post.Tags.Concat(new[]
                      {
                          post.Description,
                          post.Title
                      }),
                      DatePosted = post.DatePosted
                  };
        }
    }

This index is a little bit funky, and differs from what Ayende showed in his example. I wanted to try something a little different.

In my scenario I have a collection of Tag's that I wanted to include in the search, this the tags is already a collection, I concatenate the additional array of items I want to add into the map.

The SearchQuery is the property that we will search against, while the DatePosted wont be included in the Search, but is there to provide additional filtering on my search.

## Querying ##

Querying threw me off at first, because in order to query against this index, we have to specify the ReduceResult class.

So we end up with the starting of our query looking like this:

    var result = session.Query<Post_Search.ReduceResult, Post_Search>()

At first I thought "oh, that means we end up with a ReduceResult result type, this is pointless and useless". But I commented on Ayende's blog post and it turns out we can call 'As<T>' on the query.

Without filtering the results just yet, our query would look like the following:

    var result = session.Query<Post_Search.ReduceResult, Post_Search>()
                        .As<Post>()
                        .ToList();

So if I run this up now, for a quick test, I should get 10 results back of type Post

<img src="/images/ravendb-searching-1.png" />

Great!

So now I need to begin filtering out the results. To begin with I'm doing to use the .Where extension. Since we are looking an object array, we can't directly compare it to a string, but if we explicitly cast it to an object we can look for:

coffeescript expecting 2 results:

    var result = session.Query<Post_Search.ReduceResult, Post_Search>()
                        .Where(x => x.SearchQuery == (object)"coffeescript")
                        .As<Post>()
                        .ToList();

<img src="/images/ravendb-searching-2.png" />

How about javascript expecting 2 (1 via Tag and 1 via the Description)

<img src="/images/ravendb-searching-3.png" />

Oh, we didn't get the desired result... This is because the search is only doing a search on an exact match. Since the search value is an exact match of the tag, the result is returned.

So to fix this we need to make the index analysed. Adding to the index:

    Index(x => x.SearchQuery, FieldIndexing.Analyzed);

If we run the exact same query again:

<img src="/images/ravendb-searching-4.png" />

Now we get 2 results.

Now to try something a little bit different, using 'Search', if we wanted to search for something like mvc which happens to only be in the description, rather than using 'Where' like shown above, we can use 'Search' like so:

    var result = session.Query<Post_Search.ReduceResult, Post_Search>()
                        .Search(x => x.SearchQuery, "mvc")
                        .As<Post>()
                        .ToList();

This will give us the same result, except it looks much cleaner

<img src="/images/ravendb-searching-5.png" />

Now there's 1 catch I've found with this, which is searching is always an exact match. I'm not sure (no research done into lucene yet) if lucene has the ability to do a wild-card type search similar to SQL like: '%mvc%', but you can get suggestions from this.

For example if I search for 'coffee' rather than 'coffeescript' I would expect all documents containing 'coffee' to be returned. This doesn't happen. It does give you suggestions though.

Looking at the management studio for 'coffee' :

<img src="/images/ravendb-searching-6.png" />

<em>Side Comment: I think it would be cool if RavenDB provided the ability to have say include suggestions, like:</em>

    var result = session.Query<Post_Search.ReduceResult, Post_Search>()
                        .Search(x => x.SearchQuery, "coffee")
                        .IncludeAllSuggestions()
                        .As<Post>()
                        .ToList();

Or other variations such as:

- .Suggestions.IncludeAll()
- .Suggestions.IncludeTop(3)
- .Suggestions.IncludeAll(WhenResults.AreEmpty)
- .Suggestions.IncludeAll(WhenResults.AreLessThan, 10)

Hopefully you can work out where I'm going with this?

Ok continuing on. Why do we need to call 'As<T>()' on the query?

Well from my understanding of how RavenDB works is like this, when we create an index, it's creating a sub-set of data that points to the document in RavenDB.

For example I have all those documents inserted (<a href="http://pastie.org/3200462">link for the lazy</a>), and these are all stored like so:

<img src="/images/ravendb-searching-7.png" />

When we created the index with the following Map:

    Map = posts => 
          from post in posts
          select new 
          {
              SearchQuery = post.Tags.Concat(new[]
              {
                  post.Description,
                  post.Title
              }),
              DatePosted = post.DatePosted
          };

It basically created an index that looks like this, for the data above:

<table>
  <tbody>
    <tr>
      <td valign="top" rowspan="2" width="50">posts/2</td>
      <td valign="top" width="540">SearchQuery: ["c#", "nhibernate", "search", "queryover", "Benjamin Day slides us into "How to be a C# ninja in 10 easy steps"", "Test Post 2"</td>
    </tr>
    <tr>
      <td valign="top">DatePosted: "2012-01-02T00:00:00.0000000"</td>
    </tr>
  </tbody>
</table>

So the index actually points directly to a Document in RavenDB, when we search against the index, if a match is found, the index returns the Id 'posts/2' back, and that knows to go to the posts collection and grab the document with Id 2.

The problem with the query is we need to specify an object to query against.

So we introduced the ReduceResult <em>(not sure on this naming but I took it from Ayende's blog)</em>, this allows us to specify the Properties we defined in our index, as search criteria, but now our query is expecting ReduceResult:

<img src="/images/ravendb-searching-8.png" />

By specifying as we are telling the query that our result is going to be a type of 'Post':

<img src="/images/ravendb-searching-9.png" />

## Conclusion ##

This functionality is really cool, it allows us to easily search against multiple different properties without having to create messy conjunctions in our LINQ. If we were to attempt to do this without an index, we would probably end up writing something like:

    var result = session.Query<Post>()
                        .Where(x =>
                                x.Description.Contains("c#")
                                ||
                                x.Tags.Any(y => y == "c#")
                                ||
                                x.Title.Contains("c#")
                            )
                        .ToList();

And really, that's just nasty... Specially considering we get the same results for writing more readable code:

<img src="/images/ravendb-searching-10.png" />