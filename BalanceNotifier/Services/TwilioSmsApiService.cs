using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BalanceNotifier.Constants;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="ISmsApiService" /> using the Twilio SMS API.
/// </summary>
public class TwilioSmsApiService : ISmsApiService
{
    /// <summary>
    /// The base URL of the SMS API.
    /// <para>
    /// TODO: This could be further optimized by using an edge location. This would improve the time it takes to deliver
    /// the SMS message.
    /// </para>
    /// </summary>
    private const string BASE_URL = "https://api.twilio.com";

    /// <summary>
    /// The API version of the SMS API.
    /// </summary>
    private const string API_VERSION = "2010-04-01";

    /// <summary>
    /// An HTTP client for making HTTP requests.
    /// </summary>
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new(BASE_URL)
    };

    /// <summary>
    /// A service used to manage secrets.
    /// </summary>
    private readonly ISecretsService _secretsService;

    /// <summary>
    /// A string identifier (SID) used to identify a specific Twilio SMS API account resource.
    /// </summary>
    private readonly string _twilioAccountSid;

    /// <summary>
    /// The phone number of the Twilio SMS sender.
    /// </summary>
    private readonly string _twilioSenderPhoneNumber;

    /// <summary>
    /// The phone number of the Twilio SMS recipient.
    /// </summary>
    private readonly string _twilioRecipientPhoneNumber;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="secretsService">A service used to manage secrets.</param>
    public TwilioSmsApiService(ISecretsService? secretsService = null)
    {
        _secretsService = secretsService ?? new KeyVaultSecretsService();

        _twilioAccountSid = _secretsService.GetSecret(SecretVariable.TwilioAccountSid)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioAccountSid}` is not defined.");

        _twilioSenderPhoneNumber = _secretsService.GetSecret(SecretVariable.TwilioSenderPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioSenderPhoneNumber}` is not defined.");

        _twilioRecipientPhoneNumber = _secretsService.GetSecret(SecretVariable.TwilioRecipientPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioRecipientPhoneNumber}` is not defined.");

        var twilioAuthenticationToken = _secretsService.GetSecret(SecretVariable.TwilioAuthenticationToken)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioAuthenticationToken}` is not defined.");

        var twilioBasicAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_twilioAccountSid}:{twilioAuthenticationToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                                                                        twilioBasicAuthenticationValue);
    }

    /// <inheritdoc />
    /// TODO: There's a way to verify whether the SMS was sent using Twilio's API. We should implement this, and upon
    /// failure, resend the request.
    public async Task SendMessageAsync(string body)
    {
        var response = await _httpClient.PostAsync($"{API_VERSION}/Accounts/{_twilioAccountSid}/Messages.json",
                                                   new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                                                             {
                                                                                 new("Body", body),
                                                                                 new("From", _twilioSenderPhoneNumber),
                                                                                 new("To", _twilioRecipientPhoneNumber)
                                                                             }));
        response.EnsureSuccessStatusCode();
    }
}
