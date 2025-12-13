---
layout: post
title: Post a collection of ViewModel's to a MVC Action with jQuery
category: jQuery
---

Maybe I searched for the wrong thing, but I couldn't find what I was looking for :( My Bing and Google fu failed me.

Basically I wanted to post a collection of ViewModels to an MCV action. Turns out it's rather simple.

Lets say I have a bunch of Products, and Products are managed in a WarehouseLocation. A product doesn't have a warehouse location, since it could exist in multiple locations.

If I'm currently working in Location A, I want to post a collection of Products to an action, as well as the WarehouseLocationId.

So given a simple ViewModel, and an Action:


    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

and

<!--excerpt-->

    public JsonResult Update(int warehouseLocationId, IEnumerable<ProductViewModel> products)
    {
        //Do something with the products...
        return Json(new {Staus = "success"});
    }

I think usually when someone sends data from jQuery it's usually a single parameter,so the JSON would look something like:

    var data = { id: 1,name: 'test name', price: 15.95 };

This would populate an action that looked like:

    public JsonResult Update(ProductViewModel product)

But what I was faced with was passing in two parameters, one of which is a collection...

MVC seems to pair up the posted result with the parameter name, in the same way they it does for Route parameters. So to the JSON needs to look like:

    var data = {
        warehouseLocationId: 12,
        products: [{
            Id: 1,
            Name: "Product 1",
            Price: 15.95
        }, {
            Id: 3,
            Name: "Product 2",
            Price: 12.50
        }]
    };

As you can see the key's on the first level match the parameter names, while the array on products, match the ViewModel.

Taking this exact data and posting it to the action:

    $.ajax({
        type: 'POST',
        url: '@Url.Action("Update", "Home")',
        data: JSON.stringify(data),
        contentType: 'application/json',
        success: function(result) {
            //Do something with result...
        },
        dataType: 'json'
    });
 

I submit that, and with a breakpoint on my action I see:

![](/images/jquery-mvc-1.png)

^ The warehouseLocationId...

![](/images/jquery-mvc-2.png)

^ 2 products with the values:

![](/images/jquery-mvc-3.png)

And:

![](/images/jquery-mvc-4.png)

The exact same data we defined in our JavaScript.

The really cool thing about this, is you can have nested collections also. Using the same scenario, but extending Product to have a collection of Categories like so:

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

I can update the JSON object to include Category information on 1 of the two products:

    var data = {
        warehouseLocationId: 12,
        products: [{
            Id: 1,
            Name: "Product 1",
            Price: 15.95
        }, {
            Id: 3,
            Name: "Product 2",
            Price: 12.50,
            Categories: [{
                Id: 1,
                Name: "Category 1"
            }, {
                Id: 1,
                Name: "Category 2"
            }]
        }]
    };
    
And submit that in the same way as before, capturing the results in the Action we get the first product with null for the categories, since we didn't define it as an empty array:

![](/images/jquery-mvc-5.png)

While the second Product has 2 items, the first item is Category 1, and the second item is Category 2.

![](/images/jquery-mvc-6.png)

![](/images/jquery-mvc-7.png)

And that's it, easy peasy, sending a collection of ViewModels from jQuery to an MVC Action.

I <3 MVC :)

