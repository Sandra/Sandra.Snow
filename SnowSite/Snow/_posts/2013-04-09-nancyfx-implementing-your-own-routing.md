---
layout: post
title: NancyFX - Implementing your own routing!
category: NancyFX
---

With the up and coming release of 0.17 of NancyFX, the routing has been completely rewritten, and now it's super easy to implement your own routing. So I'm going to show you how.


## How it works

The routing works by defining a route:

`Get["/products/{id}"]`

The route is then broken up into segments:

1. `products`
2. `{id}`

Each segment is checked against a Node Condition in the `TrieNodeFactory` like so

	if (segment.StartsWith("(") && segment.EndsWith(")"))
	{
	    return new RegExNode(parent, segment, this);
	}

When a request comes in, the segment is compared to the node for a match and returns true/false + the captured parameter. 

These are a bunch of conditions for checking different nodes, currently Nancy supports out of the box the following nodes.

## Existing Nodes
This is brief description of the existing nodes that currently exist in 0.17. 

### CaptureNode
This node captures `{foo}`, or basically any value defined in the segment.

<!--excerpt-->

### CaptureNodeWithDefaultValue
Similar to `CaptureNode`, `{foo?defaultValue}`, allows you to capture any value, with a default value should the value not exist.

### GreedyCaptureNode
This is like the be-all-end-all node `{greedy*}`. It will capture anything in the current segment and onward. Although other segments are still checked. I would think it's rare to ever need this node.

### LiteralNode
If nothing else is captured in any other node, then likely hood it's a literal value, this will just capture the segment as is, non-capturing, its just a match or non-match. 

### OptionalCaptureNode
This is like the `CaptureNode` and `CaptureNodeWithDefaultValue`, but it just makes the segment as optional. It can or cannot exist, if it exists it's captured.

### RegExNode
Wooo Regular Expression support! `(?<foo>\d{2,4})` is a named capture that will find an numeric value between 2-4 digits long, in the segment.

### GreedyRegExCaptureNode
The `GreedyRegExCaptureNode` is a little more complicated, it's a mix between the `RegExNode` and `GreedyCaptureNode`, and supports any number of segments in a regular expression match. i.e `^(?:(?<id>videos/\d{1,10})(?:/{0,1}(?<slug>.*)))$` will match `videos/123` or `videos/123/some-slug-url`.

### RootNode
This node simply dictates that this is the very start of the route segments, the very root. Effectively its `/`

## Implementing your own Node

So we're going to create a route constraint. Our node will look like `[foo:even]`, we're saying that the segment will be captured, only if the value is an even number, anything else and it wont match. 

So we create a class and inherit `TrieNode`

	public class CapturedOddEvenNode : TrieNode
	{
	    public CapturedOddEvenNode(TrieNode parent, string segment, ITrieNodeFactory nodeFactory) 
	        : base(parent, segment, nodeFactory)
	    {
	    }
	
	    public override SegmentMatch Match(string segment)
	    {
	        throw new NotImplementedException();
	    }
	
	    public override int Score
	    {
	        get { throw new NotImplementedException(); }
	    }
	}

The first thing we want to do is prepare the segment, so in the ctor we are going to trim the `[]` values off the start/end of the segment and, then split the remaining value by `:` as the name on the left, and the condition on the right.

	private string segmentName;
	private bool shouldBeEven;
	
	public CapturedOddEvenNode(TrieNode parent, string segment, ITrieNodeFactory nodeFactory)
	    : base(parent, segment, nodeFactory)
	{
	    this.ExtractParameterName();
	}
	
	private void ExtractParameterName()
	{
	    var innerSegment = this.RouteDefinitionSegment.Trim('[', ']');
	    var segmentSplit = innerSegment.Split(':');
	
	    this.segmentName = segmentSplit[0];
	    this.shouldBeEven = segmentSplit[1] == "even";
	}

Next we need to implement the match. Here we want to check the value and condition:

1. The value is not a number = no match
2. The value is an odd number and the condition is it should be odd = match
3. The value is an even number and the condition is it should be even = match
4. Else the condition is not met so its a no match

