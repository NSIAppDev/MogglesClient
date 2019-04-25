# MogglesClient

Client code for the [Moggles](https://github.com/NSIAppDev/Moggles) project.  
Released as a Nuget Package.


Retrieves feature toggles from source and caches them in the application at a configurable period of time. The different features the package offers are discussed in the next sections.


## Features

* The feature toggles are retrieved using an API provided by each application it is installed in. Even though it was originally created for [Moggles](https://github.com/NSIAppDev/Moggles), the API is configurable.
  * A timeout period for the call can be configured, the default value is 30s.
* The feature toggles are retrieved based on the **Application** and **Environment**.
* The feature toggles are saved in the application cache and are refreshed hourly. The period of time in which the feature toggles are refreshed is configurable.
  * Feature toggles are saved in two cache entries, an expiring one and a persistent one. 
  * If the call that retrieves the toggles fails, the persistent cache will hold the previous feature toggles values that are going to be used and the call will be retried every 3 minutes until successful.
____________________________________
  :heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup.*
  
  The following features are enabled by default, but they can be disabled by adding a key in the application configuration file.
  
  The **message bus url**, **user** and **password** will need to be provided.
  
* **Force cache refresh** 

  * If the impact of a toggle needs to be immediate, a force cache event can be handled by the client.  
  * The queue name for this event will need to be provided.  
  * The consumer implemented in the MogglesClient will read the message from the queue and based on the **Application** and **Environment** it will refresh the corresponding application.  
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles).
  
* **Show deployed feature toggles**
  
    At application start, the client will search all assemblies for feature toggles and will publish a message containing the feature toggles found in the application. [Moggles](https://github.com/NSIAppDev/Moggles) will read the message and update the deployed status of the feature toggles.  
    * A list of assemblies that can be ignored in the search can be provided.
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles).
_______________________________________

* **Logging**  

  Different information is logged in [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview):  
  * An instrumentation key will need to be provided in order for this feature to be available.

    Exceptions | Events
    --- | --- 
    The API call fails | When the cache entries are successfully refreshed 
    When the FTs are not available (both cache entries are empty) | When a force cache refresh event was handled 
    When detecting the deployed feature toggles and one of the assemblies it searches in could not be loaded | 
    
## Installation

## How to use?

The configuration keys for MogglesClient will need to be provided in the application configuration file.

* **.NET Framework**
```C#
  <appSettings>
    <!-- MogglesClient configuration keys-->

    <!--REQUIRED KEYS-->
    <add key="Moggles.ApplicationName" value="MogglesExampleApp" />
    <add key="Moggles.Environment" value="DEV" />
    <add key="Moggles.Url" value="http://featureToggleSource.com/getFeatureToggles" />

    <!--OPTIONAL KEYS-->
    <add key="Moggles.CachingTime" value="3600"/>
    <add key="Moggles.RequestTimeout" value="30"/>
    <add key="Moggles.ApplicationInsightsInstrumentationKey" value="instrumentationKey"/>

    <!--Messaging features-->
    <add key="Moggles.UseMessaging" value = "true" />
    <add key="Moggles.MessageBusUrl" value="rabbitmq://messageBusUrl" />
    <add key="Moggles.MessageBusUser" value="user" />
    <add key="Moggles.MessageBusPassword" value="password" />
    <add key="Moggles.CacheRefreshQueue" value="cache_refresh_queue"/>
    <add key="Moggles.EnvironmentDetectorCustomAssembliesToIgnore" value="Assembly1, Assembly2"/>
  </appSettings>
```

* **.NET Core**
```C#
"Moggles": {
    //REQUIRED KEYS
    "ApplicationName": "MogglesExampleApp",
    "Environment": "DEV",
    "Url": "http://featureToggleSource.com/getFeatureToggles",

    //OPTIONAL KEYS
    "CachingTime": "3600",
    "RequestTimeout":  "30", 
    "ApplicationInsightsInstrumentationKey": "instrumentationKey",

    //Messaging features
    "UseMessaging": "true",
    "MessageBusUrl": "rabbitmq://messageBusUrl",
    "MessageBusUser": "user",
    "MessageBusPassword": "password",
    "CacheRefreshQueue": "cache_refresh_queue",
    "EnvironmentDetectorCustomAssembliesToIgnore": "Assembly1, Assembly2"
  } 
```

* At application start ``` ConfigureAndStartClient()``` method has to be called. The method will initialize the client, will cache the feature toggles and it will publish the message with the deployed feature toggles.
