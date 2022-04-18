using System.Threading.Tasks;

namespace BalanceNotifier.Services;

/// <summary>
/// An interface specifying the contract that an SMS API service should implement.
/// </summary>
public interface ISmsApiService
{
    /// <summary>
    /// Asynchronously sends an SMS message via an SMS API with the specified <paramref name="body" />.
    /// </summary>
    /// <param name="body">The message body to send.</param>
    /// <returns>A task wrapping the result of the action.</returns>
    public Task SendMessageAsync(string body);
}
