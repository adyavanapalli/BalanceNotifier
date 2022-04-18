namespace BalanceNotifier.Constants;

/// <summary>
/// Represents the name of an secret variable.
/// </summary>
public class SecretVariable
{
    /// <summary>
    /// The name of the secret variable representing a Plaid client ID.
    /// </summary>
    public const string PlaidClientId = "PLAID_CLIENT_ID";

    /// <summary>
    /// The name of the secret variable representing a Plaid client secret.
    /// </summary>
    public const string PlaidClientSecret = "PLAID_CLIENT_SECRET";

    /// <summary>
    /// The name of the secret variable representing a Plaid client access token.
    /// </summary>
    public const string PlaidClientAccessToken = "PLAID_CLIENT_ACCESS_TOKEN";

    /// <summary>
    /// The name of the secret variable representing a Twilio account SID.
    /// </summary>
    public const string TwilioAccountSid = "TWILIO_ACCOUNT_SID";

    /// <summary>
    /// The name of the secret variable representing a Twilio authentication token.
    /// </summary>
    public const string TwilioAuthenticationToken = "TWILIO_AUTHENTICATION_TOKEN";

    /// <summary>
    /// The name of the secret variable representing the Twilio sender phone number.
    /// </summary>
    public const string TwilioSenderPhoneNumber = "TWILIO_SENDER_PHONE_NUMBER";

    /// <summary>
    /// The name of the secret variable representing the Twilio recipient phone number.
    /// </summary>
    public const string TwilioRecipientPhoneNumber = "TWILIO_RECIPIENT_PHONE_NUMBER";
}
