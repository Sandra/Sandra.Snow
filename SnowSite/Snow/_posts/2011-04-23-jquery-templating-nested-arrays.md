---
layout: post
title: jQuery Templating - Nested Collections
category: jQuery
---

Was asked me a couple of weeks ago if it was possible to have nested collections in a jQuery template. Well you can! The sample code kind of buries the ability since it tries to demonstrate too many things at once. But it's rather simple.

    {{"{{each *array*"}}}}
        *stuff*
    {{"{{/each"}}}}
    
So assuming we have a simple json object like...

    var people = [
        {
            firstName: "James",
            favouriteColours: [ 
                { colorName: "green" },
                { colorName: "red" }
            ]
        },
        {
            firstName: "Jack",
            favouriteColours: [
                { colorName: "blue" },
                { colorName: "red" }
            ]
        },
        {
            firstName: "Jim",
            favouriteColours: [
                { colorName: "blue" },
                { colorName: "black" },
                { colorName: "white" }
            ]
        }
    ];
    
We can iterate over the people and then their favourite colours by using `each` in the template like so:

<!--excerpt-->

    <script id="sample" type="text/x-jquery-tmpl">
        Name: ${firstName} <br />
        Colours: 
            {{"{{each favouriteColours"}}}}
                ${this.colorName}
            {{"{{/each"}}}}
        <hr />
    </script>

You're not limited to 1 level of collections, if you had many levels you can have an `each` inside an `each`... inside an `each`. But that would be getting silly.
