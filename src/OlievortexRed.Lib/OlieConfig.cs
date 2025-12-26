using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace OlievortexRed.Lib;

[ExcludeFromCodeCoverage]
public class OlieConfig(IConfiguration config)
{
    public string OlieBlobBronzeContainerUri => config["OlieBlobBronzeContainerUri"] ??
                                                throw new ApplicationException(
                                                    "OlieBlobBronzeContainerUri configuration missing");

    public string OlieBlobGoldContainerUri => config["OlieBlobGoldContainerUri"] ??
                                              throw new ApplicationException(
                                                  "OlieBlobGoldContainerUri configuration missing");

    public string OlieCosmosDatabase => config["OlieCosmosDatabase"] ??
                                        throw new ApplicationException("OlieCosmosDatabase configuration missing");

    public string OlieCosmosEndpoint => config["OlieCosmosEndpoint"] ??
                                        throw new ApplicationException("CosmosEndpoint configuration missing");

    public string OlieAwsServiceBus => config["OlieAwsServiceBus"] ??
                                       throw new AggregateException("OlieAwsServiceBus configuration missing");
}