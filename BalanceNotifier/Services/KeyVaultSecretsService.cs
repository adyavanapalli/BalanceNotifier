using System;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BalanceNotifier.Constants;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="ISecretsService" /> using Azure Key Vault as the underlying secrets
/// store.
/// </summary>
public class KeyVaultSecretsService : ISecretsService
{
    /// <summary>
    /// A secrets client used for managing the secrets store.
    /// </summary>
    private readonly SecretClient _secretClient;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="secretClient">A secrets client used for managing the secrets store.</param>
    public KeyVaultSecretsService()
    {
        var azureKeyVaultUri = Environment.GetEnvironmentVariable(EnvironmentVariable.AzureKeyVaultUri)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.AzureKeyVaultUri}` is not defined.");

        _secretClient = new(new(azureKeyVaultUri), new DefaultAzureCredential());
    }

    /// <inheritdoc />
    public string? GetSecret(string name)
    {
        var keyVaultSecret = _secretClient.GetSecret(name);
        var secretValue = keyVaultSecret?.Value.Value;

        return secretValue;
    }
}
