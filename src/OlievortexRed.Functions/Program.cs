using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OlievortexRed.Lib;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Maps.Satellite;
using OlievortexRed.Lib.Radar;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;
using OlievortexRed.Lib.StormPredictionCenter.Mesos;

namespace OlievortexRed.Functions;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static readonly bool IsShortCircuit = false;

    public static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();
        builder.Services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
        builder.Services.AddScoped<IOlieWebServices, OlieWebServices>();
        builder.Services.AddScoped<IOlieImageServices, OlieImageServices>();
        builder.Services.AddScoped<ICosmosRepository, CosmosRepository>();
        builder.Services.AddScoped<ISpcBusiness, SpcBusiness>();
        builder.Services.AddScoped<ISpcProcess, SpcProcess>();
        builder.Services.AddScoped<IDailySummaryBusiness, DailySummaryBusiness>();
        builder.Services.AddScoped<ISpcSource, SpcSource>();
        builder.Services.AddScoped<IMesoProductProcess, MesoProductProcess>();
        builder.Services.AddScoped<IMesoProductSource, MesoProductSource>();
        builder.Services.AddScoped<IMesoProductParsing, MesoProductParsing>();
        builder.Services.AddScoped<IRadarBusiness, RadarBusiness>();
        builder.Services.AddScoped<IRadarSource, RadarSource>();

        #region Satellite Services

        builder.Services.AddScoped<ISatelliteAwsBusiness, SatelliteAwsBusiness>();
        builder.Services.AddScoped<ISatelliteAwsSource, SatelliteAwsSource>();
        builder.Services.AddScoped<ISatelliteProcess, SatelliteProcess>();
        builder.Services.AddScoped<ISatelliteSource, SatelliteSource>();
        builder.Services.AddScoped<ISatelliteIemBusiness, SatelliteIemBusiness>();
        builder.Services.AddScoped<ISatelliteIemSource, SatelliteIemSource>();

        #endregion

        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<TimerDailyStormDownload>()
            .Build();
        builder.AddCosmosDb();

        builder.Build().Run();
    }

    private static void AddCosmosDb(this FunctionsApplicationBuilder builder)
    {
        var config = new OlieConfig(builder.Configuration);
        var credOptions = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true
        };

        builder.Services.AddDbContext<CosmosContext>(options =>
        {
            options.UseCosmos(config.OlieCosmosEndpoint, new DefaultAzureCredential(credOptions),
                config.OlieCosmosDatabase);
        }, optionsLifetime: ServiceLifetime.Singleton);
    }
}