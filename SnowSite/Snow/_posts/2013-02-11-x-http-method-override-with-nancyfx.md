---
layout: post
title: X-Http-Method-Override with NancyFX
category: NancyFX
---

Forms in HTML only allow you to use the method's POST and GET, I'm not sure if this would be classified as a limitation or not, but it does introduce a slight problem when you want to create a nice API taking advantage of verbs such as PUT and DELETE, when you can only POST and GET. 

This means if you want to delete something you have to use a different URL, like:

`/products/123/delete`

Then define a route like

    Post["/products/{id}/delete"] = _ =>
    {
        ... Do Something...
    };

This isn't exactly ideal, what we would prefer to have is a more semantic route.

* `GET` -> gets an object
* `DELETE` -> deletes an object
* `PUT` -> modifies an object
* `PATCH` -> modifies part of an object
* `POST` -> creates an object

<span class="note">**Note:** I'm no expert on this stuff so the VERBs above may not be correct wording.</span>

This would allow us to define the same route like so

    Delete["/products/{id}"] = _ =>
    {
        ... Do Something...
    };

<!--excerpt-->

Since HTML forms only allow GET/POST, the web framework needs to do a little bit of trickery in order to send a request to the correct route/verb. In this case we want to use a POST but have it go to a DELETE route.

MVC offers the ability to use the `HttpMethodOverride` helper to add a hidden input field to a form

    @Html.HttpMethodOverride(HttpVerbs.Delete)
    
This will generate a hidden field like so

    <input name="X-HTTP-Method-Override" type="hidden" value="DELETE" />

NancyFX also allows you to override the Method, but the field is actually named `_method`

    <input type="hidden" name="_method" value="DELETE"/>
    
<span class="note">**Note:** I'm not actually sure *why* the NancyFX team decided to name it `_method` rather than `X-HTTP-Method-Override`, though from reading it seems if you're posting a form it should be `_method`, but if you're sending it via headers it should be `X-HTTP-Method-Override` (turns out they followed Sinatra)</span>

Give a super basic scenario, I defined a module

    public ProductsModule() : base("products")
    {
        Get["/"] = _ => View["index", Products];

        Delete["/{id}"] = _ =>
        {
            Products.Remove(Products.Single(x => x.Id == (int)_.id));

            return View["index", Products];
        };
    }
    
Products is just a static list products. The view contains the following

    <ul>
      @Each.Model
      <li>
          <form action="/products/@Current.Id" method="POST">        
            <label>@Current.Name</label>
            <input type="submit" value="Delete"/>
          </form>   
      </li> 
      @EndEach
    </ul>
    
<span class="note">**Note:** This demo is using the Nancy Super Simple View Engine, not Razor :)</span>

This renders the page like

![](/images/nancy-method-override-1.png)

Clicking on the delete button

![](/images/nancy-method-override-2.png)

As you can see, it doesn't hit the Delete route, so if we update the View to contain the `_method` field

    <ul>
      @Each.Model
      <li>
          <form action="/products/@Current.Id" method="POST">
            <input type="hidden" name="_method" value="DELETE"/>
        
            <label>@Current.Name</label>
            <input type="submit" value="Delete"/>
          </form>   
      </li> 
      @EndEach
    </ul>
    
And now click the delete button again

![](/images/nancy-method-override-3.png)

We can see that it hits the correct route.

![](/images/nancy-method-override-4.png)

And the item (Product 4) has been removed from the list. So that's it, super simple to override the Method verb in NancyFX :)

<span class="note">**Note:** At the time of writing this, version 0.15.3 only supports `_method` on form posts, but the next version will support both `X-HTTP-Method-Override` and `_method` on both Headers and Forms.</span>