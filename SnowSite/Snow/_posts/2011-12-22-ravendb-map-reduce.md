---
layout: post
title: RavenDB - Map Reduce
category: RavenDB
---

So, learning Map Reduce in RavenDB I decided that to take what I learnt from the index created in my previous post. I think I picked something rather difficult to begin with, but I've succeeded 

Given a document Article which has a collection of Tags.

I want to get a Count of each Tag across all Articles.

    public class Content
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }

    public class Tag
    {
        public string Name { get; set; }
    }

<span class="note">**Note:** Tag is it's own class because I added additional properties to it.</span>

<!--excerpt-->

Now I insert some data:

    using (var session = documentStore.OpenSession())
    {
        session.Store(new Content
        {
            Title = "Test Title for a Video",
            Tags = new List<Tag>
            {
                new Tag() {Name = "c#"},
                new Tag() {Name = "autofac"},
                new Tag() {Name = "asp.net"},
            }
        });
        session.Store(new Content
        {
            Title = "Test Title for an Article",
            Tags = new List<Tag>
            {
                new Tag() {Name = "c#"},
                new Tag() {Name = "nhibernate"},
                new Tag() {Name = "fluent-nhibernate"},
                new Tag() {Name = "mvc"}
            }
        });
        session.Store(new Content
        {
            Title = "Test Title for an Article",
            Tags = new List<Tag>
            {
                new Tag() {Name = "ravendb"},
                new Tag() {Name = "asp.net"},
                new Tag() {Name = "autofac"},
                new Tag() {Name = "c#"}
            }
        });
        session.SaveChanges();
    }

So I'm expecting a count of:

- 3 x c#
- 2 x autofac
- 2 x asp.net
- 1 x ravendb
- 1 x mvc
- 1 x nhibernate
- 1 x fluent-nhibernate

I'm going to pull these out with a defined type rather than dynamic/object, so I've created a new class with Count and Name:

    public class TagResult
    {
        public int      Count   { get; set; }
        public string   Name    { get; set; }
    }
    
So creating a new Index:

    public class All_Tags : AbstractMultiMapIndexCreationTask<TagResult>
    {
        public All_Tags()
        {
        }
    }

The first thing I need to do is map out ONLY the Tag's, when I select out the Tag's, I'm also going to include another field called Count, with a default value of 1. This is so I can re-use it to sum the total number of times the tag is used.

    AddMap<Content>(contents => from content in contents
                                from tag in content.Tags
                                select new
                                {
                                    Name = tag.Name,
                                    Count = 1
                                });
                                
This would give me a result that contains duplicates for the tags. Along the lines of:

<table>
  <tr>
    <td>c#</td>
    <td>1</td>
  </tr>
  <tr>
    <td>c#</td>
    <td>1</td>
  </tr>
  <tr>
    <td>c#</td>
    <td>1</td>
  </tr>
  <tr>
    <td>autofac</td>
    <td>1</td>
  </tr>
  <tr>
    <td>autofac</td>
    <td>1</td>
  </tr>
  <tr>
    <td>asp.net</td>
    <td>1</td>
  </tr>
  <tr>
    <td>asp.net</td>
    <td>1</td>
  </tr>
  <tr>
    <td>ravendb</td>
    <td>1</td>
  </tr>
  <tr>
    <td>mvc</td>
    <td>1</td>
  </tr>
  <tr>
    <td>nhibernate</td>
    <td>1</td>
  </tr>
  <tr>
    <td>fluent-nhibernate</td>
    <td>1</td>
  </tr>
</table>

So what I need to do in the Reduce, is group the tags together by their Name.

    Reduce = results => from result in results
                        group result by result.Name into tag
                        select new
                        {
                            Count = tag.Sum(x => x.Count),
                            Name = tag.Key,
                        };
                        
So here, I group all the tags together by their name, but I also sum the 'count' value together to get the total number of times the tag is used.

Now run up the app and view the index:

![](/images/ravendb-map-reduce-1.png)

Now if I query the index:

![](/images/ravendb-map-reduce-2.png)

Awesome. Now to query this, I have to use the TagResult class defined previously, and the All_Tags index just created.

    using (var session = documentStore.OpenSession())
    {
        var result = session.Query<TagResult, All_Tags>()
                            .ToList();
        foreach (var tag in result)
        {
            Console.WriteLine(tag.Count + " x " + tag.Name);
        }

        session.SaveChanges();
    }

Running this I get the following result:

![](/images/ravendb-map-reduce-3.png)

The results I expected previously.

So there you have it. Map Reduce.
