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
    /// <para>
    /// TODO: This could be further optimized by using an edge location. This would improve the time it takes to deliver
    /// the SMS message.
    /// </para>
    /// </summary>
    public const string Twilio = "https://api.twilio.com";
}
