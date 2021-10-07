# Introduction 
This nuget Package contains configuration to setup Health-Endpoints for a Service.
The Endpoints will be configured by the [App Metrics Framework](https://www.app-metrics.io/)

## Configuration
* appsettings.json
```json
"HealthOptions": {
    "Enabled": true
},
"HealthEndpointsOptions": {
"HealthEndpointEnabled": false,
"HealthEndpointRoute": "/health",
"PingEndpointEnabled": false,
"Timeout": "0:0:10"
},
"MetricEndpointsOptions": {
"MetricsEndpointEnabled": false,
"MetricsTextEndpointEnabled": false,
"EnvironmentInfoEndpointEnabled": false
}
```

* appsettings.Development.json
```json
"HealthEndpointsOptions": {
    "HealthEndpointEnabled": true,
    "PingEndpointEnabled": true
  },
  "MetricEndpointsOptions": {
    "MetricsEndpointEnabled": true,
    "MetricsTextEndpointEnabled": true,
    "EnvironmentInfoEndpointEnabled": true
  }
```

* Startup 
    * ConfigureServices
   ```c#
   services.AddAppHealth(Configuration);
   ```
    * Configure:
   ```c#
   app.UseAppHealth();
   ```
* launchSettings.json
    * add another port into ASPNETCORE_URLS. Like:
  ```
  "ASPNETCORE_URLS": "http://*:50089;http://*:50090"
  ```
* Dockerfile 
    * Change the config to: 

  ```
  ENV ASPNETCORE_URLS="https://*:5000"
  EXPOSE 5000
  ```

## HealthCheck
To support a Health-Check the Service needs to implement a class  which is derivate from App.Metrics.Health.HealthCheck.

## Exposed Endpoints
The Service expose the following endpoints:
- http://url_of_service/metrics
- http://url_of_service/metrics-text
- http://url_of_service/env
- http://url_of_service/health (Only if Health-Class is created)