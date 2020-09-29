# MogglesClient

Client code for the [Moggles](https://github.com/NSIAppDev/Moggles) project.  
Released as a Nuget Package.


Retrieves feature toggles from source ([Moggles](https://github.com/NSIAppDev/Moggles) or other application respecting the [contract](./PublicInterface/FeatureToggle.cs)) and caches them in the application (where the package is installed) at a configurable period of time. Available for both .NET Core and .NET Framework.  

1. [Installation](#installation)
2. [Features](#features)
3. [Logging](#logging)
4. [How to use?](#how-to-use)
5. [Testing](#testing)
6. [Credits](#credits)
7. [License](#license)

## Installation  
The package can be downloaded from [NuGet](https://www.nuget.org/packages/MogglesClient/).

## Features

* The feature toggles are retrieved using an API provided in the [configuration file](#how-to-use) by each application the package is installed in. Even though it was originally created for [Moggles](https://github.com/NSIAppDev/Moggles), the API can be replaced by any application respecting the [contract](./MogglesClient/PublicInterface/FeatureToggle.cs).
  * A timeout period for the call can be configured, the default value is 30s.
* The feature toggles are retrieved based on the **Application** and **Environment**.
* The feature toggles are saved in the application cache and are **refreshed hourly**. The period of time in which the feature toggles are refreshed is configurable by adding a key in the [application configuration file](#how-to-use).
  * Feature toggles are saved in two cache entries, an expiring one and a persistent one. 
  * If the call that retrieves the toggles fails, the persistent cache will hold the previous feature toggles values that are going to be used and the call will be retried every 3 minutes until successful.
  * If none of the cache entries are available, the default toggle value is **false** and an exception is logged in [Application Insights](#logging).
* [Check if a feature toggle is enabled](#adding-and-using-a-feature-toggle).
* Get all feature toggle values.
  * This feature will return a list with all the feature toggles and their values from the application cache. The class returned can be found [here](./MogglesClient/PublicInterface/FeatureToggle.cs). The client instance can be [registered in the dependency injection container](#how-to-use) in order for this feature to be used.
____________________________________
  :heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup.*
  
  The following features are disabled by default, but they can be disabled by adding a key in the application configuration file (*UseMessaging*).
  
  The **message bus url**, **user** and **password** will need to be provided in the [application configuration file](#how-to-use).
  
  #### **Force cache refresh** 

  * If the impact of a toggle needs to be visible prior to the new refresh time of the cache, a *force cache* event (triggered by [Moggles](https://github.com/NSIAppDev/Moggles)) can be handled by the client.   
  * MogglesClient will read the message from the queue and based on the **Application** and **Environment** matched by the [configuration keys](#how-to-use) provided it will refresh the feature toggles values from the application. The expected message contract can be found [here](./MogglesClient/Messaging/RefreshCache/RefreshTogglesCache.cs) (*the namespace of the contract class will also have to match*).
  * The cache will be refreshed as soon as the message is published and read from the queue.
  * If the queue name for the event is not provided, a temporary queue will be created. (It will have a randomly assigned name & is going to be deleted when there are no consumers subscribed to the queue)
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles#force-cache-refresh).
  
  #### **Show deployed feature toggles**
  
   * At application start, the client will search all assemblies for feature toggles and will publish a message containing the feature toggles found in the application. [Moggles](https://github.com/NSIAppDev/Moggles) will receive the message and update the deployed status of the feature toggles. The published message contract can be found [here](./MogglesClient/Messaging/EnvironmentDetector/RegisteredTogglesUpdate.cs) (*the namespace of the contract class will also have to match*). 
   * A list of assemblies that can be ignored in the search can be provided (ex: EntityFramework, System, log4net).
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles/#show-deployed-feature-toggles).

## Logging

Different information is logged in [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview):  
  * An instrumentation key will need to be provided in order for this feature to be available.

    Exceptions logged| Events logged
    --- | --- 
    The API call that retrieves the feature toggles fails | When the cache entries are successfully refreshed 
    When the FTs are not available (both cache entries are empty) | When a force cache refresh event was handled 
    When detecting the deployed feature toggles and one of the assemblies it searches in could not be loaded |   
    
 #### Custom Logging

If Application Insights is not an available option or you want to log to another location:
* Provide an implementation for [IMogglesLoggingService](https://github.com/NSIAppDev/MogglesClient/blob/master/MogglesClient/PublicInterface/IMogglesLoggingService.cs)
```C# 
    public class CustomLoggingService : IMogglesLoggingService
    {
        public void TrackEvent(string eventName, string application, string environment)
        {
            throw new NotImplementedException();
        }

        public void TrackException(Exception ex, string application, string environment)
        {
            throw new NotImplementedException();
        }

        public void TrackException(Exception ex, string customMessage, string application, string environment)
        {
            throw new NotImplementedException();
        }
    }
```
* Call overload of _ConfigureAndStartClient_:  
```C# 
Moggles.ConfigureAndStartClient(loggingService: new CustomLoggingService());
```

## How to use?

The configuration keys for MogglesClient will need to be provided in the application configuration file.

* **.NET Framework**
```C#
  <appSettings>
    <!-- MogglesClient configuration keys-->

    <!--REQUIRED KEYS-->
    <add key="Moggles.ApplicationName" value="MogglesExampleApp" />
    <add key="Moggles.Environment" value="DEV" />
    <add key="Moggles.Url" value="http://myFeatureToggleSource.com/api/FeatureToggles/getApplicationFeatureToggles" />

    <!--OPTIONAL KEYS-->
    <add key="Moggles.CachingTime" value="3600"/>
    <add key="Moggles.RequestTimeout" value="30"/>
    <add key="Moggles.ApplicationInsightsInstrumentationKey" value="myInstrumentationKey"/>

    <!--Messaging features, also optional-->
    <add key="Moggles.UseMessaging" value = "true" />
    <add key="Moggles.MessageBusUrl" value="rabbitmq://myMessageBusUrl" />
    <add key="Moggles.MessageBusUser" value="user" />
    <add key="Moggles.MessageBusPassword" value="password" />
    <add key="Moggles.CacheRefreshQueue" value="my_cache_refresh_queue"/>
    <add key="Moggles.EnvironmentDetectorCustomAssembliesToIgnore" value="Assembly1, Assembly2"/>
  </appSettings>
```

* **.NET Core**
```C#
"Moggles": {
    //REQUIRED KEYS
    "ApplicationName": "MogglesExampleApp",
    "Environment": "DEV",
    "Url": "http://myFeatureToggleSource.com/api/FeatureToggles/getApplicationFeatureToggles",

    //OPTIONAL KEYS
    "CachingTime": "3600",
    "RequestTimeout":  "30", 
    "ApplicationInsightsInstrumentationKey": "myInstrumentationKey",

    //Messaging features, , also optional
    "UseMessaging": "true",
    "MessageBusUrl": "rabbitmq://myMessageBusUrl",
    "MessageBusUser": "user",
    "MessageBusPassword": "password",
    "CacheRefreshQueue": "my_cache_refresh_queue",
    "EnvironmentDetectorCustomAssembliesToIgnore": "Assembly1, Assembly2"
  } 
```

* At application start ```Moggles.ConfigureAndStartClient()``` method has to be called. The method will initialize the client, will cache the feature toggles and it will publish the message with the deployed feature toggles.  
  * The method will also return the configured client instance which can be registered and injected in the code. The registration step is only necessary if the GetAllFeatureToggles method is used:  
    * **.NET Framework**  
    
        Global.asax  
      ```C#
      using Autofac;  
      using MogglesClient.PublicInterface;  
      
      public void Application_Start()
      {
        Moggles mogglesClient = Moggles.ConfigureAndStartClient();
        DependencyInjectionContainer.RegisterInstance(mogglesClient);
      }
      ```  
    * **.NET Core**  
    
      The **IConfiguration** object will have to be passed to the method.  
      
      Startup.cs  
      ```C#
      using Microsoft.Extensions.DependencyInjection.Extensions;  
      using MogglesClient.PublicInterface;  
      
      public void ConfigureServices(IServiceCollection services)
      {
        Moggles mogglesClient = Moggles.ConfigureAndStartClient(Configuration);
        services.TryAddSingleton(mogglesClient);
      }
      ```  

    In controller:  

    ```C#
    using MogglesClient.PublicInterface;  
    
    public FeatureToggleController(Moggles mogglesClient)
    {
       _mogglesClient = mogglesClient;
    }

    ...

    _mogglesClient.GetAllToggles();
    ```

* #### Adding and using a feature toggle  

    Each feature toggle needs to have a corresponding class that inherits from ```MogglesFeatureToggle```. The feature toggle class name has to be the same as the feature toggle name returned from source ([Moggles](https://github.com/NSIAppDev/Moggles) or other application respecting the [contract](./PublicInterface/FeatureToggle.cs)):  
    
    ```C#
    using MogglesClient.PublicInterface;

    namespace TestApp.FeatureToggles
    {
        public class TestFeatureToggle: MogglesFeatureToggle
        {

        }
    }
    ```
   Usage:  
    ```C#
    if (Is<TestFeatureToggle>.Enabled)
    {
      ...
    }
    ```
## Testing  

In order to mock the feature toggles values ```Moggles.ConfigureForTestingMode()``` will have to be called before each test and a key will need to be added in the tests configuration file (*TestingMode*) together with the feature toggle value.

* **NET. Framework**  
  ```C#
  Moggles.ConfigureForTestingMode();
  ```
  
  In configuration file:  
  ```C#
    <add key="Moggles.TestingMode" value="true" />
    <add key="Moggles.TestFeatureToggle" value="true" />
  ```  
  In this case ```TestFeatureToggle``` will have the same value in all tests.  
   
  or
   
  ```C#
  ConfigurationManager.AppSettings["Moggles.TestingMode"] = "true";
  ConfigurationManager.AppSettings["Moggles.TestFeatureToggle"] = "true";
  ```  
  This option can be used to set ```TestFeatureToggle``` with different values in each test.

* **NET. Core**  
  ```C#
   var configuration = new ConfigurationBuilder()
       .AddJsonFile("testConfig.json")
       .Build();  
   Moggles.ConfigureForTestingMode(configuration);
   ```
 
   In configuration file:  
   ```C#
   "Moggles": {
     "TestingMode": "true",
     "TestFeatureToggle":  "true" 
   } 
   ```
   In this case ```TestFeatureToggle``` will have the same value in all tests.  
   
   or 
   
   ```C#
   configuration["Moggles:TestingMode"] = "true";
   configuration["Moggles:TestFeatureToggle"] = "true";
   ```
   
   This option can be used to set ```TestFeatureToggle``` with different values in each test.

## Credits  
The initial insipiration was the [feature toggle library](https://github.com/jason-roberts/FeatureToggle) created by Jason Roberts.  

## License
The project is licensed under the [GNU Affero General Public License v3.0](./LICENSE) 
    
