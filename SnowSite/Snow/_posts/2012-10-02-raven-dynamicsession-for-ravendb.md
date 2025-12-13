---
layout: post
title: Raven.DynamicSession for RavenDB
category: RavenDB
---

*Just a word of warning before you read this. I currently don't recommend using this project. MAYBE in the future you could use it, but for now I think it's just a cool proof of concept or something handy for rapid prototyping.*

Last week I had a discussion with [@PrabirShrestha](https://twitter.com/@PrabirShrestha) in [JabbR](https://jabbr.net/) about APIs for MongoDB to achieve something along the lines of:

    db.posts.insert({title: ''first''}, function(err, doc){})
    
Example from: <http://alexeypetrushin.github.com/mongo-lite/docs/index.html>

Or

    _db.Users.Insert(Name: "Steve", Age: 50);
    
Example from: <http://simplefx.org/simpledata/docs/pages/Modify/AddingData.html>

Looking at the syntax and thinking about C#'s DynamicObject I decided to see what I could come up with. I spent the better part of the next 2-3 hours messing around in Visual Studio, chatting and coming back to it, and came up with this gist.

<https://gist.github.com/3798206>

<!--excerpt-->

I basically came up with a really rough working prototype. Currently it doesn't do much, but what it allows you to do is wrap RavenDB's IDocumentSession in what I've called 'DynamicSession' inside this class is a nested class called Chainer, which is/will be responsible for all the method/property chaining that occurs to make the API completely dynamic.

Rather than creating a normal session, you create OpenDynamicSession. All it really does is save you the hassle of having to write two using statements.

    public static class DynamicSessionExtension
    {
        public static DynamicSession OpenDynamicSession(this IDocumentStore store)
        {
            return new DynamicSession(store.OpenSession());
        }
    }

It just opens the normal RavenDB session and passes it in.

    using (dynamic session = store.OpenDynamicSession())
    {
        session.Bananas.insert(new
        {
            Colour = "Yellow",
            Bunch = 14
        }, 1);
        session.SaveChanges();
    }

This code snippet allows you to call a dynamic property, 'Bananas', this refers to the collection. (Word of warning, RavenDB is case sensitive, I don't know what to do in this scenario)

Then the next part is the command, which takes in an anonymous type as the first parameter, and an id as the second parameter. I haven't figured out how to let RavenDB generate the Id yet, not sure if it's possible but will see :)

    using (dynamic session = documentStore.OpenDynamicSession())
    {
        dynamic post1 = session.Posts.load(123);
        Console.WriteLine(post1.Name);
    }
    
In this scenario. I'm just calling Load which returns a dynamic type, which you can call all the properties you want off.

I've created no POCOs :)

So far the two obstacles I've had to overcome have been:

## Collection Names ##

RavenDB uses meta data to put the document into a collection. When inserting, RavenDB doesn't know what the collection is since there's no class name to infer it from. To get around this I worked out that when you have an object from RavenDB, you can access the meta data. This is done like so:

    var metadata = Session.Advanced.GetMetadataFor(objectToStore);
    
This allows me to use the collection named accessed previously to assign it to the RavenDB Entity Name:

    metadata["Raven-Entity-Name"] = CollectionName;

This allows all the documents to be grouped together and be placed under the correct collection.

As I mentioned before RavenDB is case sensitive, so if you insert:

`session.Posts` then `session.posts`, these will be stored in two different collections  (this is a feature of RavenDB I personally don't like, yes I LOVE RavenDB, but even it has things I don't like, nothing is perfect)

Meta data brings me to the 2nd issue...

## CLR Type ##

RavenDB persists the CLR Type in the meta data, this is so when it uses Newtonsoft.Json it can use that data to create the object you want, and return it.

The problem with inserting anonymous types, is there is no real CLR Type information to persist, so what you end up with is all your entities being persisted as:

> "Raven-Clr-Type": "<>f__AnonymousType11[[System.String, mscorlib]], Raven.DynamicSession.TestConsole"

This prevents us from doing:

    using (var session = store.OpenSession())
    {
        result = session.Load<Post>("posts/123");
    }
    
Since RavenDB is unable to convert the type. This happens to be another rather annoying thing I find in RavenDB. When using Index's you can get away with casting them using As<T> or calling AsProjection<T> when the type differs in terms of properties. But when accessing a specific document via the Id, you can't return your own specified type, regardless if the properties match.

I got around this by using a convention, first of all I persist MY OWN information to the meta data :)

    metadata[DynamicClrTypePlaceHolder] = CollectionName;
    
Which persists to the document like so:

> "Raven.DynamicSession.DynamicClrTypePlaceHolder": "People"

Then I setup the store with a convention.

    store.Conventions.FindClrType = (id, doc, metadata) =>
    {
        var clrType = metadata.Value<string>(DynamicSession.DynamicClrTypePlaceHolder);
        if (clrType.Equals(clrPlaceHolder, StringComparison.OrdinalIgnoreCase))
            return type.FullName;

        return metadata.Value<string>(Constants.RavenClrType);
    };

This checks to see if it matches and then returns the type I define rather than what RavenDB has set.

Here's a working test: <https://github.com/phillip-haydon/Raven.DynamicSession/blob/master/src/Raven.DynamicSession.Tests/QueryGetFixture.cs>

At the moment this convention is hard-coded to handle one type but I plan on expanding it out and putting it into some configuration you can setup once.

This project can be found on GitHub:

<https://github.com/phillip-haydon/Raven.DynamicSession>

I plan to continue expanding on it and trying to cover all the basics of RavenDB, some of it's not easy and the hacks I have to put in place to handle POCOs makes me feel that this can never be used in the real world.

But as a prototype I think it's pretty cool!

Interested in hearing anyone's feedback. It's still changing, I've spent maybe 4 hours total playing around with this. So there hasn't been a LOT of work involved. I did have some help from StackOverflow, around wanting to know if it's possible to chain dynamic methods, but turns out you need to create new objects with information and chain those.

<http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject>

