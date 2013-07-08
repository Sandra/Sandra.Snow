---
layout: post
title: I love clean client IDs - especially with .Net 2.0!
category: ASP.NET
---

ASP.Net 4.0 got a brand spanking new feature. Clean ClientID's! ABOUT BLOODY TIME... Only, I'm working on a legacy application which can't be migrated to 4.0 :(

So why do we need Nice Readable, predictable ClientID's? I can think of two main reasons, outside of that, I would assume you're doing it wrong.

1. JavaScript, to find elements...
2. JavaScript in external files where you can't write spaghetti code.

What do I mean by spaghetti code? I mean this stuff:

    <%= txtUserName.ClientID %>
    <script type="text/javascript">
      var element = $('#<%= txtUserName.ClientID %>');
      var textbox = document.getElementById('<%= txtUserName.ClientID %>');
    </script>

I absolutely detest seeing this sort of thing. It makes me sick, and worst of all you can't pull this sort of code back to an external js file. In Web Forms, spaghetti coding is NOT your presentation, your server control is your presentation, the spaghetti code is your binding, and this should be in the codebehind.

<!--excerpt-->

Right, so what is my solution? Well I basically find all the server controls that are of a particular type, and write the information as a Json array to the HTML doc.

I've put the project on codeplex and named it, [Awesome.ClientID](http://awesomeclientid.codeplex.com/), because everything I do is awesome, atleast I like to think so :)

There's a few issues and the code is a little messy since I rushed it, but it works.

It serializes all the controls and you end up with a piece of JavaScript in the HTML Doc. Assume we had a form like:

    <fieldset>
      <ul>
        <li><label>Username: </label> <asp:textbox id="txtUserName" runat="server" /></li>
        <li><label>Email: </label> <asp:textbox id="txtEmail" runat="server" /></li>
      </ul>
      <asp:button id="btnSubmit" runat="server" text="Submit" />
    </fieldset>

This would get put into the HTML Doc like so:

    <script type="text/javascript">
    //<![CDATA[
      var controls = {
        "txtUserName": "ctl00_ContentPlaceHolder1_txtUserName",
        "txtEmail": "ctl00_ContentPlaceHolder1_txtEmail",
        "btnSubmit": "ctl00_ContentPlaceHolder1_btnSubmit"
      };
    //]]>
    </script>

Now when you write your JavaScript, instead of writing spaghetti code, you can write it like:

    <script type="text/javascript">
    //<![CDATA[
      var element = document.getElementById(controls.btnSubmit);
    //]]>
    </script>
    
Neat huh? That's not all tho, sometimes you need may need access to a property. For an example, at work we use a JavaScript charting library, and I need to pass in some widths so I can render the chart with the correct number of bars.

I have a property I set in my page with an attribute against it 'CanSerializeProperty', this will now get serialized just like the controls:

    [CanSerializeProperty]
    protected int ScheduleWidth
    {
      get; set;
    }
 
    <script type="text/javascript">
    //<![CDATA[
      var properties = { "ScheduleWidth": "50" };
    //]]>
    </script>

Originally intended for just .NET 2.0/3.5, the properties feature is pretty handy for .NET 4.0 also.

I've got some additional features I'm going to add, at the moment it serializes for every page, but I'm going to add an attribute for pages to turn the feature off. And possibly make a web.config section so you can specify the controls you want to serialize. That way you can easily add controls from libraries such as Telerik without needing to recompile the project.

<http://awesomeclientid.codeplex.com/>