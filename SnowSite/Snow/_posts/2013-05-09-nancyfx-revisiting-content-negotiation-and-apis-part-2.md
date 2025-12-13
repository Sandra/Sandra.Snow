---
layout: post
title: NancyFX - Revisiting Content Negotiation & APIs (Part 2)
category: NancyFX
---

- Original Post: [NancyFX and Content Negotiation](/2012/11/nancy-and-content-negotiation)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 1)](/2013/04/nancyfx-revisiting-content-negotiation-and-apis-part-1/)
- NancyFX - Revisiting Content Negotiation & APIs (Part 2)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 3)](/2013/05/nancyfx-revisiting-content-negotiation-and-apis-part-3/)

In part 1 I went over a really basic scenario, and one of the fiddly things we had was the view name for returning a collection.

Using `Negotiate` gives as more flexibility on what we can return, allowing us to customize the response to respond differently to different media types or returning partial content in some scenarios.

Going back to my previous post I said we should be able to use the same API to build our website as we expose. But what about when we want to have additional information on the website that isn't pushed out to the client.

Lets say you have a product catalog, and you can view a particular product on your website. You also allow people to pull the content from your site to display on their website as like an affiliate system of some sort. But when you render your product you may have a special, but when you send the product to the consuming client you don't want to include the special for them since it's something specific to your website.

## Show me the codez!

So lets update the `Product` to include `SpecialPrice`
	
<!--excerpt-->

	public class Product
	{
	    public int Id { get; set; }
	    public string Name { get; set; }
	    public decimal Price { get; set; }
	    public decimal? SpecialPrice { get; set; }
	}

***Note:** and also update the repository to return the Special Price on each `Product`*

We're also going to introduce a new model called `PartialProduct`. There may be a better name for it, but its what I picked for the purpose of demonstrating. This is going to be the same as `Product` but without the `SpecialPrice` property.

	public class PartialProduct
	{
	    public PartialProduct()
	    {
	    }
	
	    public PartialProduct(Product product)
	    {
	        Id = product.Id;
	        Name = product.Name;
	        Price = product.Price;
	    }
	
	    public int Id { get; set; }
	    public string Name { get; set; }
	    public decimal Price { get; set; }
	}

The constructor is used to populate the model based on a `Product`. You may want to use something like AutoMapper to do this for you, or write some extension methods, etc.

Now with our route, we want to return the `PartialProduct` when responding to `application/json` or `application/xml`

	Get["/{id}"] = _ =>
	{
	    var product = productRepository.Get((int)_.id);
	
	    return Negotiate.WithView("product")
	                    .WithModel(product)
	                    .WithMediaRangeModel(MediaRange.FromString("application/json"),
	                                         new PartialProduct(product))
	                    .WithMediaRangeModel(MediaRange.FromString("application/xml"),
	                                         new PartialProduct(product));
	};

So we are saying that we want to negotiate the response with the view `product` using the model `product`, but if the MediaRange is `application/json` we want to return a partial product, likewise if its `application/xml` we want to also return a partial model. *(if it looks like a lot to type, don't worry we can tidy this up)*

Now when we setup our Postman looking for `text/html` we get the `SpecialPrice` returned on the product:

![](/images/nancyfx-conneg-updated-part2-1.png)

However if we update it to `application/json` we get:

![](/images/nancyfx-conneg-updated-part2-2.png)

Anddddd `application/xml` we get:

![](/images/nancyfx-conneg-updated-part2-3.png)

But... We kinda had to write a lot of code for the media ranges. Extension methods are awesome though, so we can tidy them up by introducing our own extension methods:

	public static class NegotiateExtensions
	{
	    public static Negotiator ForJson(this Negotiator negotiator, object model)
	    {
	        return negotiator.WithMediaRangeModel(MediaRange.FromString("application/json"), model);
	    }
	
	    public static Negotiator ForXml(this Negotiator negotiator, object model)
	    {
	        return negotiator.WithMediaRangeModel(MediaRange.FromString("application/xml"), model);
	    }
	}	

This reduces things down to:

	Get["/{id}"] = _ =>
	{
	    var product = productRepository.Get((int)_.id);
	
	    return Negotiate.WithView("product")
	                    .WithModel(product)
	                    .ForJson(new PartialProduct(product))
	                    .ForXml(new PartialProduct(product));
	};

Still too much? We can introduce one more extension

	public static Negotiator OrPartial(this Negotiator negotiator, object model)
	{
	    return negotiator.ForJson(model).ForXml(model);
	}

And now all we have is
	
	Get["/{id}"] = _ =>
	{
	    var product = productRepository.Get((int)_.id);
	
	    return Negotiate.WithView("product")
	                    .WithModel(product)
	                    .OrPartial(new PartialProduct(product));
	};

Obviously you can play around to figure out what suits you. But this gives us the flexibility to customize outputs for different media types. 

In part 3 I'll discuss implementing your own Media Type :)