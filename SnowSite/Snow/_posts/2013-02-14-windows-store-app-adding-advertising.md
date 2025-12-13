---
layout: post
title: Windows Store App - Adding Advertising
category: Windows 8 App
---

I searched for hours trying to figure this out and I think the information is a little scarce or not obvious.

Basically I wanted to add some advertising to my app, I'm trying to cover multiple different area's in order to learn what's involved, and I thought I had it all down, only to work out that the advert's that were displaying were test adverts and not the real thing.

Now don't get me wrong, when you actually do find the correct URL (by searching Advertising rather than Ad) the MSDN link does have all the information, I didn't find this out until my friend actually sent it to me...

<http://msdn.microsoft.com/en-us/library/advertising-windows-sdk(v=msads.10).aspx>

There's two main things you need to do (besides installing the correct SDK, I had the Beta first time around)

  1. Implement the advert into your app
  2. Setup an advertising account
  
The first part is easy... (well the 2nd part is too... but I didn't realise that at first)

<!--excerpt-->

## Implementing your advert

Head on over to MSDN to download the [Microsoft Advertising SDK](http://go.microsoft.com/?linkid=9815330) and install it.

After installing, open up your solution and add a reference to the `Microsoft Advertising SDK for Windows 8 (Xaml)` assembly. 

![](/images/win-app-ad-1.png)

Now you can use the Tools to drag/drop the advert control to your Xaml file. 

    <ui:AdControl Name="Advert"
                  Grid.Row="1"
                  Width="728"
                  Height="90"
                  Margin="30,441,0,0"
                  HorizontalAlignment="Left"
                  VerticalAlignment="Top"
                  AdUnitId="10042998"
                  ApplicationId="d25517cb-12d4-4699-8bdc-52040c712cab" />
                  
The two things that are important are the `AdUnitId` and `ApplicationId`, the ApplicationId should get populated automatically using the test value. (the value shown in the code above) if not, you can manually assign it.

To test your advert you need to add a `AdUnitId` specific to the dimensions you want to show, and for the type of advert you want to test.

<table>
  <tbody>
    <tr>
      <th>
        pubCenter Ad Unit Size (Width x Height)
      </th>
      <th>
        Experience
      </th>
      <th>
        AdUnitId
      </th>
    </tr>
    <tr>
      <td>
        160 x 600
      </td>
      <td>
        Windows 8 Image Ad with click to Full Screen Image
      </td>
      <td>
        10043136
      </td>
    </tr>
    <tr>
      <td>
        160 x 600
      </td>
      <td>
        Windows 8 Video Ad with click to Full Screen Video
      </td>
      <td>
        10043135
      </td>
      </tr>
    <tr>
      <td>
        160 x 600
      </td>
      <td>
        Windows 8 Image Ad
      </td>
      <td>
        10043134
      </td>
    </tr>
  </tbody>
</table>

You can find a full list of test modes on [MSDN - Test Mode Values](http://msdn.microsoft.com/en-us/library/advertising-windows-test-mode-values(v=msads.10).aspx)

Find the Height/Width you want, ensure your Ad control is set to the same Height/Width, and assign the corresponding `AdUnitId` to the control.

Now when you run your app you should get something like

![](/images/win-app-ad-2.png)

The next thing you need to do is add some error handling. To do this you just add the event to the control directly like `ErrorOccurred="AdvertErrorOccurred"`

    void AdvertErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
    {
        Advert.Visibility = Visibility.Collapsed;
    }
    
You may want to do something more, maybe try loading a different sized advert. I've chosen to just hide the control instead.

## Setup an advertising account

Head on over to <https://pubcenter.microsoft.com/> and login with your Windows Live account (or signup)

First you will need to head on over to the Accounts

![](/images/win-app-ad-3.png)

Once you have created an account (you don't need to add a payout right away, you can do that in the future when you've generated some revenue), you need to head on over to Setup.

![](/images/win-app-ad-4.png)

You need to Register your application, and then you need to create an AdUnit. This stuff is really straight forward so should be easy peasy.

Once you have registered your app (which gives you the `ApplicationId`) and created an AdUnit (which gives you the `AdUnitId`) you can replace the values in your app, and publish your app with advertising in it :D

Although this information is available on the net, I initially had trouble finding it, so hopefully this helps anyone else out with confusion.