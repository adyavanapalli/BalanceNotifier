using System.Threading.Tasks;
using BalanceNotifier.Models.Banking;

namespace BalanceNotifier.Services;

/// <summary>
/// An interface specifying the contract that a Banking API service must implement.
/// </summary>
public interface IBankingApiService
{
    /// <summary>
    /// Asynchronously gets a container containing a list of accounts and their associated balances in the bank.
    /// </summary>
    /// <returns>A task wrapping a <see cref="Container" /> that contains a list of <see cref="Account" />.</returns>
    public Task<Container> GetAccountBalancesAsync();
}