<!---->

	public override SegmentMatch Match(string segment)
	{
	    int numericValue;
	
	    if (!int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
	    {
	        return SegmentMatch.NoMatch;
	    }
	
	    if ((numericValue%2 == 0 && shouldBeEven) ||
	        (numericValue%2 != 0 && !shouldBeEven))
	    {
	        var match = new SegmentMatch(true);
	        match.CapturedParameters.Add(segmentName, numericValue);
	             
	        return match;
	    }
	
	    return SegmentMatch.NoMatch;
	}

Lastly we need to implement the Score. The score is used in the scenario when two routes have two matches, the summed total of the score for all segments becomes the weight deciding which route wins. Highest score of the two or more matches wins. 

We will set it to 100.

	public override int Score
	{
	    get { return 100; }
	}

Our final Node looks like so:

	public class CapturedOddEvenNode : TrieNode
	{
	    private string segmentName;
	    private bool shouldBeEven;
	
	    public CapturedOddEvenNode(TrieNode parent, string segment, ITrieNodeFactory nodeFactory)
	        : base(parent, segment, nodeFactory)
	    {
	        this.ExtractParameterName();
	    }
	        
	    private void ExtractParameterName()
	    {
	        var innerSegment = this.RouteDefinitionSegment.Trim('[', ']');
	        var segmentSplit = innerSegment.Split(':');
	
	        this.segmentName = segmentSplit[0];
	        this.shouldBeEven = segmentSplit[1] == "even";
	    }
	        
	    public override SegmentMatch Match(string segment)
	    {
	        int numericValue;
	
	        if (!int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
	        {
	            return SegmentMatch.NoMatch;
	        }
	
	        if ((numericValue%2 == 0 && shouldBeEven) ||
	            (numericValue%2 != 0 && !shouldBeEven))
	        {
	            var match = new SegmentMatch(true);
	            match.CapturedParameters.Add(segmentName, numericValue);
	             
	            return match;
	        }
	
	        return SegmentMatch.NoMatch;
	    }
	
	    public override int Score
	    {
	        get { return 100; }
	    }
	}

## Implementing your own factory

You're required to implement your own factory to call the new Node, this is super easy because we can just inherit the existing one.

Create a new Factory called `CustomTrieNodeFactory` and implement `TrieNodeFactory`

	public class CustomTrieNodeFactory : TrieNodeFactory
	{
	    public override TrieNode GetNodeForSegment(TrieNode parent, string segment)
	    {
	        if (parent == null)
	        {
	            return new RootNode(this);
	        }
	
	        if (segment.StartsWith("[") && segment.EndsWith("]") && segment.Contains(":"))
	        {
	            return new CapturedOddEvenNode(parent, segment, this);
	        }
	
	        return base.GetNodeForSegment(parent, segment);
	    }
	}

You can see that the first condition is that I check that the parent is null, that's because the first segment is always the root node, all segments after that are out implementation. The condition is checked in the base call, but we want to check before we run out code.

	if (parent == null)
	{
	   return new RootNode(this);
	}

So by checking its null, we return a RootNode, then the call comes in the second time, it will have a parent node, and then check to see if the segment starts/ends with our criteria.

## Wiring up the new factory

Lastly we need to wire up the factory in the bootstrapper, we can do this by overriding the `NancyInternalConfiguration` property and overriding the `TrieNodeFactory` property with our custom type like so:

	protected override NancyInternalConfiguration InternalConfiguration
	{
	    get
	    {
	        return NancyInternalConfiguration.WithOverrides(config => 
			{
				config.TrieNodeFactory = typeof (CustomTrieNodeFactory);
			});
	    }
	}

Bam, that's it! 

## Does it work?

So if we create a new Module called `TestModule` and implement two routes:
	
	Get["/test/[oddNumbersRawr:odd]"] = _ =>
	{
	    return "I love odd numbers! Like: " + _.oddNumbersRawr;
	};

Running up the project and entering a with an odd number

![](/images/nancyfx-new-routing-1.png)

Pretty sweet!

But if we enter the URL with an even number

![](/images/nancyfx-new-routing-2.png)

BAM page not found, because it didn't match the condition!

That's really all there is to it. You can create any type of custom routing you like. 