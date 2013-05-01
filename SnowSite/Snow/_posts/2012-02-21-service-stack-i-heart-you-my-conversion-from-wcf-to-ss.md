---
layout: post
title: Service Stack... I heart you. My conversion from WCF to SS
category: ServiceStack
---

I've just spent the weekend ripping out that dreaded WCF abomination and replacing it with Service Stack.

<http://servicestack.net/>

> A modern fresh alternative to WCF. Code-first, convention-based, codegen-free. Encourages best-practices high-performance, scalable REST & RPC web services.

Over the past couple of months I've been fighting with WCF to the point I wanted to slit my wrists.

So I asked [JabbR](http://jabbr.net/) and Twitter if I should use Web API. Well -no- one recommended Web API and everyone recommended Service Stack.

## It's a different way of thinking

The first noticeable difference between WCF and SS (Service Stack) is that I'm no longer writing a single service class with a butt load of methods. Which is most likely a good thing because after a while they just become hard to manage.

So instead of a Contract, Service, Response DTO, and Request DTO, with 9234823 methods defined in the Contract/Service. It's now **1 Request DTO per Service.**

What does that mean?

<!--excerpt-->

Well before I would have something like:

    [ServiceContract]
    public interface IMemberQueryService
    {
        [OperationContract]
        MemberResponse ById(string id);
        [OperationContract]
        MemberResponse ByEmail(string email);

        [OperationContract]
        MemberResponse ByOpenId(string openId);
    }

Obviously with the actual service implementation and all that jazz.

## Implementation with Service Stack

Now with Service Stack I would write that as a single service. This means I need a request class.

    public class MemberRequest
    {
        public string Id { get; set; }
        public string OpenId { get; set; }
        public string Email { get; set; }
    }

<span class="note">**Note:** My 'Id' is a string because I'm using RavenDB and this is an a real example</span>

The next class,the Service itself:

    public class MemberService : IService<MemberRequest>
    {
        private IDocumentStore DocumentStore { get; set; }
        public MemberService(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public object Execute(MemberRequest request)
        {
        }
    }

So now I have a Request and a Service. But the request is meant to handle what the WCF service with three methods was doing, so how is this implemented.

Well rather than having three methods, I simply add the results to a collection and return the result.

The full implementation of this service looks like so:

    public class MemberService : IService<MemberRequest>
    {
        private IDocumentStore DocumentStore { get; set; }
        public MemberService(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public object Execute(MemberRequest request)
        {
            var result = new List<MemberResponse.Member>();

            using (var session = DocumentStore.OpenSession())
            {
                if (!string.IsNullOrWhiteSpace(request.Id))
                {
                    var member = session.Load<Member>(request.Id);
                    if (member != null)
                        result.Add(member.TranslateTo<MemberResponse.Member>());
                }

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var member = session.Query<Member, All_Members>()
                                        .SingleOrDefault(x => x.Email == request.Email);

                    if (member != null)
                        result.Add(member.TranslateTo<MemberResponse.Member>());
                }

                if (!string.IsNullOrWhiteSpace(request.OpenId))
                {
                    var member = session.Query<Member, All_Members>()
                                        .SingleOrDefault(x => x.OpenId == request.OpenId);

                    if (member != null && member.OpenId.Equals(request.OpenId, StringComparison.Ordinal))
                        result.Add(member.TranslateTo<MemberResponse.Member>());
                }
            }

            return new MemberResponse { Results = result };
        }
    }

So if I have any of the information defined on the request object, I simply query for it.

Now if you're looking at the method you're probably thinking the same thing I thought when I first looked at something similar. How the fark do I query that? *Well I'll explain that soon*. :)

The last thing missing tho is the Response.

There's a couple of things to note, in the code above I actually translate my Model to a DTO, this is done using the `TranslateTo<T>` method. This maps the object from 1 object to another, providing the two models share similar properties. This is exactly the same as AutoMapper except it doesn't handle relationships.

It is possible to handle relationships however and I'll demonstrate that in future posts.

Now one thing that annoyed me with WCF was getting null objects as a response. The approach used here is I have a MemberResponse which has a collection of Results.

The actual DTO looks like so:

    public class MemberResponse : IHasResponseStatus
    {
        public IEnumerable<Member> Results { get; set; }
        public class Member
        {
            public string Id { get; set; }
            public string OpenId { get; set; }
            public string DisplayName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        public ResponseStatus ResponseStatus { get; set; }
    }

<span class="note">**Note:** I like to use nested classes for the results because I can name it what it is, and modify it without breaking any other Response classes I make in the future.</span>

The response implements the interface `IHasResponseStatus` (which I think needs to be renamed to ICanHazResponseStatus) which provides the property ResponseStatus, this allows SS to attach it's own information about the response such as exception information.

So rather than WCF where it just faults and throws exceptions and falls over and starts a fire, it just returns a response and gives you the information about it. AWESOME!

Also I've added the Member as a collection so that I can have multiple results if I need, maybe I want to find a user who has an OpenId of 'xyz' and an email of 'abc' so I can link them. No need to write yet ANOTHER WCF method.

## Configuration

Configuration in WCF is always a pain in the ass, specially when dealing with message sizes, buffers, bindings and endpoints, so on and so forth somebody shoot me because WCF configuration is the bane of my existence.

Configuring SS is so easy that I over configured it to begin with. While configuring SS I realised I can remove Autofac, AutoMapper and a bunch of configuration code. The end result was the following:

    public class Global : System.Web.HttpApplication
    {
        public class QueryServiceAppHost : AppHostBase
        {
            private readonly IContainerAdapter _containerAdapter;
            public QueryServiceAppHost(IDocumentStore documentStore)
                : base("ITCompiler Query Services", typeof(MemberService).Assembly)
            {
                base.Container.Register<IDocumentStore>(documentStore);

                base.SetConfig(new EndpointHostConfig { DebugMode = true });
            }

            public override void Configure(Funq.Container container)
            {
                container.Adapter = _containerAdapter;
            }
        }

        private static IDocumentStore DocumentStore { get; set; }

        public void Application_Start()
        {
            DocumentStore = ConfigureRavenDb();

            (new QueryServiceAppHost(DocumentStore)).Init();
        }

        private static IDocumentStore ConfigureRavenDb()
        {
            var documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenDB",
                DefaultDatabase = "ITCompiler"
            }.Initialize();

            IndexCreation.CreateIndexes(typeof(All_Members).Assembly, documentStore);

            return documentStore;
        }
    }

I would show you the original configuration I had for WCF but you would probably freak out and run.

But I've cut out Autofac, AutoMapper, and it's really just 'Configure RavenDB' and 'Initialize SS'

I didn't touch the .config file, didn't do anything special to setup SS, simply created a AppHost class and registered my Document Store.

## Querying the Services

The last piece to the puzzle was querying the newly written services. Usually with WCF I configure the ChannelFactory and then inject a new Channel for every controller that needs specific services.

This caused a lot of configuration since each configured service has it's own endpoint I ended up with a lot of code.

SS creates a reusable client for querying, and all it needs is the base URL of the service host.

I first started just by creating a new client like so:

    var client = new JsonServiceClient("http://localhost:9001");

<span class="note">**Note:** I use the JSON service client but there's a few to choose from, XML, JSV, WCF, SOAP, etc.</span>

Now when calling the client I can specify the response and pass in a request, so lets say I wanted to get a use by email address:

    client.Send<MemberResponse>(new MemberRequest
    {
        Email = "bob@googlelymail.com"
    });
    
This sends a request, and works out which service to invoke, passes in the request, and returns the result.

It couldn't be easier. If I wanted to find a user by Id, just pass a request with just the Id.

Now I setup my application with two different projects, one for Queries, and one for Commands. So when I setup my client I just created two really simple classes:

    public class QueryServiceClient : JsonServiceClient
    {
        public QueryServiceClient(string url) : base(url) { }
    }
And another for Commands named CommandServiceClient.

Then I registered them in Autofac (on the MVC site I'm still using Autofac)

    builder.RegisterType<QueryServiceClient>()
           .WithParameter(new NamedParameter("url", QueryServicesUrl))
           .AsSelf()
           .SingleInstance();
            
    builder.RegisterType<CommandServiceClient>()
           .WithParameter(new NamedParameter("url", CommandServicesUrl))
           .AsSelf()
           .SingleInstance();

Now I can just inject those two service clients and reuse them over and over.

## Conclusion

I had to change my way of thinking and to be honest, I threw in the towel pretty early on. But I stuck with it. I was lucky enough to have help from the creator himself, [@demisbellot](http://www.twitter.com/demisbellot) in the [JabbR ServiceSack](http://jabbr.net/#/rooms/servicestack) room.

He was kind enough to answer all my woes and put me on the right path, regardless of how silly my questions probably were.

After a little perseverance I'm now completely in love with Service Stack and I look forward to learning more of it's capabilities around Error Handling, REST, and Messaging.