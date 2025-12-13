---
layout: post
title: Multiple IISExpress Sites profiled with DotTrace
---

Yesterday (14th of June) I had a need to run more than one website while running [JetBrain's](http://www.jetbrains.com/) [DotTrace](http://www.jetbrains.com/profiler/), to give a bit of background, Website A needs to redirect to Website B in order to simulate a single sign on.

Website B can't be accessed without using the SSO, so in order to profile Website B I need to run Website A.

The problem is when using DotTrace, I have to select the website I wish to profile:

![](/images/multiple-iis-1.png)

The problem with that, is that when you run the profiler it only starts up one of the websites. I need it to start both.

This would be easy to profile if the Standard Version came with Attach To Process feature. But unfortunately that only comes with Professional.

That way, when I debug using Visual Studio I could pick the process to profile:

![](/images/multiple-iis-2.png)

*(Information can be found by right clicking the system try and selecting 'show all applications')*

<!--excerpt-->

However that isn't an option.

I had a few idea's of trying to get it to work such as having multiple start-up projects, in VS and using the 'Profile Startup Project' feature.

## The Solution ##

The solution was to run IISExpress with the parameters to startup the sites. The problem with this approach is that you cannot specify an array of website's to start.

IISExpress gives you the following options when you run /?

**/config:config-file**  
The full path to the applicationhost.config file. The default value is the IISExpressconfigapplicationhost.config file that is located in the user"s Documents folder.

**/site:site-name**  
The name of the site to launch, as described in the applicationhost.config file.

**/siteid:site-id **  
The ID of the site to launch, as described in the applicationhost.config file.

**/path:app-path **  
The full physical path of the application to run. You cannot combine this option with the /config and related options.

**/port:port-number **  
The port to which the application will bind. The default value is 8080. You must also specify the /path option.

**/clr:clr-version **  
The .NET Framework version (e.g. v2.0) to use to run the application. The default value is v4.0. You must also specify the /path option.

**/systray:true|false **  
Enables or disables the system tray application. The default value is true.

**/trace:trace-level **  
Valid values are "none", "n", "info", "i", "warning", "w", "error", and "e". The default value is none.

If we look at the applicationhost.config file for the site entries we get:

    <sites>
      <site name="TestProjectOne" id="1">
        <application path="/" applicationPool="Clr4IntegratedAppPool">
          <virtualDirectory path="/" physicalPath="C:\Users\phillip\Documents\Visual Studio 2010\Projects\TestProjectOne\TestProjectOne" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:7946:localhost" />
        </bindings>
      </site>
      <site name="TestProjectTwo" id="2">
        <application path="/" applicationPool="Clr4IntegratedAppPool">
          <virtualDirectory path="/" physicalPath="C:\Users\phillip\Documents\Visual Studio 2010\Projects\TestProjectOne\TestProjectTwo" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:8921:localhost" />
        </bindings>
      </site>
      <siteDefaults>
        <logFile logFormat="W3C" directory="%IIS_USER_HOME%\Logs" />
        <traceFailedRequestsLogging directory="%IIS_USER_HOME%\TraceLogFiles" enabled="true" maxLogFileSizeKB="1024" />
      </siteDefaults>
      <applicationDefaults applicationPool="Clr4IntegratedAppPool" />
      <virtualDirectoryDefaults allowSubDirConfig="true" />
    </sites>
    
As you can see there's nothing special in the website's. Except for one thing!

This attribute:

    applicationPool="Clr4IntegratedAppPool"
    
What IISExpress doesn't tell you is that you can start an application pool.

Running the command `/AppPool:Clr4IntegratedAppPool` gives us:

    C:\Program Files (x86)\IIS Express>iisexpress.exe /AppPool:Clr4IntegratedAppPool  
      
    Starting IIS Express ...   
    Successfully registered URL "http://localhost:7946/" for site "TestProjectOne" application "/"   
    Successfully registered URL "http://localhost:8921/" for site "TestProjectTwo" application "/"   
    Registration completed   
    IIS Express is running.   
    Enter "Q" to stop IIS Express 

To get this working with DotTrace, we just need to select > Profile Application

![](/images/multiple-iis-3.png)

Running this will run IISExpress and all the website's under the same process id:

[![](/images/multiple-iis-4.png)](/images/multiple-iis-4.png)

(click to enlarge)

Bam, not we're now able to run multiple sites at once, and even profile them all at once!

## The Catch ##

There is one gotcha with this approach, if you work on multiple sites, you end up running those up as well:

    C:\Program Files (x86)\IIS Express>iisexpress.exe /AppPool:Clr4IntegratedAppPool  
      
    Starting IIS Express ...   
    Successfully registered URL "http://localhost:7946/" for site "TestProjectOne" application "/"   
    Successfully registered URL "http://localhost:8921/" for site "TestProjectTwo" application "/"   
    Successfully registered URL "http://localhost:16207/" for site "JabbR" application "/"   
    Registration completed   
    IIS Express is running.   
    Enter "Q" to stop IIS Express

This can be fixed easily, to get around this, simply open up your applicationhost.config file located in:

    C:Users\\*user\*\DocumentsIIS\Expressconfig\applicationhost.config

Locate the application pools:

    <applicationPools>
      <add name="Clr4IntegratedAppPool" managedRuntimeVersion="v4.0" managedPipelineMode="Integrated" CLRConfigFile="%IIS_USER_HOME%\config\aspnet.config" autoStart="true" />
      <add name="Clr4ClassicAppPool" managedRuntimeVersion="v4.0" managedPipelineMode="Classic" CLRConfigFile="%IIS_USER_HOME%\config\aspnet.config" autoStart="true" />
      <add name="Clr2IntegratedAppPool" managedRuntimeVersion="v2.0" managedPipelineMode="Integrated" CLRConfigFile="%IIS_USER_HOME%\config\aspnet.config" autoStart="true" />
      <add name="Clr2ClassicAppPool" managedRuntimeVersion="v2.0" managedPipelineMode="Classic" CLRConfigFile="%IIS_USER_HOME%\config\aspnet.config" autoStart="true" />
      <add name="UnmanagedClassicAppPool" managedRuntimeVersion="" managedPipelineMode="Classic" autoStart="true" />
      <applicationPoolDefaults managedRuntimeLoader="v4.0">
        <processModel />
      </applicationPoolDefaults>
    </applicationPools>
    
Add a new entry:

    <add name="TestProjectAppPool" managedRuntimeVersion="v4.0" managedPipelineMode="Integrated" CLRConfigFile="%IIS_USER_HOME%\config\aspnet.config" autoStart="true" />

And update your website's to use this new application pool:

    <site name="TestProjectOne" id="1">
      <application path="/" applicationPool="TestProjectAppPool">
        <virtualDirectory path="/" physicalPath="C:\Users\phillip\Documents\Visual Studio 2010\Projects\TestProjectOne\TestProjectOne" />
      </application>
      <bindings>
        <binding protocol="http" bindingInformation="*:7946:localhost" />
      </bindings>
    </site>
    <site name="TestProjectTwo" id="2">
      <application path="/" applicationPool="TestProjectAppPool">
        <virtualDirectory path="/" physicalPath="C:\Users\phillip\Documents\Visual Studio 2010\Projects\TestProjectOne\TestProjectTwo" />
      </application>
      <bindings>
        <binding protocol="http" bindingInformation="*:8921:localhost" />
      </bindings>
    </site>
    <site name="JabbR" id="3">
      <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="D:\Development\phillip-haydon\JabbR\JabbR" />
      </application>
      <bindings>
        <binding protocol="http" bindingInformation="*:16207:localhost" />
      </bindings>
    </site>
    
Now we can run our new application pool:

    C:\Program Files (x86)\IIS Express>iisexpress.exe /AppPool:TestProjectAppPool  
      
    Starting IIS Express ...  
    Successfully registered URL "http://localhost:7946/" for site "TestProjectOne" application "/"  
    Successfully registered URL "http://localhost:8921/" for site "TestProjectTwo" application "/"  
    Registration completed  
    IIS Express is running.  
    Enter "Q" to stop IIS Express  

And now when we profile we only get the two websites we want, running.

Even though DotTrace Standard Edition doesn't allow you to attach to process, you can easily debug across multiple sites, without the need of upgrading to the Pro edition.

It would be really cool if JetBrains could add a new option to the IIS Expression Application profile screen, so that we can select the application pool to run if we want to start up multiple websites:

![](/images/multiple-iis-5.png)

That would be awesome :) and I think it would be really easy for them to implement!