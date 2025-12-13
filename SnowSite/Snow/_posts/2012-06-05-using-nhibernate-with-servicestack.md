---
layout: post
title: Using NHibernate with ServiceStack
category: ServiceStack
---

A few people have asked me how they can use ServiceStack with other persistence technologies like RavenDB and NHibernate, or if you really must... EntityFramework... Rather than [ServiceStack.OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) or [ServiceStack.Redis](https://github.com/ServiceStack/ServiceStack.Redis) like many of the examples in SS show.

<span class="note">**Note:** This isn't about best practice on using NHibernate or ServiceStack, my services are named just to quickly get something up and running.</span>

<span class="note">**Note 2:** This blog post may not be completely inline with the GitHub repository since I will be updating the repository to include some additional samples in the future.</span>

I've created a small sample project which can be found [here on GitHub](https://github.com/phillip-haydon/ServiceStack-NHibernate-Sample), I plan to flesh it out a little bit from the date this is posted, but it's purely there as a sample.

## No Repositories! ##

Utilizing other persistence frameworks is really easy with ServiceStack, the thing with ServiceStack Services is that they are doing something concise, it's a single service implementation, so there's really no need to use repositories for them, you gain absolutely no benefit from using repositories other than adding an additional layer of abstraction and complexity to your services.

<!--excerpt-->

That doesn't mean you don't have to use repositories, if you REALLY want to use them. You can, and I'll add a sample of using a repository with the RestService implementation.

## Setting Up SS ##
Assuming you've setup NHibernate and your mappings, all we need to do is setup ServiceStack.

First things first! Install ServiceStack.

    PM> Install-Package ServiceStack

<span class="note">**Note:** You can use ServiceStack.Host.* (replace the * with MVC or ASPNET) which will automatically configure the web.config. Personally I prefer to do it myself.</span>

Using the Package Manager (or GUI) install Service Stack into your project. This should add the required code to the web.config, if not you can double check your web config is setup like shown [here](https://github.com/ServiceStack/ServiceStack/wiki/Create-your-first-webservice).

Next in the global.asax we want to create an AppHost and configure it:

    public class Global : HttpApplication
    {
        public class SampleServiceAppHost : AppHostBase
        {
            private readonly IContainerAdapter _containerAdapter;
            public SampleServiceAppHost(ISessionFactory sessionFactory)
                : base("Service Stack with Fluent NHibernate Sample", typeof(ProductFindService).Assembly)
            {
                base.Container.Register<ISessionFactory>(sessionFactory);
            }

            public override void Configure(Funq.Container container)
            {
                container.Adapter = _containerAdapter;
            }
        }

        void Application_Start(object sender, EventArgs e)
        {
            var factory = new SessionFactoryManager().CreateSessionFactory();

            (new SampleServiceAppHost(factory)).Init();
        }
    }

In our service host we take in our NHibernate Session Factory, and we wire it up to [Funq (the default IoC container SS uses)](https://github.com/ServiceStack/ServiceStack/wiki/The-IoC-container), this is so when the Service is resolved, it gets the SessionFactory to create a Session.

If you were using RavenDB, this is where you would inject your DocumentStore, and if you were using EntityFramework, you would inject that DataContext thing it uses.

So when the application is started, we create the SessionFactory, and create an instance of the AppHost, passing in the SessionFactory.

## Services ##
Now that SS is setup, we need to implement our services. This part is just as easy.

In some of the SS samples such as [this one](https://github.com/ServiceStack/ServiceStack.Examples/blob/master/src/ServiceStack.MovieRest/MovieService.cs#L135), dependencies are injected via the properties. Personally I don't like this, because the service is absolutely dependent on that dependency. It cannot function without it, so in my opinion this dependency should be done via the constructor.

I'm not going to go over EVERY service implementation, I'm only going to show Insert and Select By Id.

## Insert ##
Besides the model defined for NHibernate, we need our Service Request Model, and we need our implementation.

    public class ProductInsert
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

This is our Service Request Model, really plain and simple DTO used for doing a Product Insert.

    public class ProductInsertService : ServiceBase<ProductInsert>
    {
        public ISessionFactory NHSessionFactory { get; set; }
        public ProductInsertService(ISessionFactory sessionFactory)
        {
            NHSessionFactory = sessionFactory;
        }

        protected override object Run(ProductInsert request)
        {
            using (var session = NHSessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var result = request.TranslateTo<Product>();

                session.Save(result);

                tx.Commit();
            }

            return null;
        }
    }

This is our Service Implementation, as you can see we have a constructor which takes in the `ISessionFactory`, this is our NHibernate `ISessionFactory`, you need to be careful here since ServiceStack has it's own `ISessionFactory`:

![](/images/servicestack-nhibernate-1.png)

We need to make sure this is the NHibernate one:

![](/images/servicestack-nhibernate-2.png)

You can of course, inject your Unit Of Work, or the NHibernate Session, or what ever you like, if you're using Repositories you may opt to inject an instance of your desired repository such as `IProductRepository`. For this example I'm using NHibernates SessionFactory so that the service is responsible for opening a Session and Transaction.

So that's all there is to it, inject your SessionFactory, or your desired persistence implementation, and do your thing.

The cool thing about ServiceStack is it has built in functionality to do mapping.

    var result = request.TranslateTo<Product>();

`TranslateTo<T>` is functionality built into ServiceStack for mapping 1 object to another.

If you want to update an object, ServiceStack handles that too using PopulateWith.

    var existing = session.Get<Product>(request.Id)
                          .PopulateWith(request);
                          
No need to introduce anything like AutoMapper.

## Select By Id ##

This service I've called `ProductFindService`, in the future there will be a `ProductSearchService` to show selection by criteria.

Like the Insert service, I've defined a simple model which only has an Id property for selecting the product out:

    public class ProductFind
    {
        public Guid Id { get; set; }
    }
    
In addition to the Request Model I have a Response Model:

    public class ProductFindResponse : IHasResponseStatus
    {
        public class Product
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public Product Result { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

This has a nested Product class which defines all the properties of a Product. The outer Response object has a Result and Status. ([status is for Exception/Error information](http://www.philliphaydon.com/2012/03/service-stack-exceptions-and-errors/))

As you can see the Response is the same name as the Request, with Response appended to the end, so that SS can create this object itself.

When I setup these Request/Response objects in Visual Studio, I use an extension called [NestIn](http://visualstudiogallery.msdn.microsoft.com/9d6ef0ce-2bef-4a82-9a84-7718caa5bb45) which allows me to select the two classes and nest the Response under the Request like so:

![](/images/servicestack-nhibernate-3.png)

The service is similar to the insert, we inject the SessionFactory, open a Session, no transaction (unless you want to use 2nd level caching, but that's beyond this post) and select out the Product:

    public class ProductFindService : ServiceBase<ProductFind>
    {
        public ISessionFactory NHSessionFactory { get; set; }
        public ProductFindService(ISessionFactory sessionFactory)
        {
            NHSessionFactory = sessionFactory;
        }

        protected override object Run(ProductFind request)
        {
            using (var session = NHSessionFactory.OpenSession())
            {
                var result = session.Load<Models.Product>(request.Id);

                return new ProductFindResponse
                {
                    Result = result.TranslateTo<ProductFindResponse.Product>()
                };
            }
        }
    }

Lastly we return a new Response object, and translate the result from NHibernate to the Response result.

Easy peasy :)

Swapping out NHibernate for anything else like RavenDB or MongoDB is super easy. I hope this helps those few people who have asked me how to use other persistence frameworks get up and running.

I find it amazing how little code you're required to write to get a ServiceStack Service up and running.