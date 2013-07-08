---
layout: post
title: jQuery Serialize a Fieldset
category: jQuery
---

I'm really not a fan of [ASP.Net Ajax](http://ajax.asp.net/) and would prefer to use jQuery where ever I can. So for anything public facing I tend to use jQuery and HTTPHandlers.

jQuery has this nifty little function called [serialize](http://docs.jquery.com/Ajax/serialize) for serializing form data for Ajax. So if we have the following HTML.

    <body>
      <form id="aspNetForm" runat="server">
        <div>
          <fieldset class="fsLoginForm">
            <legend>Login Fieldset</legend>
            <ul>
              <li>Email: <input type="text" name="txtEmail" /></li>            
              <li>Password:   <input type="text" name="txtPassword" /></li>        
            </ul>                
          </fieldset>

          <fieldset class="fsName">        
            <legend>Name Fieldset</legend>        
            <ul>            
              <li>My Name: <input type="text" name="email" /></li>        
            </ul>    
          </fieldset>

          <fieldset class="fsColour">        
            <legend>Colour Fieldset</legend>        
            <ul>            
              <li>Colour: <input type="text" name="colour" /></li>        
            </ul>    
          </fieldset>

          <input type="button" value="Submit Form" onclick="SubmitForm();" />
        </div>    
      </form>
    </body>
    
And a basic alert method that shows us the result of the form serialized:

    function SubmitForm() {    
      alert($('form').serialize());
    }

<!--excerpt-->

We end up with a result like:

*image missing*

This is great, however we may not want to submit the entire form... and unfortunately when using ASP.Net Web Forms, your limited to one form element.

jQuery only allows you to serialize a form element, so I came up with a little work around for submitting only the elements you want. The example is done using fieldsets, however you could group them using div's, tables, or what ever you want.

Basically I add a new form, and clone the fieldset into the form,then serialize the new form.

So I've added some new buttons for this example:

    <input type="button" value="Submit Login"
           onclick="SubmitFieldset('.fsLoginForm');" />
    <input type="button" value="Submit Name"
           onclick="SubmitFieldset('.fsName');" />
    <input type="button" value="Submit Name &amp; Colour"   
           onclick="SubmitFieldset('.fsName, .fsColour');" />
           
And the JavaScript:

    function SubmitFieldset(fieldsetName) {    
      //Add a new form and hide it.    
      $('body').append('<form id="form-to-submit" style="visibility:hidden;"></form>');

      //Clone the fieldset into the new form.    
      $('#form-to-submit').html($(fieldsetName).clone());

      //Serialize the data    
      var data = $('#form-to-submit').serialize();

      //Remove the form    
      $('#form-to-submit').remove();

      //Alert to see the data, this is where you would do your ajaxy stuff :)
      alert(data);
    }
    
Now when I press 'Submit Login' the result I get is:

*image missing* 

I only get the elements in the login fieldset. If I want to submit more than one fieldset I can specify them all in the selector by using the specifying multiple arguments in the selector. <http://docs.jquery.com/Selectors/multiple#selector1selector2selectorN>

So by passing in `.fsName, .fsColour` I end up with:

*image missing* 

Now I can submit only the data I need to submit, and not all of it :)