using System;
using System.Configuration;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Identity;
using BalanceNotifier.Constants;
using Microsoft.Extensions.Logging;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="IStorageService" /> using Azure Storage Account Tables as the
/// underlying storage.
/// </summary>
public class StorageAccountTableService : IStorageService
{
    /// <summary>
    /// A logger used for logging information.
    /// </summary>
    private readonly ILogger<StorageAccountTableService> _logger;

    /// <summary>
    /// An Azure Storage Account Tables client used for managing storage.
    /// </summary>
    private readonly TableClient _tableClient;

    /// <summary>
    /// Constructor.
    /// <para>
    /// TODO: This needs logging.
    /// </para>
    /// </summary>
    /// <param name="logger">A logger used for logging information.</param>
    public StorageAccountTableService(ILogger<StorageAccountTableService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var azureStorageAccountTableUri = Environment.GetEnvironmentVariable(EnvironmentVariable.AzureStorageAccountTableUri)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.AzureStorageAccountTableUri}` is not defined.");

        var azureStorageAccountTableName = Environment.GetEnvironmentVariable(EnvironmentVariable.AzureStorageAccountTableName)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.AzureStorageAccountTableName}` is not defined.");

        _tableClient = new(new(azureStorageAccountTableUri), azureStorageAccountTableName, new DefaultAzureCredential());
    }

    /// <inheritdoc />
    /// TODO: This API call needs to be fortified using Polly.
    /// TODO: This needs logging.
    public async Task<TableEntity> GetTableEntity()
    {
        var response = await _tableClient.GetEntityAsync<TableEntity>(Guid.Empty.ToString(), Guid.Empty.ToString());
        return response!.Value;
    }

    /// <inheritdoc />
    /// TODO: This API call needs to be fortified using Polly.
    /// TODO: This needs logging.
    public async Task StoreTableEntity(TableEntity tableEntity)
    {
        await _tableClient.UpsertEntityAsync(tableEntity);
    }
}
