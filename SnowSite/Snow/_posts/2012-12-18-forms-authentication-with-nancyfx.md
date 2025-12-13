---
layout: post
title: Forms Authentication with NancyFX
category: NancyFX
---

There's already quite a bit of documentation on the Nancy wiki about forms authentication, but I wanted to write about it anyway.

<span class="note"><strong>Note:</strong> This is written with Nancy 0.14.1 - this may be subject to change in future versions.</span>

Nancy supports a module for forms authentication, it works some-what similar to the way forms auth works in ASP.NET, except it's abstracted away and is not part of the core Nancy engine, which is great. It means that if you don't like the way forms authentication works, you can rip it out and write your own from scratch, or download the project and modify it to your hearts content.

<a href="http://nuget.org/packages/Nancy.Authentication.Forms">http://nuget.org/packages/Nancy.Authentication.Forms</a>

<a href="https://github.com/NancyFx/Nancy/tree/master/src/Nancy.Authentication.Forms">https://github.com/NancyFx/Nancy/tree/master/src/Nancy.Authentication.Forms</a>

If you take a look at the GitHub project, you can see that the implementation is actually really small. So should you want to poke around, there's not much to look at.

<!--excerpt-->

### Packages ###

First up we need to install the packages, Nancy, Nancy.Hosting.Aspnet, and Nancy.Authentication.Forms

    PM> install-package Nancy 
    Successfully installed "Nancy 0.14.1". 
    Successfully added "Nancy 0.14.1" to Nancy.FormsAuth.
    
    PM> install-package Nancy.Authentication.Forms 
    Attempting to resolve dependency "Nancy (= 0.14.1)". 
    Successfully installed "Nancy.Authentication.Forms 0.14.1". 
    Successfully added "Nancy.Authentication.Forms 0.14.1" to Nancy.FormsAuth.

    PM> install-package Nancy.Hosting.Aspnet 
    Attempting to resolve dependency "Nancy (= 0.14.1)". 
    Successfully installed "Nancy.Hosting.Aspnet 0.14.1". 
    Successfully added "Nancy.Hosting.Aspnet 0.14.1" to Nancy.FormsAuth.
    
### Configuring Forms Auth ###

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            container.Register<IUserMapper, DatabaseUser>();
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            var formsAuthConfiguration = new FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>(),
            };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }
    
This is all I've done to the bootstrapper. The first method ConfigureRequestContainer registers a class for IUserMapper. Currently it's in red because it doesn't exist yet, we need to create it :)

The second method configures the Forms Authentication plugin. All we need to define is the redirect URL for unauthenticated requests. That means if the user attempts to go to a page that requires authentication, they will be redirected here instead. Using the IoC container we resolve the registered IUserMapper class which is what is used to retrieve the user from the Database (or where ever you persist your users)

### Creating a class implementing IUserMapper ###
    
    public class DatabaseUser : IUserMapper
    {
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            throw new NotImplementedException();
        }
    }

The IUserMapper interface only requires you to implement 1 method. This method is used to authenticate the user on each request and return some basic data of the user.

So we need to create a user to return that implements IUserIdentity.

    public class AuthenticatedUser : IUserIdentity
    {
        public string UserName { get; set; }
        public IEnumerable<string> Claims { get; set; }
    }

That's all we need for the user. Now we need to implement the DatabaseUser class.

The method GetUserFromIdentifier takes a Guid. This is what's used to identify the user, it's a Guid because it's a good idea to not use an identifier that is easily guessable.

It does mean that your persisted user needs to have an identifier that's a Guid. This doesn't mean the PrimaryKey needs to be a Guid, you can leave it as an INT or what ever your database currently uses. But add a new field to the table or document etc.

    public class DatabaseUser : IUserMapper
    {
        public IDocumentStore DocumentStore { get; set; }
        public DatabaseUser(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var member = session.Query<Member>().SingleOrDefault(x => x.Identifier == identifier);

                if (member == null)
                    return null;

                return new UserIdentity
                {
                    UserName = member.DisplayName,
                    Claims = new []
                    {
                        "NewUser",
                        "CanComment"
                    }
                };
            }
        }
    }

This example is using RavenDB for data access, it's pulling the Member based on an Identifier property, but same applies to any persistence you use, SQL Server, MongoDB, etc.

Now we have configured everything, now we just need to Login, and visit a page we can't access. Lets create two modules:

### HomeModule ###

    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ =>
            {
                return "Hello World";
            };
            Get["/login"] = _ =>
            {
                return View["login.html"];
            };
        }
    }

### SecureModule ###

    public class SecureModule : NancyModule
    {
        public SecureModule()
        {
            this.RequiresAuthentication();
            Get["/secure"] = _ =>
            {
                return "I'm secure!";
            };
        }
    }

The two Modules are really simple, for the purpose of demoing :) as you can see the Secure module calls RequiresAuthentication, this calls the extension method defined in the Nancy's security, this as a result, calls the the Forms Authentication plugin.

<a href="https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Security/ModuleSecurity.cs">https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Security/ModuleSecurity.cs</a>

Now when we visit the webpage /secure we end up at the login page.

<img src="/images/image31.png" />

Now we need to implement the login. So on the HomeModule I'll add a POST handler.

    Post["/login"] = _ =>
    {
        var loginParams = this.Bind<LoginParams>();
        Member member;
                
        using (var session = store.OpenSession())
        {
            member = session.Query<Member>().SingleOrDefault(x => x.Username == loginParams.Username);
            if (member != null && member.Password != loginParams.Password)
            {
                return "username and/or password was incorrect";
            }
        }

        return this.LoginAndRedirect(member.Identifier, fallbackRedirectUrl: "/");
    };

Once we verify that the user has successfully logged in, we call LoginAndRedirect extension method, passing in the Identifier. This makes a cookie which is sent back on each request to authenticate.

Now that's implemented, we can login and visit the Secure page:

<img src="/images/image32.png" />

So when we hit the secure module, and put a break point on the DatabaseUser:

<img src="/images/image33.png" />

As you can see Nancy has invoked it, passing in the identifier which is then used to find the user in the database and return it.

This also populates the CurrentUser on the context:

<img src="/images/image34.png" />

To log a user out again, all you need to do is call the Logout() extension method inside your own module and you're done :)

That's the basic use of Forms Authentication using Nancy.

One thing people get concerned about is the DatabaseUser class querying the database on every request. For 99% of the scenarios you ever face this is perfectly fine, this is a select by Id, and should be fast even if you have a few million records in the table.

If your table is slow to query then you have much bigger issues to worry about, and you should be fixing those issues!

Next post I'm going to explain how you can run two Forms Authentications for two different sections of the website. i.e Normal users and Admin users. I'll also explain how this identifier is beneficial to make it so users can only be logged on in 1 location at a time.
