namespace BalanceNotifier.Models.Banking;

/// <summary>
/// A model representing a bank account.
/// </summary>
public class Account
{
    /// <summary>
    /// A set of fields describing the balance for an account.
    /// </summary>
    public Balances? Balances { get; set; }

    /// <summary>
    /// The type of account.
    /// </summary>
    public string? Type { get; set; }
}
