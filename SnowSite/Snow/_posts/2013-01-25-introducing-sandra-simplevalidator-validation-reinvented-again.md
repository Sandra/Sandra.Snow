---
layout: post
title: Introducing Sandra.SimpleValidation - Validation reinvented... again
---

Yup, I decided to reinvent the wheel, don't hate on me :)

So I grew a little fustrated, theres quite a few serverside validation libraries around, but I feel they do too much, are hard to test, yada yada yada...

So I came up with Sandra.SimpleValidation

The idea is that it's dead simple, it requires 0 up front configuration, it doesn't allow you to inject stuff into the validators, validators are newed up once and only once, there's no client-side validation.

I don't believe validation should allow you to do things like inject a repository and query the database, those begin to become Business Rules and should be handled seperately. All it does is validate a model you give it. 

So what do you need to do?

> PM> Install-Package Sandra.SimpleValidator

First up, install the package. It requires 4.0 or above, it could probably work on 3.5 but who uses that anymore?

<!--excerpt-->

Once installed, you can create a validator

    public class UserValidator : ValidateThis<User>
    {
        public UserValidator()
        {
            For(x => x.Username)
                .Ensure(new Required());

            For(x => x.Email)
                .Ensure(new Required())
                .Ensure(new Email());

            For(x => x.Locale)
                .Ensure(new Required());
        }
    }

If you don't like the default messages you can change those by chaining the rule like so

    public class UserValidator : ValidateThis<User>
    {
        public UserValidator()
        {
            For(x => x.Username)
                .Ensure(new Required().WithMessage("Username is required");
                
            ...
        }
    }
    
Now you can inject (or create an instance of) the `ValidationService`.

Using Nancy with the default `TinyIoC` container, you can just include this in the module constructor. If you're using MVC you may need to register it depending on the container you're using. In any case it can be a singleton since it only needs to be created once.

    public class HomeModule : NancyModule
    {
        public HomeModule(ValidationService validate)
        {
            Get["/"] = _ =>
            {
                var user = this.Bind<User>();
                var validationResult = validate.This(user);

                if (validationResult.IsInvalid)
                {
                    //Handle errors with
                    //validationResult.Messages

                    return "Validation has fails :(";
                }

                return "Validation was successful :)";
            };
        }
    }
    
Once you've injected the service, you simply call `This(...)` passing in the object you want to validate. With the result you can then call `IsValid` or `IsInvalid` and handle the errors.

What's nice about the validator class is that because it's so simple, it's simple to test. Given the example above, we can write a unit test like so.

    [Fact]
    public void Given_Valid_Model_Should_Return_IsValid_As_True()
    {
        var validator = new UserValidator();
        var model = new User
        {
            Username = "HelloWorld",
            Email = "test@banana.com",
            Locale = "en-AU"
        };

        var result = validator.Validate(model);

        Assert.True(result.IsValid);
    }
    
That's all there is to it. Currently as of writing this it only supports `Required`, `MinimumLength`, `MaximumLength`, `Between`, and `Email` rules. Since that's all I require for my current project. If you want to create your own simply implement `IRule` and new them up in the `Ensure` method of the validators.

In the future I hope to get Nancy intergration so the validation can be invoked on `this.Bind<T>()`, but for now you can find the github repo, and nuget

<https://github.com/phillip-haydon/Sandra.SimpleValidator>
<https://www.nuget.org/packages/Sandra.SimpleValidator>