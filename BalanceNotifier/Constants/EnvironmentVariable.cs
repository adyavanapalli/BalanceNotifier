namespace BalanceNotifier.Constants;

/// <summary>
/// Represents the name of an environment variable.
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    /// The name of the environment variable representing the Azure Key Vault URI.
    /// </summary>
    public const string AzureKeyVaultUri = "AZURE_KEY_VAULT_URI";

    /// <summary>
    /// The name of the environment variable representing the Azure Storage Account Table URI.
    /// </summary>
    public const string AzureStorageAccountTableUri = "AZURE_STORAGE_ACCOUNT_TABLE_URI";

    /// <summary>
    /// The name of the environment variable representing the name of the Azure Storage Account Table.
    /// </summary>
    public const string AzureStorageAccountTableName = "AZURE_STORAGE_ACCOUNT_TABLE_NAME";
}
