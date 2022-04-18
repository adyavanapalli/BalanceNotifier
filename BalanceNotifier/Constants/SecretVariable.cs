namespace BalanceNotifier.Constants;

/// <summary>
/// Represents the name of an secret variable.
/// </summary>
public class SecretVariable
{
    /// <summary>
    /// The name of the secret variable representing a Plaid client ID.
    /// </summary>
    public const string PlaidClientId = "PLAID-CLIENT-ID";

    /// <summary>
    /// The name of the secret variable representing a Plaid client secret.
    /// </summary>
    public const string PlaidClientSecret = "PLAID-CLIENT-SECRET";

    /// <summary>
    /// The name of the secret variable representing a Plaid client access token.
    /// </summary>
    public const string PlaidClientAccessToken = "PLAID-CLIENT-ACCESS-TOKEN";

    /// <summary>
    /// The name of the secret variable representing a Twilio API key SID.
    /// </summary>
    public const string TwilioApiKeySid = "TWILIO-API-KEY-SID";

    /// <summary>
    /// The name of the secret variable representing a Twilio API key secret.
    /// </summary>
    public const string TwilioApiKeySecret = "TWILIO-API-KEY-SECRET";

    /// <summary>
    /// The name of the secret variable representing the Twilio sender phone number.
    /// </summary>
    public const string TwilioSenderPhoneNumber = "TWILIO-SENDER-PHONE-NUMBER";

    /// <summary>
    /// The name of the secret variable representing the Twilio recipient phone number.
    /// </summary>
    public const string TwilioRecipientPhoneNumber = "TWILIO-RECIPIENT-PHONE-NUMBER";
}
