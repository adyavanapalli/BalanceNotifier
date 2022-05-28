using System.Threading.Tasks;
using Azure.Data.Tables;

namespace BalanceNotifier.Services;

/// <summary>
/// An interface specifying the contract that a Storage service must implement.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Asynchronously gets the previously stored <see cref="TableEntity" /> from storage.
    /// </summary>
    /// <returns>A task wrapping a <see cref="TableEntity" />.</returns>
    Task<TableEntity> GetTableEntity();

    /// <summary>
    /// Asynchronously stores the specified <paramref name="tableEntity" /> in storage. If the <paramref
    /// name="tableEntity" /> already exists, then it is updated.
    /// </summary>
    /// <param name="tableEntity">The <see cref="TableEntity" /> to store.</param>
    /// <returns>A task wrapping the result of the action.</returns>
    Task StoreTableEntity(TableEntity tableEntity);
}
