using Azure.Communication.Identity;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string acsEndpointKey = "ACS_ENDPOINT";

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
        var logLevel = context.Configuration["Logging:LogLevel:Default"];
        if (Enum.TryParse<LogLevel>(logLevel, out var level))
        {
            logging.SetMinimumLevel(level);
        }
    })
    .ConfigureServices(services =>
    {
        services.AddAuthorization();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<Program>>();

            var acsEndpoint = configuration[acsEndpointKey];
            if (string.IsNullOrEmpty(acsEndpoint))
            {
                logger.LogCritical($"{acsEndpointKey} is not configured. Application cannot start.");
                throw new InvalidOperationException("ACS_ENDPOINT environment variable is required but not configured.");
            }

            if (!Uri.TryCreate(acsEndpoint, UriKind.Absolute, out var endpointUri))
            {
                logger.LogCritical($"{acsEndpointKey} is not a valid URI: {{Endpoint}}", acsEndpoint);
                throw new InvalidOperationException($"{acsEndpointKey} must be a valid URI. Current value: {acsEndpoint}");
            }

            logger.LogInformation(
                "Initializing CommunicationIdentityClient with endpoint: {Endpoint}",
                endpointUri.GetLeftPart(UriPartial.Authority)
            );

            return new CommunicationIdentityClient(endpointUri, new DefaultAzureCredential());
        });
    })
    .Build();

host.Run();