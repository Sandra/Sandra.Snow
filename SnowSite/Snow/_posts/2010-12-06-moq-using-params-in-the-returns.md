---
layout: post
title: Moq-Using Params in the Returns
category: Unit Testing
---

Had an interesting scenario to solve today, while I was away a bunch of unit tests got turned off since they broke during some refactoring, I spent the day fixing theses unit tests.

The method under test had a dependency on another class, it performed two actions on this class.

1. to get a list of data
2. to return a DateRange (this is done in a foreach loop)
 
DateRange is just a class with a start/end property of DateTime. The method is sort of like this (obviously with proper names I just wrote some random code to illustrate the scenario)

    public IEnumerable<Stuff> GetStuff(DateTime dateList)
    {
        var result = new List<Stuff>();
        var someService = IoC.Resolve<ISomeService>();
        var data = someService.GetData();
        
        foreach (var date in dateList)
        {
            var tempDate = date;
            DateRange range = someService.GetDates(OpenRules, tempDate);
            //do stuff...
        }
        
        return result;
    }

So the same service is called twice, the issue is when iterating over the dateList, it needs to be filtered based on a DateRange during the day. So say, 8am till 10pm. Removing the stuff outside of that time period during that day.

<!--excerpt-->

The method GetDates would take some rules and based on the date, create the the DateRange for the filter.

To test this method we needed to get some data, and filter it,and check the list had the correct data after being filtered.

We mocked ISomeService,using Moq:

    var mockService = new Mock<ISomeService>();
    mockService.Setup(x => x.GetData()).Returns(FakeData());
    IoC.Register<ISomeService>(mockService.Object);
    
The issue is now in the foreach, I couldn't just mock a return because it would be the same value and I wouldn't be testing the actual foreach. I ended up writing something along the lines of:

    mockService.Setup(x => x.GetDates(It.IsAny<OpenCloseRule>(), It.IsAny<DateTime>()))
               .Returns(
                   (OpenCloseRule openRules, DateTime, startDate) =>
                   //Some logic
                   return new DateRange(start, end);
               );
              
This worked great, except, if the implementation of GetDates changes, would result in false positives or false negatives, or something.

So I ended up changing it to:

    mockService.Setup(x => x.GetDates(It.IsAny<OpenCloseRule>(), It.IsAny<DateTime>()))
               .Returns(
                   (OpenCloseRule openRules, DateTime, startDate) =>
                   return (new SomeService()).GetDates(openRules, startDate);
               );
            
That way if the implantation changes, the test will break or, work. Personally I don't like this cos I'm testing two things instead of one. But at least I can test the original method works correctly.

So what I actually learnt was that the returns method on the mock can actually use the parameters that were passed in. That's pretty awesome. Bed time, it's late.
