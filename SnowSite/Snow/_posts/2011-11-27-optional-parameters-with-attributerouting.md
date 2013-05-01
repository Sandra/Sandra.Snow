---
layout: post
title: Optional Parameters with AttributeRouting
category: ASP.NET MVC
---

I found a little trick with using Optional Parameters with [AttributeRouting](https://github.com/mccalltd/AttributeRouting/wiki/2.-Usage), by using standard optional parameters in the action.

The documentation says you can add an attributes to specify the defaults, or add `=value` to the parameter name, and I guess that's a more correct way to generate routes, but you can achieve the same affect by making the parameter optional. Like so:

    [GET("videos/{?page}")]
    public ActionResult Videos(int page = 1)
    {
        return View("Result");
    }

If I browse to the URL:

![](/images/attribute-routing-1.png)

It uses the default value of 1.

![](/images/attribute-routing-2.png)

Now when appending a number to the end of the URL:

<!--excerpt-->

![](/images/attribute-routing-3.png)

It captures the correct value:

![](/images/attribute-routing-4.png)

I prefer this method, the optional parameter on the action is more quickly identified, than looking at the attribute to see the defaults. (in my opinion)

Ahh AttributeRouting, how I love you.

