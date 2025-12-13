---
layout: post
title: Windows Store App with Caliburn.Micro - Binding Converters
category: Windows 8 App
---

In my last post about [Basic Binding](http://www.philliphaydon.com/2013/01/windows-store-app-with-caliburn-micro-basic-binding) we added the ability to Hide/Show a control using Viability property of the control to make it `Visible` or `Collapsed`

However the View Model shouldn't have any real knowledge of how the view works, meaning it shouldn't actually dictate the visibility using the controls properties.

<span class="note">**Note:** I left this out of the original post because I wanted to keep it 'Basic' without confusing binding with converters and such, making the topic more complicated</span>

If in the future you changed from say `TextBlock` to a 3rd party control called `BananaTextBlock` and that 3rd party decided that they were not going to use the built in Viability enum, and instead decided to create their own naming convention, and enum etc. You would be forced to change your ViewModel, which isn't good.

That's where Value Converters come in handy. 

<!--excerpt-->

## Creating your own Converter

Value Converters are easy to implement, simply create a new class and inherit the `IValueConverter` interface. There are two methods you need to implement, the `Convert`, this is where you convert your object to the target type. 

i.e if your View Model uses a `Boolean`, you would accept the `Boolean` type and convert it to a `Visibility` type. 

The second method is ConvertBack, where it takes the control type and converts it back to the ViewModel type. 

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
                throw new ArgumentException("value is not type of boolean");        
        
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility))
                throw new ArgumentException("value is not type of Visibility");
                
            return (Visibility)value == Visibility.Visible;
        }
    }
    
## Registering your Value Converter

You can do this in a couple of different places, it's similar to registering any other Resource.

- Page.Resources
- Application.Resources
- separate Resource file. 

I'm only going to show it at Page.Resources

In the root Page node of your view, add the namespace to your converts

    xmlns:sp="using:SampleProject.Converters"

In your View, add a element called `Page.Resources` and then using the alias for the namespace just added `sp` reference the converter we just created.

    <Page.Resources>
        <sp:BooleanToVisibilityConverter x:Key="TrueConveter" />
    </Page.Resources>
    
The `Key` is the name we will use when referencing the converter.

## Updating the ViewModel from Visibility to Boolean

The original post used the Visibility enum, but we need to update this to be a boolean

    public bool TextBlockVisibility
    {
        get { return _textBlockVisibility; }
        set
        {
            if (value == _textBlockVisibility)
                return;

            _textBlockVisibility = value;

            NotifyOfPropertyChange();
        }
    }

The button we used to change the value is also updated

    private void UpdateText()
    {
        TextBlockVisibility = !TextBlockVisibility;
    }

This will just invert the existing value each time it's clicked, like before.

## Using the converter

Now we need to update the `TextBlock` from the original post

    <TextBlock FontSize="28" 
               Text="{Binding Message}" 
               Visibility="{Binding TextBlockVisibility}" />
    
To use the converter, in the binding we simply want to add a comma at the end and specify a Converter

    <TextBlock FontSize="28"
               Text="{Binding Message}"
               Visibility="{Binding TextBlockVisibility,
                            Converter={StaticResource TrueConveter}}" />
                                    
Now when we run the app, and click the button, it should hide/show, and our ViewModel now no longer needs to know how to change the visibility of the control. 
