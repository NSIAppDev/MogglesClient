# MogglesClient

Client code for the [Moggles](https://github.com/NSIAppDev/Moggles) project.  
Released as a Nuget Package.


Retrieves feature toggles from source and caches them in the application at a configurable period of time. Available for both .NET Core and .NET Framework.  

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

* The feature toggles are retrieved using an API provided by each application it is installed in. Even though it was originally created for [Moggles](https://github.com/NSIAppDev/Moggles), the API is configurable.
  * A timeout period for the call can be configured, the default value is 30s.
* The feature toggles are retrieved based on the **Application** and **Environment**.
* The feature toggles are saved in the application cache and are refreshed hourly. The period of time in which the feature toggles are refreshed is configurable.
  * Feature toggles are saved in two cache entries, an expiring one and a persistent one. 
  * If the call that retrieves the toggles fails, the persistent cache will hold the previous feature toggles values that are going to be used and the call will be retried every 3 minutes until successful.
* Check if a feature toggle is enabled.
* Get all feature toggle values.
____________________________________
  :heavy_exclamation_mark: *In order to make use of the following features a [Rabbitmq](https://www.rabbitmq.com/configure.html) machine will need to be setup.*
  
  The following features are enabled by default, but they can be disabled by adding a key in the application configuration file (*UseMessaging*).
  
  The **message bus url**, **user** and **password** will need to be provided.
  
* **Force cache refresh** 

  * If the impact of a toggle needs to be immediate, a force cache event can be handled by the client.  
  * The queue name for this event will need to be provided.  
  * The consumer implemented in the MogglesClient will read the message from the queue and based on the **Application** and **Environment** it will refresh the corresponding application. The expected message contract can be found [here](./MogglesClient/Messaging/RefreshCache/RefreshTogglesCache.cs).
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles).
  
* **Show deployed feature toggles**
  
    At application start, the client will search all assemblies for feature toggles and will publish a message containing the feature toggles found in the application. [Moggles](https://github.com/NSIAppDev/Moggles) will read the message and update the deployed status of the feature toggles. The published message contract can be found [here](./MogglesClient/Messaging/EnvironmentDetector/RegisteredTogglesUpdate.cs). 
    * A list of assemblies that can be ignored in the search can be provided.
  
  More information on how this feature is implemented can be found in the [Moggles documentation](https://github.com/NSIAppDev/Moggles).

## Logging

Different information is logged in [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview):  
  * An instrumentation key will need to be provided in order for this feature to be available.

    Exceptions logged| Events logged
    --- | --- 
    The API call fails | When the cache entries are successfully refreshed 
    When the FTs are not available (both cache entries are empty) | When a force cache refresh event was handled 
    When detecting the deployed feature toggles and one of the assemblies it searches in could not be loaded | 

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

* At application start ```Moggles.ConfigureAndStartClient()``` method has to be called. The method will initialize the client, will cache the feature toggles and it will publish the message with the deployed feature toggles.  
  * The method will also return the configured client instance which can be registered and injected in the code. The registration step is only necessary if the GetAllFeatureToggles method is used:  
    * **.NET Framework**
  
      ```C#
      Moggles mogglesClient = Moggles.ConfigureAndStartClient();
      builder.RegisterInstance(mogglesClient);
      ```  
    * **.NET Core**  
      The **IConfiguration** object will have to be passed to the method.
      ```C#
      Moggles mogglesClient = Moggles.ConfigureAndStartClient(Configuration);
      services.TryAddSingleton(mogglesClient);
      ```  

    In controller:  

    ```C#
    public FeatureToggleController(Moggles mogglesClient)
    {
       _mogglesClient = mogglesClient;
    }

    ...

    _mogglesClient.GetAllToggles();
    ```

* Adding and using a feature toggle  

    Each feature toggle needs to have a corresponding class that inherits from ```MogglesFeatureToggle``` (The feature toggle class name has to be the same as the feature toggle name returned from source):  
    
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
    Is<TestFeatureToggle>.Enabled;
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
  (having the same value in all tests)  
   
  or
   
  ```C#
  ConfigurationManager.AppSettings["Moggles.TestingMode"] = "true";
  ConfigurationManager.AppSettings["Moggles.TestFeatureToggle"] = "true";
  ```  
  (having the possibility to mock the values independently in each test)

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
   (having the same value in all tests)  
   
   or 
   
   ```C#
   configuration["Moggles:TestingMode"] = "true";
   configuration["Moggles:TestFeatureToggle"] = "true";
   ```
   
   (having the possibility to mock the values independently in each test)

## Credits  
The initial insipiration was the [feature toggle library](https://github.com/jason-roberts/FeatureToggle) created by Jason Roberts.  

## License
The project is licensed under the [GNU Affero General Public License v3.0](https://github.com/NSIAppDev/MogglesClient/blob/master/LICENSE) 
    
