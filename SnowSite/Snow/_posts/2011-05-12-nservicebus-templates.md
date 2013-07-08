---
layout: post
title: NServiceBus Templates
category: NServiceBus
---

For anyone whose interested, the Visual Studio Gallery has some templates for NServiceBus.

<http://visualstudiogallery.msdn.microsoft.com/9546d382-7ffa-4fb8-8c0f-b7825d5fd085>

****

NServiceBus Templates includes project and item templates for those building solutions upon NServiceBus.

**Project Templates**    

- Client Endpoint - defines a project that performs as a client
- Server Endpoint - defines a project that performs as a server
- Publisher Endpoint - defines a project that performs as a publisher

**Item Templates**  

- Message Handler - default implementation of the IHandleMessages<T> interface
- EndpointConfig - default implementation of developer endpoint configuration

<!--excerpt-->

**Instructions**  

- Project templates include a custom wizard that will prompt for a directory. This directory should be the path to your NServiceBus assemblies and the generic host.  Select the directory and hit "Finish" to complete the project.

**Futures**  

- Distributor Endpoint
- Worker Endpoint
- Complete Solutions
    - Pub/Sub
    - Request/Response
    - Client/Server
    - Distributor/Worker



