using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BalanceNotifier.Constants;
using Microsoft.Extensions.Logging;
using Polly;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="ISmsApiService" /> using the Twilio SMS API.
/// </summary>
public class TwilioSmsApiService : ISmsApiService
{
    /// <summary>
    /// The API version of the SMS API.
    /// </summary>
    private const string API_VERSION = "2010-04-01";

    /// <summary>
    /// An HTTP client for making HTTP requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// A logger used for logging information.
    /// </summary>
    private readonly ILogger<TwilioSmsApiService> _logger;

    /// <summary>
    /// A service used to manage secrets.
    /// </summary>
    private readonly ISecretsService _secretsService;

    /// <summary>
    /// A string identifier (SID) used to identify a Twilio API account.
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
    /// <param name="httpClient">An HTTP client for making HTTP requests.</param>
    /// <param name="logger">A logger used for logging information.</param>
    /// <param name="secretsService">A service used to manage secrets.</param>
    public TwilioSmsApiService(HttpClient httpClient,
                               ILogger<TwilioSmsApiService> logger,
                               ISecretsService secretsService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _secretsService = secretsService ?? throw new ArgumentNullException(nameof(secretsService));

        _twilioAccountSid = _secretsService.GetSecret(SecretVariable.TwilioAccountSid)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioAccountSid}` is not defined.");

        _twilioSenderPhoneNumber = _secretsService.GetSecret(SecretVariable.TwilioSenderPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioSenderPhoneNumber}` is not defined.");

        _twilioRecipientPhoneNumber = _secretsService.GetSecret(SecretVariable.TwilioRecipientPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioRecipientPhoneNumber}` is not defined.");

        var twilioApiKeySid = _secretsService.GetSecret(SecretVariable.TwilioApiKeySid)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioApiKeySid}` is not defined.");

        var twilioApiKeySecret = _secretsService.GetSecret(SecretVariable.TwilioApiKeySecret)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.TwilioApiKeySecret}` is not defined.");

        _httpClient.BaseAddress = new(BaseUrl.Twilio);

        var twilioBasicAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{twilioApiKeySid}:{twilioApiKeySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                                                                        twilioBasicAuthenticationValue);

        _logger.LogInformation("[{Source}] Exiting constructor.", nameof(TwilioSmsApiService));
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(string body)
    {
        _logger.LogInformation("[{Source}] Attempting to send a text message.", nameof(SendMessageAsync));

        var response = await Policy.HandleResult<HttpResponseMessage>(httpResponseMessage => !httpResponseMessage.IsSuccessStatusCode)
                                   .RetryAsync(3,
                                               (context, _) =>
                                               {
                                                   _logger.LogInformation("[{Source}] Request returned with response:\n{Response}",
                                                                          nameof(SendMessageAsync),
                                                                          context.Result);
                                               })
                                   .ExecuteAsync(() =>
                                   {
                                       return _httpClient.PostAsync($"{API_VERSION}/Accounts/{_twilioAccountSid}/Messages.json",
                                                                    new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                                                                                              {
                                                                                                  new("Body", body),
                                                                                                  new("From", _twilioSenderPhoneNumber),
                                                                                                  new("To", _twilioRecipientPhoneNumber)
                                                                                              }));
                                   });

        _logger.LogInformation("[{Source}] Request returned returned with response:\n{Response}",
                               nameof(SendMessageAsync),
                               response);

        response.EnsureSuccessStatusCode();
    }
}
