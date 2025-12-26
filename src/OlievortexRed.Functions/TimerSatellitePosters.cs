using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OlievortexRed.Lib;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.StormEvents.Interfaces;

namespace OlievortexRed.Functions;

public class TimerSatellitePosters
{
    private const bool RunOnStartup = false;
    private const int ContainerLimit = 1;

    private const string Queue = "satellite_aws_posters";
    private readonly IAmazonS3 _awsClient;
    private readonly BlobContainerClient _goldClient;
    private readonly BlobContainerClient _bronzeClient;
    private readonly DefaultAzureCredential _credential;
    private readonly ILogger<TimerSatellitePosters> _logger;
    private readonly ISatelliteProcess _satelliteProcess;
    private readonly ISatelliteSource _satelliteSource;
    private readonly IDailySummaryBusiness _dailySummaryBusiness;
    private readonly ServiceBusClient _serviceBusClient;

    public TimerSatellitePosters(ILogger<TimerSatellitePosters> logger, IConfiguration configuration,
        ISatelliteProcess satelliteProcess, ISatelliteSource satelliteSource,
        IDailySummaryBusiness dailySummaryBusiness)
    {
        _logger = logger;
        _satelliteProcess = satelliteProcess;
        _satelliteSource = satelliteSource;
        _dailySummaryBusiness = dailySummaryBusiness;

        _awsClient = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);

        var olieConfig = new OlieConfig(configuration);
        var credOptions = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true
        };
        _credential = new DefaultAzureCredential(credOptions);
        _serviceBusClient = new ServiceBusClient(olieConfig.OlieAwsServiceBus, _credential);
        _goldClient = new BlobContainerClient(new Uri(olieConfig.OlieBlobGoldContainerUri), _credential);
        _bronzeClient = new BlobContainerClient(new Uri(olieConfig.OlieBlobBronzeContainerUri), _credential);
    }

    [Function(nameof(TimerSatellitePosters))]
    public async Task Run([TimerTrigger("0 0,45 8 * * *", RunOnStartup = RunOnStartup)] TimerInfo myTimer,
        CancellationToken ct)
    {
        if (Program.IsShortCircuit) return;

        try
        {
            _logger.LogInformation("OlievortexRed TimerSatellitePosters triggered");

            var sender = _serviceBusClient.CreateSender(Queue);
            var olie = new SatellitePosterProcess(_satelliteProcess, _satelliteSource, _dailySummaryBusiness);
            await olie.RunAsync(ContainerLimit, sender, Delay, _bronzeClient, _goldClient, _awsClient, _credential, ct);

            if (myTimer.ScheduleStatus is not null)
                _logger.LogInformation("OlievortexRed TimerSatellitePosters Next timer schedule at: {a}",
                    myTimer.ScheduleStatus.Next);
        }
        catch (Exception ex)
        {
            _logger.LogError("OlievortexRed TimerSatellitePosters Error: {a}", ex.ToString());
            throw;
        }

        return;

        async Task Delay(int attempt)
        {
            await Task.Delay(30 * attempt, ct);
        }
    }
}