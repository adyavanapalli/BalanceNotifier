namespace BalanceNotifier.Constants;

/// <summary>
/// Represents a base URL for an API.
/// </summary>
public class BaseUrl
{
    /// <summary>
    /// A base URL for the Plaid API.
    /// <para>
    /// TODO: This base URL needs to be eventually updated to a production API base URL.
    /// </para>
    /// </summary>
    public const string Plaid = "https://development.plaid.com";

    /// <summary>
    /// A base URL for the Twilio
    /// </summary>
    public const string Twilio = "https://api.ashburn.us1.twilio.com";
}
