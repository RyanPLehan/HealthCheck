# HealthCheck 
A .Net library that designed to monitor and respond to a Health Check probe.  
This library support both HTTP and TCP Probes without the need of any hosting server (ie IIS or Apache).
The ideal application is a Windows Service or Kuberentes/Docker container that runs as a background service.


## Supported Health Check Monitor Types
1.  *Status* - Responds with one or more Health Check Results in JSON format.  
    -  The output is similar to MS Health Check Results. 
    -  This is useful for Application Monitors
    -  Only for HTTP Probe
2.  *Startup* - Response is based upon probe type
    -  HTTP Probe - HTTP 200 OK if **ALL** health check results are Healthy, else HTTP 503 System Unavailable
    -  TCP Probe - Will not accept connection until **ALL** health checks are Healthy.
3.  *Readiness* - Response is based upon probe type
    -  HTTP Probe - HTTP 200 OK if **ALL** health check results are Healthy, else HTTP 503 System Unavailable
    -  TCP Probe - Will not accept connection until **ALL** health checks are Healthy.
4.  *Liveness* - Response is based upon probe type
    -  HTTP Probe - HTTP 200 OK if **ALL** health check results are Healthy, else HTTP 503 System Unavailable
    -  TCP Probe - Will not accept connection until **ALL** health checks are Healthy.


## HTTP Probes
1.  A particular Health Check monitor will respond to a HTTP probe if the endpoint is assigned (ie not null) in the configuration.
    -  One or more **Endpoints** must be defined for the HTTP/HTTPS Probe to be supported as a whole
2.  Obviously, uses a Request/Response model.  
    -  This means, health checks are executed when a request for a specific endpoint is requested.  Then the response is returned.
3.  Supports both HTTP and/or HTTPS, but the following must be observed:
    -  HTTP will be supported when the **Port** has been assigned a valid port number.
    -  HTTPS will be supported when the **SslPort** has been assigned a valid port number
4.  Only supports HTTP **GET** Method


## TCP Probes
1.  Only *Startup*, *Readiness* and *Liveness* health checks are supported
2.  A particular Health Check monitor will respond to a TCP probe if a port number is assigned (ie not null) in the configuration.
3.  Each Health Check Monitor can be assigned an individual or shared port number.
4.  When probed, a **Successful** indication is when the connection is accepted and then closed.
5.  This is **not** a Request/Response model.
    -  This means, health checks are executed until **ALL** results are Healthy.  Then a port will be listened upon for a probe.
6.  Startup and Readiness monitors are a **One Time Occurrance**.
    -  This means once a TCP probe is successful, it will not be reactivated.
7.  Liveness monitor will be active for the entire lifetime of the applicaiton.
    -  Health Checks will be re-ran after each successful probe.

### TCP Probes **MUST READ**
Kuberentes has a *Startup*, *Readiness*, and *Liveness* Probe (in order)  
Therefore, if defined, the monitoring will not progress until the probe has occurred.  
Meaning, if *Startup*, *Readiness* and *Liveness* are defined to be monitored.  
Then the monitor will not respond to the *Readiness* probe until the *Startup* probe has occurred.  
The same applies to *Liveness*, in that, the monitor will not respond to the *Liveness* probe until the *Readiness* probe has occurred.  


## Health Checks
1.  All Health Check Monitors have a default Health Check routine that will return a single result.  
    These defaults are in place so that a client can respond without the need of a custom written health check.
    -  *Status* - By default will return **UnHealthy**
        -  This works in accordance to Microsoft's Health Check
    -  *Startup* - By default will return **Healthy**
    -  *Readiness* - By default will return **Healthy**
    -  *Liveness* - By default will return **Healthy**
2.  When the *Status* Monitor has been defined, the following should be noted.
    -  The client should have a custom written health check.
    -  When a custom written health check is added, the default will automatically be removed so that the result of **UnHealthy** is not returned.
3.  When the *Startup*, *Readiness* or *Liveness* Monitor have been defined, the following should be noted.
    -  A custom written health check is optional.
    -  When a custom written health check is added, the default health check will automatically be removed.
4.  Client custom written Health Checks
    -  Can have as many as needed
    -  Must be registered via the AddHealthChecks Dependency Injection service collection extension method
    -  Health Checks are specifically registered to a Monitor Type (ie Status, Startup, Readiness, Liveness)
    -  The same custom written Health Check can be registered to multiple Monitor Types.


