---
layout: post
title: Unit of Work with WCF and Autofac
---

I've just spent the last few days trying to find a way to have a Unit of Work with WCF, but it seems no one has a nice clean easy solution.

The first, and the only decent solution I found was here:

<http://ianfnelson.com/archives/2010/04/09/wcf-nhibernate-unit-of-work-endpoint-behavior>

The problem I found with this solution is that the ICallContextInitializer as well as the EndpointBehavior is only created once. So it would seem all calls to a service would share the same Unit of Work instance.

Maybe Castle Windsor does something fancy and injects a brand new `EndpointBehavior` every request to a service, but for me, it seemed the `ServiceBehavior`, `EndpointBehavior`, and `ICallContextInitializer` were all created once.

This caused my service to resolve a different instance of `IUnitOfWork` to what was in the `ICallContextInitializer`.

## Interim Solution 1

The first solution I came up with was to use Autofac to call Commit on release:

    builder.RegisterType(typeof (UnitOfWork))
            .As(typeof (IUnitOfWork))
            .InstancePerLifetimeScope()
            .OnRelease(x =>
                            {
                                ((IUnitOfWork) x).Commit();
                            });

<!--excerpt-->
                            
It works...  but it seemed like a real hack, so I kept digging.

I posted on [StackOverflow](http://stackoverflow.com/questions/7989918/using-a-custom-endpoint-behavior-with-wcf-and-autofac) & [Autofac Google Group](http://groups.google.com/group/autofac/browse_thread/thread/7310498aea634abd), but so far I haven't had anyone suggest a good solution.

## Solution 2

The next took a while to come up with.

<span class="note">**Note:** I don't know if this is an appropriate solution for utilizing a Unit of Work with WCF. It works for me but if there is a better solution I would like to hear it.</span>

I started by giving all my classes an interface called IService.

    public interface IService
    {
        IUnitOfWork UnitOfWork { get; set; }
    }

My Service implements this interface:

    public class WasteInventoryQueryService : IWasteInventoryQueryService, IService
    {
        public IUnitOfWork UnitOfWork { get; set; }
        public IWasteStockRepository WasteStockRepository { get; set; }
        public WasteInventoryQueryService(IUnitOfWork unitOfWork, IWasteStockRepository wasteStockRepository)
        {
            UnitOfWork = unitOfWork;
            WasteStockRepository = wasteStockRepository;
        }

        ...
    }

Next I created some behaviours similar to the linked article.

Since finding that the ServiceBehavior, EndpointBehavior and ICallContextInitializer all are created once, I started at the ServiceBehavior.

I created a class called EndpointResolverServiceBehavior, it's purpose is to inject all the endpoint behaviours I create.

    public class EndpointResolverServiceBehavior : IServiceBehavior
    {
        protected IEnumerable<IEndpointBehavior> EndpointBehaviors { get; set; }
        public EndpointResolverServiceBehavior(IEnumerable<IEndpointBehavior> endpointBehaviors)
        {
            EndpointBehaviors = endpointBehaviors;
        }

        #region Implementation of IServiceBehavior

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, 
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var endpoint in serviceDescription.Endpoints)
            {
                foreach (var endpointBehavior in EndpointBehaviors)
                {
                    endpoint.Behaviors.Add(endpointBehavior);
                }
            }
        }

        #endregion
    }

Next I created an EndpointBehavior called UnitOfWorkEndpointBehavior, it's purpose is to add the ICallContextInitializer instance.

    public class UnitOfWorkEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (DispatchOperation operation in endpointDispatcher.DispatchRuntime.Operations)
            {
                operation.CallContextInitializers.Add(new UnitOfWorkCallContextInitializer());
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

And then is the ICallContextInitializer.

    public class UnitOfWorkCallContextInitializer : ICallContextInitializer
    {
        private PropertyInfo _userObjectInfo;
        private readonly BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance;
        public object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message)
        {
            if (_userObjectInfo == null)
                _userObjectInfo = instanceContext.GetType()
                                                    .GetProperty("UserObject", _flags);
                
            return _userObjectInfo.GetValue(instanceContext, null) as IService;
        }

        public void AfterInvoke(object correlationState)
        {
            var uow = correlationState as IService;

            if (uow != null)
                uow.UnitOfWork.Commit();
        }
    }

While debugging I found out that the InstanceContext has a private property which has the current service attached to it:

![](/images/uow-autofac-1.png)

![](/images/uow-autofac-2.png)

(click the image for a larger view)

So I reflected the property and casted it to an IService, and return the result. When the service has been invoked, the result passed out of the 'BeforeInvoke' method, is passed into the 'AfterInvoke'.

![](/images/uow-autofac-3.png)

So basically I return the result from the BeforeInvoke which is passed into the AfterInvoke. Then I attempt to cast it to an IService again.

If the cast is successful, then I can call Commit on my UnitOfWork.

The last piece is to wire up the Behavior with Autofac.

    builder.RegisterType(typeof(UnitOfWorkEndpointBehavior)).As(typeof(IEndpointBehavior));
    builder.RegisterType(typeof(EndpointResolverServiceBehavior)).As(typeof(IServiceBehavior));
    
    Container = builder.Build();

    AutofacHostFactory.Container = Container;
    AutofacHostFactory.HostConfigurationAction =
        host =>
        {
            host.Description.Behaviors.Add(Container.Resolve<IServiceBehavior>());
        };

And it's done. I have a working Unit of Work, that is injected into my Service and Committed after the service has been invoked.

The only down-side I see to all of this, is that if an exception is thrown that I don't capture, then the UoW will still be committed regardless.

If anyone has any better solutions, let me know! :)
