---
layout: post
title: RavenDB Inheritance
category: RavenDB
---

<span class="note">**Note:** Updated solution: <http://www.philliphaydon.com/2011/12/ravendb-inheritance-revisited/></span>

Continuing my learning of RavenDB, I wanted to see how it handled Inheritance.

I found: <http://ravendb.net/faq/polymorphic-indexes>

Which showed what to do allow you to select over all types of `Animal` for the example shown. So I wanted to see what happens before and after using this method.

So like the example shown I've created an `Animal`, with a `Dog` and `Cat`.

    public abstract class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class Dog : Animal { }
    
    public class Cat : Animal { }

Now if I insert a Dog and Cat:

    using (var session = documentStore.OpenSession())
    {
        session.Store(new Dog() { Name = "Test Dog" });
        session.Store(new Cat() { Name = "Test Cat" });
        session.SaveChanges();
    }
    
<!--excerpt-->

What's stored in RavenDB is two separate documents, one for 'dogs' and one for 'cats'.

![](/images/ravendb-inheritance-revisited-1.png)

If I include the Convention.

    var documentConvention =
        new DocumentConvention()
            {
                FindTypeTagName =
                    type =>
                        {
                            if (typeof (Animal).IsAssignableFrom(type))
                                return "animals";
                            return DocumentConvention.DefaultTypeTagName(type);
                        }
            };
            
<span class="note">**Note:** You can do the conversion when the DocumentStore is initialized, I broke the two up so that it would fit easier into my blog. Otherwise it's too nested and yucky.</span>

    var documentStore =
        (new DocumentStore()
                {
                    Url = "http://localhost:8080",
                    Conventions = documentConvention
                }).Initialize();
                
Now when I insert a Dog and Cat I get:

![](/images/ravendb-inheritance-revisited-2.png)

Awesome. If we look at the document however:

![](/images/ravendb-inheritance-revisited-3.png)

There is no information about it being a cat or dog, I thought it would add some sort of discriminator similar to how NHibernate works.

However, if we look at the Metadata tab:

![](/images/ravendb-inheritance-revisited-4.png)

We can see the CLR type is stored in the metadata so RavenDB knows what type to create when we query it.

This means if we query for `Animal` we get a list of Dogs and Cats.

    using (var session = documentStore.OpenSession())
    {
        var result = session.Query<Animal>();
        foreach (var animal in result)
        {
            Console.WriteLine(animal.Name);
        }
    }
 
![](/images/ravendb-inheritance-revisited-5.png)

However, if you wanted to query for just Dogs, like so:

    var result = session.Query<Dog>().ToList();
    
It doesn't seem to work :(

![](/images/ravendb-inheritance-revisited-6.png)

I'm probably just doing something wrong, either way, the more I play with RavenDB. The more I love it.

