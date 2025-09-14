using SimpleDispatch.ServiceBase.Examples;

namespace SimpleDispatch.ServiceBase;

/// <summary>
/// Example program showing how to use the base service
/// This file demonstrates usage and can be removed when creating the NuGet package
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create and run an example microservice
        var service = new ExampleMicroservice(args);
        await service.RunAsync();
    }
}
