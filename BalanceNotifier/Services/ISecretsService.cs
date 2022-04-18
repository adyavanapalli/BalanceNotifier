using System.Threading.Tasks;

namespace BalanceNotifier.Services;

/// <summary>
/// An interface specifying the contract that a secrets service should implement.
/// </summary>
public interface ISecretsService
{
    /// <summary>
    /// Gets the value of specified secret from the underlying secrets store.
    /// </summary>
    /// <param name="name">The name of the secret to get.</param>
    /// <returns>The secret value if it exists, else <see langword="null" />.</returns>
    string? GetSecret(string name);
}
