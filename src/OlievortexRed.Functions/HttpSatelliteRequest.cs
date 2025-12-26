using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OlievortexRed.Lib;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Processes.Models;

namespace OlievortexRed.Functions;

public class HttpSatelliteRequest
{
    private const int ContainerLimit = 1;
    private const string Queue = "satellite_aws_posters";

    private readonly ILogger<HttpSatelliteRequest> _logger;
    private readonly ISatelliteSource _satelliteSource;
    private readonly ISatelliteProcess _satelliteProcess;
    private readonly IAmazonS3 _awsClient;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly DefaultAzureCredential _defaultAzureCredential;

    public HttpSatelliteRequest(ILogger<HttpSatelliteRequest> logger, ISatelliteSource satelliteSource,
        ISatelliteProcess satelliteProcess, IConfiguration configuration)
    {
        _logger = logger;
        _satelliteSource = satelliteSource;
        _satelliteProcess = satelliteProcess;

        var olieConfig = new OlieConfig(configuration);
        var credOptions = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true
        };
        _defaultAzureCredential = new DefaultAzureCredential(credOptions);
        _awsClient = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
        var serviceBusClient = new ServiceBusClient(olieConfig.OlieAwsServiceBus, _defaultAzureCredential);
        _serviceBusSender = serviceBusClient.CreateSender(Queue);
        _blobContainerClient =
            new BlobContainerClient(new Uri(olieConfig.OlieBlobBronzeContainerUri), _defaultAzureCredential);
    }

    [Function(nameof(HttpSatelliteRequest))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequest req,
        [Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute]
        SatellitePreviewRequest request, CancellationToken ct)
    {
        _logger.LogInformation("HttpSatelliteRequest: EffectiveDate: {a}", request.EffectiveDate);

        var olie = new SatellitePreviewProcess(_satelliteSource, _satelliteProcess);
        await olie.RunAsync(request, _awsClient, ContainerLimit, _serviceBusSender, Delay, _blobContainerClient,
            _defaultAzureCredential, ct);

        return new OkObjectResult("Welcome to Azure Functions!");

        async Task Delay(int attempt)
        {
            await Task.Delay(30 * attempt, ct);
        }
    }
}