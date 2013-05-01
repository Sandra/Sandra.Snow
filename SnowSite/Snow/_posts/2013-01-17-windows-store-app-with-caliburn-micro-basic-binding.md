---
layout: post
title: Windows Store App with Caliburn.Micro - Basic Databinding
category: Windows 8 App
---

Continuing on with the SampleProject for a Windows Store App, we will look at binding, this is where we will finally make use of the ViewModel. 

In the previous example we output the text 'Hello World' to the screen, but rather than put it on the screen directly we can load it when we load the view.

First we need to create a ViewModel, Caliburn works on a naming convention so if your view is named `MainView`, your view model needs to be naned `MainViewModel`.

![](/images/windows-store-binding-1.png)

In our class we want to inherit from `Screen`, this is a Caliburn class that is used to represent a single screen, your application may have many screens but for now we are only dealing with a single screen. 

    using Caliburn.Micro;

    namespace SampleProject.ViewModels
    {
        public class MainViewModel : Screen
        {
        }
    }

<!--excerpt-->

Screen doesn't require us to implement anything, but it does handle a few things for us, it implements `INotifyPropertyChangedEx`, which inherits from `INotifyPropertyChanged`, which is great because this is exactly what we need!

Next we need to add a new property, but this can't be an autoproperty, we need a backing field so we can invoke `NotifyOfPropertyChange`

    private string _message;
    public string Message
    {
        get { return _message; }
        set
        {
            if (value == _message) 
                return;
            _message = value;
            NotifyOfPropertyChange();
        }
    }

This is basically what Resharper generates for us, if we create an autoproperty, we can convert it to a property with backing field that calls `NotifyOfPropertyChange`

![](/images/windows-store-binding-2.png)

Now we need to set our message, we are going to do this when the ViewModel is initialized. 

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Message = "Saying: Hello World! :)";
    }

Last of all we need to wire up the `TextBlock` control to the property, there's a few different ways you can do this, but generally you want to use the `{Binding *property*}` syntax. 

We can update our button to add the binding to the Text Field.

    <TextBlock Text="{Binding Message}" FontSize="28"/>

Now when we run our app we get the message written in the OnInitialize.

![](/images/windows-store-binding-3.png)

If we want to update the text, when we set the `Message` property, the `NotifyOfPropertyChange` will be fired and the screen will update with the new text. To show you I've added a button to the screen which invokes a method on the ViewModel.

    private void UpdateText()
    {
        Message = "This text was updated";
    }

<span class="note">**Note:** Don't worry about how this button event is wired up, I'll get to that in future posts.</span>

When we view the screen we get the default text.

![](/images/windows-store-binding-4.png)

And when we hit the button, the text is updated.

![](/images/windows-store-binding-5.png)

Bindings can be added to pretty much any property you want, for example if we updated the button to change the visibility of the text we can add a new property to the ViewModel.

    private Visibility _textBlockVisibility;
    public Visibility TextBlockVisibility
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
    
Then update the button to alternate the visibility.
    
    private void UpdateText()
    {
        var isVisible = TextBlockVisibility == Visibility.Visible;

        TextBlockVisibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
    }
    
And finally add the binding to the TextBlock.

    <TextBlock FontSize="28" Text="{Binding Message}" Visibility="{Binding TextBlockVisibility}" />
    
Now when we view the page we get the default message.

![](/images/windows-store-binding-4.png)

If we hit the button, the message disappears...
    
![](/images/windows-store-binding-6.png)
    
Next I hope to look into a collection of data.