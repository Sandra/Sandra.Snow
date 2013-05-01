---
layout: post
title: NancyFX and Content Negotiation
category: NancyFX
---

This has to be one of the most awesome features of Nancy, Content Negotiation. Recently added in 0.12, it gives you the ability to implement a single route that responds with different versions of the same document, without having to mess up your code with duplicate methods or conditional statements.

When doing this in ASP.NET MVC I would have to check the content type and decide how I want to respond to the request.

This ended up making duplicate methods, one which would be used by a normal GET request, while the 2nd would be for an AJAX request. Or if it was similar, use conditional logic in the single method to decide how the action should respond...

Nancy on the other hand supports Content Negotiation out of the box.

    Get["/negotiated"] = parameters => {
        return Negotiate
            .WithModel(new RatPack {FirstName = "Nancy "})
            .WithMediaRangeModel("text/html", new RatPack {FirstName = "Nancy fancy pants"})
            .WithView("negotiatedview")
            .WithHeader("X-Custom", "SomeValue");
    };

<span class="note">**Note:** Sample taken from Nancy GitHub Repo</span>

## What is content negotiation? ##

In short, it's the ability to serve different versions of a document to the same URI.

<!--excerpt-->

To read more you can visit [Wikipedia](http://en.wikipedia.org/wiki/Content_negotiation) or [SOA Patterns](http://www.soapatterns.org/content_negotiation.php)... or... [Google](http://www.bing.com/)

## Why should I care? ##

Well lets assume we're building a website and we have a shopping cart, we get to the checkout page and there's a button to delete the item from your cart.

You want to give a really nice user experience by not posting back the entire page. You would rather just tell the server to delete the item from the cart, and do a quick update on the screen, and avoid the whole page reloading.

However what if the page is taking a long time to load, and as a result, the JavaScript hasn't been executed and wired up all the events to the buttons to delete items from your cart, you still want to be able to post the entire page and maintain the usability of the page.

The same scenario occurs if the user has (unlikely) turned JavaScript off.

## Implementation ##

So using the example above, deleting an item from a cart and updating the page, using JavaScript verses a full postback of the page, using the exact same route.

Let's create a really basic module:

    public class HomeModule : NancyModule
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }
        public static IList<Product> Products = new List<Product>()
        {
            new Product {Id = 1, Name = "Surface", Price = 499},
            new Product {Id = 2, Name = "iPad", Price = 899},
            new Product {Id = 3, Name = "Nexus 10", Price = 599},
            new Product {Id = 4, Name = "Think Pad", Price = 499},
            new Product {Id = 5, Name = "Yoga", Price = 699},
        };

        public dynamic Model = new ExpandoObject();

        public HomeModule()
        {
            Model.Deleted = false;

            Get["/"] = _ =>
            {
                Model.Products = Products;

                return View["index", Model];
            };
        }
    }

I've nested a Product class in there, and created a static list of products for demo.

We need a View to go with this to display the Products:

    <h2>Products</h2>
    <p>Posted back using: <span class="status">@if ((bool)Model.Deleted) { @Html.Raw("Full Postback"); }</span></p>

    <table>
      <thead>
        <tr>
          <th style="width: 50px;">Id</th>
          <th style="width: 90px;">Name</th>
          <th style="width: 50px;">Price</th>
          <th style="width: 150px;">&nbsp;</th>
          <th style="width: 160px;">&nbsp;</th>
        </tr>
      </thead>

      @foreach (var product in Model.Products)
      {
        <tr>
          <td>@product.Id</td>
          <td>@product.Name</td>
          <td>@product.Price.ToString("c")</td>
          <td><a href="/delete/@product.Id">Delete With JavaScript</a></td>
          <td><a href="/delete/@product.Id">Delete Without JavaScript</a></td>
        </tr>  
      }
    </table>

This rendered will display the following:

<img src="/images/nancy-conneg-1.png" />

So when we press the 'Delete Without JavaScript' button we want it to remove the item, so we can add a new route:

    Get[@"/delete/{id}"] = _ =>
    {
        var id      = (int) _.id;
        var item    = Products.Single(x => x.Id == id);
                    
        Products.Remove(item);
        Model.Products = Products;
        Model.Deleted = true;

        return View["index", Model];
    };

Now if we press the button:

<img src="/images/nancy-conneg-2.png" />

We can see the URL has updated, and the 3rd item was deleted from the list. It also updated the text to day it used a full postpack. We can see that a full postback has occurred since the URL changed.

Now we want to make the 'Delete With JavaScript' button work.

So we can add some JavaScript:

    <script src="/Scripts/jquery-1.8.2.min.js"></script>
    <script>
      (function ($) {
        $(document).on("click", 'a:contains(Delete With JavaScript)', function (e) {
          
          e.preventDefault();
            
          var that = $(this),
              tr = that.closest('tr');

          $.ajax({
            url: this.href,
            type: 'GET',
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8'
          }).success(function (data) {
            if (data.Deleted === true) {
              tr.remove();
              $('.status').text("Using JavaScript");
            }
          });
            
        });

      })(jQuery);
    </script>

So this just looks for an anchor tag with the text 'Delete With JavaScript' since we don't want to stop the other buttons from working.

Now we need to update the Route to handle content negotiation.

    Get[@"/delete/{id}"] = _ =>
    {
        var id      = (int) _.id;
        var item    = Products.Single(x => x.Id == id);
                    
        Products.Remove(item);
        Model.Products = Products;
        Model.Deleted = true;

        return Negotiate
            .WithModel((object) Model)
            .WithMediaRangeModel("application/json", new
            {
                Model.Deleted
            })
            .WithView("index");
    };

The implementation is identical to before, the only difference is we replaced 'View' with 'Negotiate'.

<span class="note">**Note:** The Model being returned is Dynamic, this allows me to just add the properties to it without having to define a static class. As a result I have to cast the Model to an object when passing it into the method 'WithModel', if I was using a static class I wouldn't need to do this.</span>

If the type if different, in this case the request was for application/json, then we pass back only the data we need, rather than everything.

Now if we run the same page again but we click on 'Delete With JavaScript'

<img src="/images/nancy-conneg-3.png" />

This time when we delete item 4, the URL hasn't changed, but it's removed the item and said it was done using the JavaScript.

Now when we click either link, both scenarios are handled by the one route.

## Also... ##

This isn't limited to just this use case, if you're building an API and you want the route to respond with XML or JSON, or maybe even your own content type, this is an awesome feature that you can use to handle these scenarios so your consumers can get what they want, in the format they want, with no effort from you.

Content Negotiation - Is Awesome.

The demo for this project can be found [here on github](https://github.com/phillip-haydon/NancyConnegDemo).
