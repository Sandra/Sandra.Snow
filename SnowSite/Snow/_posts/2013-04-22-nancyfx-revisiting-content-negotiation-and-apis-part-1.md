---
layout: post
title: NancyFX - Revisiting Content Negotiation & APIs (Part 1)
category: NancyFX
---

- Original Post: [NancyFX and Content Negotiation](/2012/11/nancy-and-content-negotiation)
- NancyFX - Revisiting Content Negotiation & APIs (Part 1)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 2)](/2013/05/nancyfx-revisiting-content-negotiation-and-apis-part-2/)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 3)](/2013/05/nancyfx-revisiting-content-negotiation-and-apis-part-3/)

I thought I would revisit this topic since I don't believe I did it enough justice last time around, and I believe it really is important when creating an API that is going to be consumed not only by the public or client, but **by you also**!

When the browser asks for `text/html` its negotiating with the server. So really your website is an API, your `Views` are just an additional type of content that your API serves up when requested.


## Example
Lets say you're building Twitter, the initial page shows a list of tweets, so the browser makes a call to `/tweets` and the server responds with a list of tweets rendered with using HTML.

Once the page has loaded, the client uses JavaScript to load new tweets, so it calls `/tweets` again, this time it returns a `json` result, and the client-side templating engine then renders and appends those to the top of the existing list, keeping the client up to date with the latest tweets.

What's nice is no new data needed to be written on the server! 

<!--excerpt-->

<span class="note">**Note:** I'm not going to discuss RESTful API's, that would just create too many arguments.</span>

## Need code please
Right so lets see this in action for real. When I demonstrated this in my previous post, I showed a somewhat complicated Negotiate, this time I'm going to show a super simple scenario.

We have a `ProductsModule`, it can `Get` a single product, or it can `Get` a list of products.

	public class ProductsModule : NancyModule
	{
	    public ProductsModule(IProductRepository productRepository)
	        :base("products")
	    {
	        Get["/{id}"] = _ =>
	        {
	            var product = productRepository.Get((int)_.id);
	
	            return product;
	        };
	
	        Get["/"] = _ =>
	        {
	            var products = productRepository.List();
	
	            return products;
	        };
	    }
	}

Using a Chrome Plugin called Postman which can be found on the [Chrome Web Store](https://chrome.google.com/webstore/detail/postman-rest-client/fdmmgilgnpjigdojojpjoooidkmcomcm?utm_source=chrome-ntp-launcher). 

![](/images/nancyfx-conneg-updated-1.png)

We can invoke some calls to get some data. We specify the `URL`, the `VERB`, and add some Headers. In this case we will add a single header. `Accept` where we specify that we want `application/xml`. 

![](/images/nancyfx-conneg-updated-2.png)

When we invoke this, we get an XML result of our products:

![](/images/nancyfx-conneg-updated-3.png)

When we do the same call but we specify the `Accept` header with `application/json` we get a JSON result. 

![](/images/nancyfx-conneg-updated-4.png)

Nice right? So far we haven't written any code other than returning some data to a endpoint we specified. Lets take a look at the first route, its a Get By Id route, and all we're returning is a single Product.

![](/images/nancyfx-conneg-updated-5.png)

Now if we specify the Accept as something the browser would ask for, `text/html` we would expect rendered HTML.

![](/images/nancyfx-conneg-updated-6.png)

Bam we get an error, but this is only because the view doesn't exist yet. By default NancyFX looks for a view of the same name of the type returned.

Since we returned a 'product', the view name that Nancy will look for is `typeof(Product).Name` or `Product`. So if we create a new product view to display some data:

	<!DOCTYPE html>
	<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
	<head>
	    <meta charset="utf-8" />
	    <title></title>
	</head>
	<body>
	
	    <div>
	        Id: @Model.Id
	        <br />
	        Name: @Model.Name
	        <br />
	        Price: $@Model.Price
	    </div>
	
	</body>
	</html>

Super super simple, now when we call the same route again we get:

![](/images/nancyfx-conneg-updated-7.png)

Now we have a Single API that returns JSON, XML, or HTML. 

## Fiddly Issues
Here's where things get fiddly, if we want to return the collection with a view, NancyFX will attempt to convert the `List<Product>` to it's name, and look for the view. This ends up looking for a view by the name of ``List`1``, which you may think will fail, but funnily enough you can actually create a view with the backquote.

So lets create a new file called ``List`1.sshtml`` and populate it with some basic HTML:

	<!DOCTYPE html>	
	<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
	<head>
	    <meta charset="utf-8" />
	    <title></title>
	</head>
	<body>
	
	    @Each
	    <div>
	        Id: @Current.Id
	        <br />
	        Name: @Current.Name
	        <br />
	        Price: $@Current.Price
	    </div>
		<hr>
	    @EndEach
	
	</body>
	</html>

<span class="note">**Note:** in case you're wondering, this isn't Razor, this is Nancy's Super Simple View Engine :)</span>

Now if we run up the site again and hit `/products` we get:

![](/images/nancyfx-conneg-updated-8.png)

## Recap
So far we have created a single endpoint with very little code that can respond with JSON/XML/HTML. Next I'm going to show how `Negotiate` gives you more flexibility.