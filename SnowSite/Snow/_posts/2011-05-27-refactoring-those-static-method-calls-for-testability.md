---
layout: post
title: Refactoring those static method calls for testability!
category: Unit Testing
---

One thing I love about working on a legacy application is the weird things I get to problem solve.

A lot of the old system has an awful lot of static method calls, which doesn't leave much to be desired for when it comes to unit testing. I was working on a piece of the system today which is written in .NET 2.0, and is tightly coupled to everything in existence.

I didn't want to go introduce interfaces and wrapper classes in order to abstract out all the dependencies, but I wanted to unit test the work without having to touch the database, which is what these static method calls were doing.

The code I dealt with was something along the lines of...

    public class ProductService
    {
        public static void Save(Product product)
        {
            throw new Exception("This would normally touch a db...");
        }
    }
    
*(Example is completely made up for this blog post and isn't actual code from work)*

<!--excerpt-->

The class that calls this is along the lines of...

    public class SomeService
    {
        public void DoSomething(User user, Product product)
        {
            //Do a bunch of stuff...
            if (user.IsVip)
                product.Price *= 0.90;
            ProductService.Save(product);

            //Do some more stuff...
        }
    }

So there's a direct call to the static method 'Save', now unit testing this class, like...

    [TestMethod]
    public void Test()
    {
        //Arrange
        SomeService serviceUnderTest = new SomeService();
        User user = new User();
        user.IsVip = true;

        Product product = new Product();
        product.Price = 5.00d;

        //Act
        serviceUnderTest.DoSomething(user, product);

        //Assert
        Assert.AreEqual(4.5d, product.Price);
    }

Running the test I would like to expect a result of '4.5' for the price, but what I get is the exception thrown from the ProductService.

The solution, that didn't involve interfaces, a wrapper around the service, etc.

    public class SomeService
    {
        internal delegate void ProductServiceDelegate (Product product);
        private ProductServiceDelegate _productServiceDelegate;
        internal ProductServiceDelegate ProductServiceSave
        {
            get
            {
                if (_productServiceDelegate == null)
                    _productServiceDelegate = ProductService.Save;

                return _productServiceDelegate;
            }   
            set { _productServiceDelegate = value; }
        }

        public void DoSomething(User user, Product product)
        {
            //Do a bunch of stuff...
            if (user.IsVip)
                product.Price *= 0.90;

            ProductServiceSave(product);

            //Do some more stuff...
        }
    }

So what I've done is introduce a delegate for the ProductService's 'Save' method, and made it internal with 'InternalsVisibleTo' on the AssemblyInfo file so that unit testing project can see it.

Now I can update the unit test with:

    [TestMethod]
    public void TestMethod1()
    {
        SomeService serviceUnderTest = new SomeService();
        
        User user = new User();
        user.IsVip = true;

        Product product = new Product();
        product.Price = 5.00d;

        serviceUnderTest.ProductServiceSave =
            new SomeService.ProductServiceDelegate(
                delegate(Product productParam)
                    {
                        productParam.Id = 1;
                    });

        serviceUnderTest.DoSomething(user, product);

        Assert.AreEqual(4.5d, product.Price);
        Assert.AreEqual(1, product.Id);
    }

So on the class under test I add a test method to the property which just sets the Id of the product to '1', to simulate that the product was saved, and now I don't have to worry about the dependency on the ProductService class anymore.

