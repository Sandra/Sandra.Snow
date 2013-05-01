---
layout: post
title: RavenDB - Changing the Lo on the HiLo Generator
category: RavenDB
---

Well I'm currently learning RavenDB, it's awesome! But I noticed when I put data in, all the Id's generated every time I ran up my application to test were:

1, 2, 3, 4, 5...

1024, 1025, 1026, 1027, 1028...

2048, 2049, 2050, 2051, 2052...

This would be fine after the app is deployed since I wouldn't be restarting it over and over and over, but during development I personally find it annoying that the numbers jump so high.

Fortunately I figured out a way. (which about an hour later I found on Google Groups, granted I had to use a different keyword to find it)

Basically you just need to create a new instance of the `MultiTypeHiLoKeyGenerator` class, passing in the arguments and assigning it to the document store:

    var documentStore = (new DocumentStore { 
        Url = "http://localhost:12321/" 
    }).Initialize();
    
    var generator = new MultiTypeHiLoKeyGenerator(documentStore, 10);
    
    documentStore.Conventions.DocumentKeyGenerator = 
        entity => generator.GenerateDocumentKey(documentStore.Conventions, entity);

    using (var session = documentStore.OpenSession())
    {
        session.Store(new Project() { Title = "Hello World" });
        session.SaveChanges();
    }

So running up my app once:

<!--excerpt-->

![](/images/ravendb-hilo-1.png)

And again:

![](/images/ravendb-hilo-2.png)

Now the identity only increases every time the app restarts. And to show it generates more than 1 number...

![](/images/ravendb-hilo-3.png)

It took a while of hunting on the net, but it turns out Googling & Binging, or searching (StackOverflow/Google Groups) for the keyword `Lo` doesn't work, the argument is `capacity` and searching for that on Google Groups lead me here:

[http://groups.google.com/group/ravendb/browse_thread/thread/95a5b33a5d30eb71/a5197e2e01376e65?lnk=gst&q=capacity#a5197e2e01376e65](http://groups.google.com/group/ravendb/browse_thread/thread/95a5b33a5d30eb71/a5197e2e01376e65?lnk=gst&q=capacity#a5197e2e01376e65)

Hopefully someone else finds this useful :)
