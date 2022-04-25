using System.Collections.Generic;

namespace BalanceNotifier.Models.Banking;

/// <summary>
/// A container object holding a reference to a list of accounts.
/// </summary>
public class Container
{
    /// <summary>
    /// A list of accounts inside the container.
    /// </summary>
    public List<Account>? Accounts { get; set; }
}
