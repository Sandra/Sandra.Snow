---
layout: post
title: NancyFX - Revisiting Content Negotiation & APIs (Part 3)
category: NancyFX
---

- Original Post: [NancyFX and Content Negotiation](/2012/11/nancy-and-content-negotiation)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 1)](/2013/04/nancyfx-revisiting-content-negotiation-and-apis-part-1/)
- [NancyFX - Revisiting Content Negotiation & APIs (Part 2)](/2013/05/nancyfx-revisiting-content-negotiation-and-apis-part-2/)
- NancyFX - Revisiting Content Negotiation & APIs (Part 3)

In this last part I want to show you creating your own media type, so far we have gone over a really basic example of returning a response and letting the accept header handle different results, and then we went over how explicitly using `Negotiate` allows you to customize your response more.

Creating your own Media Type may be something you never have to do, but it can help you solve some strange scenarios, for instance you could have a user request an invoice, rather than clutter your code with:

	if (requestedInvoice.Type == "pdf")
	{
		// get a pdf invoice
	}
	else if (requestedInvoice.Type == "word")
	{
	
	}
	else if (...)

You get the picture, this is tedious problematic work, every time we want to supply a new invoice type we need to modify the existing request, what we want to do is just handle a new media type and process it if we know about it.

<!--excerpt-->

<span class="note">**Note:** To keep this simple I'm going to show a simple CSV Processor, you can look at NancyFX code on how a ViewProcessor works if you want to hook up a View to a Word/PDF processor.</span>

## Codez is what I need!

There's 3 things we need to create

 1. CsvProcessor
 2. CsvSerializer
 3. CsvResponse

The `CsvProcessor` takes the request, checks to see if it can handle the request, and if it can, it uses the `CsvSerializer` to process the request and return a `CsvResponse`

## Csv Response

This class is modeled off the Json Response, it takes a model and a serializer, sets all the correct headers for the response, serializes the response, and this is what is returned to the client.

	public class CsvResponse<TModel> : Response
	{
	    public CsvResponse(TModel model, ISerializer serializer)
	    {
	        if (serializer == null)
	        {
	            throw new InvalidOperationException("CSV Serializer not set");
	        }
	
	        this.Contents = GetJsonContents(model, serializer);
	        this.ContentType = "text/csv";
	        this.StatusCode = HttpStatusCode.OK;
	    }
	
	    private static Action<Stream> GetJsonContents(TModel model, ISerializer serializer)
	    {
	        return stream => serializer.Serialize("text/csv", model, stream);
	    }
	}
	
	public class CsvResponse : CsvResponse<object>
	{
	    public CsvResponse(object model, ISerializer serializer)
	        : base(model, serializer)
	    {
	    }
	}

You don't REALLY need this class, I would say its a highly recommended optional. If you don't create this class it just means your processor needs to do all the work of setting up the response.

## Csv Serializer

To handle serialization of the model to a CSV file I'm using `ServiceStack.Text` which comes with its own serializer! 

    public class CsvSerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            return IsCsvType(contentType);
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "csv"; }
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new StreamWriter(new UnclosableStreamWrapper(outputStream)))
            {
                ServiceStack.Text.CsvSerializer.SerializeToWriter(model, writer);
            }
        }

        private static bool IsCsvType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            var contentMimeType = contentType.Split(';')[0];

            return contentMimeType.Equals("text/csv", StringComparison.InvariantCultureIgnoreCase) ||
                   contentMimeType.StartsWith("text/csv", StringComparison.InvariantCultureIgnoreCase);
        }
    }

The serializer needs to implement `ISerializer`, NancyFX will automatically pick up and register this class for you, so all you need to do is create it.

The `CanSerialize` method is very important, in the `CsvProcessor` we will invoke all serializers until we find one that can handle serializing the content for the requested processor.

## Csv Processor

Lastly we have the processor.

    public class CsvProcessor : IResponseProcessor
    {
        private readonly ISerializer serializer;

        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("csv", MediaRange.FromString("text/csv")) };

        public CsvProcessor(IEnumerable<ISerializer> serializers)
        {
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("text/csv"));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return extensionMappings; }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches("text/csv"))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new CsvResponse(model, this.serializer);
        }
    }

This class has 3 methods and a constructor.

The `Constructor` simply works out which serializer it requires.

`ExtensionMapping` specifies which extension can be used on a request to call the processor. i.e if you can't pass in accepts headers, you can end the url with `.csv` and the processor will be invoked!

`CanProcess` checks to see if the request can be processed.

Lastly `Process` simply processes the request.

## Lets see it in action!

Using the EXACT same routes we defined on the previous posts, we can simply modify the header to ask for `text/csv`

![](/images/nancyfx-conneg-updated-part3-1.png)

And we get all our data serialized to CSV format! We didn't have to modify the bootstrapper or anything, Nancy just sees the implemented interfaces and says MINE! :D

## What about the Extension mentioned earlier!

In the processor we had the following

    private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
        new[] { new Tuple<string, MediaRange>("csv", MediaRange.FromString("text/csv")) };

    public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
    {
        get { return extensionMappings; }
    }

Well when you access a url with `.json` or `.csv`, it will response with that as a file!

If we have the following page

![](/images/nancyfx-conneg-updated-part3-2.png)

We add the extension `.json` to the end

![](/images/nancyfx-conneg-updated-part3-3.png)

Nancy Replies! (The JSON format shown done using [JSONView chrome extension](https://chrome.google.com/webstore/detail/jsonview/chklaanhfefbnpoihckbnefhakgolnmc?hl=en)

If we update to `.csv`

![](/images/nancyfx-conneg-updated-part3-4.png)

And if we open the file up

![](/images/nancyfx-conneg-updated-part3-5.png)

That's all there is to it!

Hope this helps anyone with Conneg or Conneg with NancyFX :)