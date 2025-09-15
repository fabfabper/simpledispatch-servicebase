# API Port Configuration Examples

This document shows various ways to configure the API server port and URLs for your microservice.

## Method 1: Configuration via appsettings.json

```json
{
  "Api": {
    "HttpPort": 8080,
    "HttpsPort": 8443,
    "EnableHttpsRedirection": true,
    "EnableSwaggerInProduction": false
  }
}
```

## Method 2: Programmatic Configuration

### Single HTTP Port

```csharp
public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
        // Configure HTTP port only
        ConfigureHttpPort(8080);
    }
}
```

### HTTP and HTTPS Ports

```csharp
public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
        // Configure both HTTP and HTTPS ports
        ConfigureHttpPorts(8080, 8443);
    }
}
```

### Custom URLs

```csharp
public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
        // Listen on all interfaces
        ConfigureUrls(
            "http://0.0.0.0:8080",
            "https://0.0.0.0:8443"
        );

        // Or specific interfaces
        ConfigureUrls(
            "http://192.168.1.100:8080",
            "http://127.0.0.1:8080"
        );
    }
}
```

## Method 3: Mixed Configuration

You can combine both approaches - configuration takes precedence over programmatic settings:

```csharp
public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
        // Default port (will be overridden by appsettings.json if present)
        ConfigureHttpPort(3000);
    }
}
```

```json
{
  "Api": {
    "HttpPort": 8080, // This will override the programmatic setting
    "EnableHttpsRedirection": false
  }
}
```

## Method 4: Environment-Specific Configuration

### appsettings.Development.json

```json
{
  "Api": {
    "HttpPort": 5000,
    "HttpsPort": 5001,
    "EnableSwaggerInProduction": true
  }
}
```

### appsettings.Production.json

```json
{
  "Api": {
    "HttpPort": 80,
    "HttpsPort": 443,
    "EnableSwaggerInProduction": false,
    "EnableHttpsRedirection": true
  }
}
```

## Common Scenarios

### Docker Container

```csharp
public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
        // Listen on all interfaces for Docker
        ConfigureUrls("http://0.0.0.0:8080");
    }
}
```

### Development with Multiple Services

```json
{
  "Api": {
    "HttpPort": 5001 // User Service
  }
}
```

```json
{
  "Api": {
    "HttpPort": 5002 // Order Service
  }
}
```

### Load Balancer Setup

```json
{
  "Api": {
    "Urls": ["http://0.0.0.0:8080", "http://0.0.0.0:8081"]
  }
}
```

## Configuration Priority

1. **Custom URLs** (highest priority)
2. **HTTP/HTTPS Ports**
3. **Programmatic configuration**
4. **ASP.NET Core defaults** (lowest priority)

## Health Check Endpoint

Regardless of the port configuration, the health check endpoint will always be available at:

- `http://your-host:your-port/health`

For example, if you configure port 8080:

- `http://localhost:8080/health`
