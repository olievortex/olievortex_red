using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OlievortexRed.Lib;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Cli;

public class OlieCliDi
{
    private DefaultAzureCredential? _defaultAzureCredential;
    private OlieConfig? _olieConfig;
    private CosmosContext? _olieCosmosContext;

    public DefaultAzureCredential GetDefaultAzureCredential()
    {
        if (_defaultAzureCredential is not null) return _defaultAzureCredential;

        var options = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true
        };

        _defaultAzureCredential = new DefaultAzureCredential(options);
        return _defaultAzureCredential;
    }

    public OlieConfig GetOlieConfig()
    {
        if (_olieConfig is not null) return _olieConfig;

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        _olieConfig = new OlieConfig(config);
        return _olieConfig;
    }

    public CosmosContext GetOlieCosmosContext()
    {
        if (_olieCosmosContext is not null) return _olieCosmosContext;

        var config = GetOlieConfig();
        var credential = GetDefaultAzureCredential();

        var builder = new DbContextOptionsBuilder<CosmosContext>();
        builder.UseCosmos(config.OlieCosmosEndpoint, credential, config.OlieCosmosDatabase);

        _olieCosmosContext = new CosmosContext(builder.Options);
        return _olieCosmosContext;
    }

    public CosmosRepository GetOlieCosmosRepository()
    {
        var context = GetOlieCosmosContext();

        return new CosmosRepository(context);
    }
}