---
layout: post
title: Raygun.io - JavaScript error logging!
category: Raygun
---

Often when building applications we worry about error handling on the server, we may not do anything with the logs after we log those errors :P but we care about them enough to attempt to make the code resilient enough to not fall over should an error occur. 

But often we overlook errors happening on the client, and if a user hits one of those errors we don't know about it, unless they are kind enough to tell us. Should they tell us, we can't reproduce it, don't know where it's happening, etc etc.

Its really important that we give the end user a good user experience, and that means ensuring that the client code is functioning well! If an error occurs and prevents the code from being executed, we could lose a customer, and if this happens with one customer, it could be happening with many.

Enter Raygun

![](/images/raygun-javascript-1.png)

Raygun offers a [JavaScript API](https://github.com/MindscapeHQ/raygun4js) that's super easy to wire up to capture all errors by default, in-fact it's one line of code, not including the reference to the Raygun JS file.

### Download and Install
Unfortunately there's no NuGet for this yet, hopefully they will add one in the future. 

Head on over to the MindscapeHQ [Github Repository](https://github.com/MindscapeHQ/raygun4js/tree/master/dist) and download the file `raygun.min.js` (or grab the non-minified version if you wish), and reference in your app. 

	<script src="/scripts/raygun.min.js"></script>

### Configuring Raygun
Usually when you want to wire up an event to capture all errors you would need to do something like:

	window.onerror = function someError(errorMsg, url, lineNumber) {
	    //handle the error
	}

But there's a lot more to error handling on the client than just the `onerror` event. So rather than just giving you the ability to send the error, Raygun offers the ability to attach itself to the `onerror` event and capture as much necessary information as it can to give you the best possible information.

So to setup Raygun all we need to do is:

	<script>
	  Raygun.init('yourApiKey').attach();
	</script>

You can share the same API key for Client/Server, so if you're already using Raygun for your Web Application, you can use the same API key for the Client. 

Should you want to stop capturing errors you can call:

	Raygun.detach();

### Lets see it in action!

Lets start out by capturing unhandled exceptions/errors, in a new project we will create a new index.html file with the following:

	<script src="scripts/raygun.min.js"></script>
	<script>
	
		(function() {
		  Raygun.init("* my key *").attach();
		
		  var test = function() {
		    throw "Something went wrong!";
		  };
		
		  test();
		
		})();
	
	</script>

Running up the page we should get an error in the chrome console:

![](/images/raygun-javascript-2.png)

Now if we head on over to the Raygun.io Dashboard

![](/images/raygun-javascript-3.png)

We can see our error was logged. Lets do something a little different next, lets add a 2nd JavaScript file, and call a function inside that file:

In our `sample.js` file we have:

	function DoSomething(value) {
	
	    return Math.power(value, value);
	
	}

The method `power` doesn't exist on `Math`, it should be `pow`, so we should get an error when calling it. 

Now in our page we can include the `sample.js` file, and call the method:

	<script src="scripts/raygun.min.js"></script>
	<script src="scripts/sample.js"></script>
	<script>
	
		(function() {
			Raygun.init("* my key *").attach();
			
			var total = DoSomething(100);
			
			console.log(total);		
		})();
	
	</script>

![](/images/raygun-javascript-4.png)

As you can see we captured the same error in Chrome as we did on the Raygun dashboard.

Raygun also tries to capture as much information to help you sort out the issue as it can, such as the error, when it occurred, and where it occurred. 

![](/images/raygun-javascript-6.png)

It tells you the URL and Browser it occurred on:

![](/images/raygun-javascript-5.png)

It even tells you 