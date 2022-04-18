namespace BalanceNotifier.Models.Banking;

/// <summary>
/// A set of fields describing the balance for an <see cref="Account" />.
/// </summary>
public class Balances
{
    /// <summary>
    /// The total amount of funds in or owed by the account.
    /// </summary>
    public double? Current { get; set; }
}
