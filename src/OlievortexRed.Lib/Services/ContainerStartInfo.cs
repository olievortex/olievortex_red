using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;

namespace OlievortexRed.Lib.Services;

[ExcludeFromCodeCoverage]
public class ContainerStartInfo
{
    public TokenCredential Credential { get; init; } = new DefaultAzureCredential();
    public string SubscriptionId { get; init; } = string.Empty;
    public string ResourceGroupName { get; init; } = string.Empty;
    public string ContainerGroupNameStart { get; init; } = string.Empty;
}