---
layout: post
title: Service Stack Exceptions and Errors
category: ServiceStack
---

<span class="note">**Note:** This post on ServiceStack is to do with the C# Client. JavaScript posts will be coming in the future.</span>

One of the most painful experiences with WCF is exceptions, if using WCF makes you want to slit your wrists, exceptions in WCF will make you want douse yourself in petrol and light yourself on fire.

Before you even get to your code exceptions, you have to wade yourself through piles of retarded errors to do with Contract Mismatching, Forcefully Disconnected, Binding Issues, Random Faults... The list goes on.

Then, it all works in development, and you put it in production, and it doesn't work, and to debug it you got to modify the config file, setup the [diagnostics logging](http://msdn.microsoft.com/en-us/library/ms732023.aspx), then view it and trawl through piles of crap.

![](/images/service-stack-errors-1.png)

(image taken from: <http://weblogs.asp.net/nmarun/archive/2011/06/10/wcf-service-trace-viewer-part-1.aspx>)

And in the end, it was probably a PEBKAC issue where you forgot to put a stupid attribute on a property...

<!--excerpt-->

At my previous job, a couple of my friends were up until about 4am, they spent 16 hours debugging a WCF issue, because it's a pain.

## Enter Service Stack ##

One of the things I looked at early on was how it handled errors, then I neglected them because I lost interest.There's a couple of exceptions that aren't SS related. The main one you may come across is:

> WebException   
> Unable to connect to remote server

This is kind of obvious, either the remote server doesn't exist or you mistyped the URL, or maybe there's a firewall issue or something, what ever it is, it can't connect to your service.

> HttpException  
> Maximum request length exceeded.

You may also get an HttpException if your requests are larger than 4mb, tho that's pretty large, not sure what you're sending to get that exception. But you can modify the [maxRequestLength](http://msdn.microsoft.com/en-us/library/e1f13641(vs.71).aspx) to get around this one.

### MethodNotAllowed ###

This method is the easiest to fix, it basically means your service hasn't been implemented, or maybe it was implemented but you didn't tell the AppHost about it.

### General Exceptions ###

General exceptions as I'll call, them, are any unhandled exceptions that are thrown, or ones that you explicitly throw yourself.

These exceptions are always thrown as a [WebServiceException](https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack.Common/ServiceClient.Web/ServiceClientBase.cs#L278). This means we can capture these exceptions in a try-catch like so:

    try
    {
    }
    catch (WebServiceException webEx)
    {
        //Handle our Web Service Exception
    }
    
There's a couple of different ways you can throw exceptions, or errors. You can raise your own:

    throw new ApplicationException("Ahhhh stuff happened :S");

And then you will get this when you call the service.

![](/images/service-stack-errors-2.png)

The second way is to throw an `HttpError`. There are a few predefined errors:

![](/images/service-stack-errors-3.png)

That allow you to just pass a message:

    throw HttpError.NotFound("Coffee wasn't found :( sad panda");

![](/images/service-stack-errors-4.png)

Or you can throw your own new one which allows you to define the `HttpStatusCode`:

    throw new HttpError(HttpStatusCode.PaymentRequired, 
        "Kneedz monies plz", "Please deposit monies into my bank account :)");

![](/images/service-stack-errors-5.png)

This is great stuff, it gives a LOT of flexibility to be able to give the client informative error messages rather than the infamous:

> The connection was closed unexpectedly

## My errors are empty ##

So I threw an error and I didn't get it on the client:

![](/images/service-stack-errors-6.png)

As you can see, I've lost a lot of information :( This is while throwing the exact same HttpError in the previous example.

One of the little catches with this is the automated error handling shown above requires a naming convention of your service Request/Response objects.

Lets say we have a `UserSearch` request object.

If an exception is thrown, ServiceStack will try and find an object of the same name, with the suffix 'Response'

This this case it would look for `UserSearchResponse`.

<https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack.ServiceInterface/ServiceUtils.cs#L124>

As you can see, the method `GetResponseDtoName` takes in what your Request object was, and appends the `ResponseDtoSuffix`

The second catch is that your Response DTO must have a property on it named `ResponseStatus`:

    public ResponseStatus ResponseStatus { get; set; }
    
Although it's not required, I personally add the interface `IHasResponseStatus` to my response DTO objects. This ensures that I remember to add the property, and it's named correctly with the correct type.

Here is an example:

    public class UserSearch
    {
        public string NameStartsWith { get; set; }
    }
    
    public class UserSearchResponse : IHasResponseStatus
    {
        public IEnumerable<User> Results { get; set; }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ResponseStatus ResponseStatus { get; set; }
    }

My `UserSearch` object is the Request, and my response is `UserSearchResponse`

I discussed with [Demis](https://github.com/mythz), because I wanted to use the same response object for a few different services. But he's specifically designed SS this way, one of the reasons for the naming is:

If a use Requests a `UserSearch`, he can expect a `UserSearchResponse`. He doesn't need to know anything about the contracts or the service implementation, just that given a UserSearch request, he will get a `UserSearchResponse`.

It's actually a really good point and after thinking about it, I completely agree!

## Conclusion ##

This is only the automated error handling you get with SS, there's more that you can do, dive in and handle it yourself if you like. But in my opinion, the automated stuff covers a lot of scenarios and is very helpful.

For more information on Error handling visit the wiki here: <https://github.com/ServiceStack/ServiceStack/wiki/Validation>
