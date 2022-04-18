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
    public TwilioSmsApiService()
    {
        _twilioAccountSid = Environment.GetEnvironmentVariable(EnvironmentVariable.TwilioAccountSid)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.TwilioAccountSid}` is not defined.");

        _twilioSenderPhoneNumber = Environment.GetEnvironmentVariable(EnvironmentVariable.TwilioSenderPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.TwilioSenderPhoneNumber}` is not defined.");

        _twilioRecipientPhoneNumber = Environment.GetEnvironmentVariable(EnvironmentVariable.TwilioRecipientPhoneNumber)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.TwilioRecipientPhoneNumber}` is not defined.");

        var twilioAuthenticationToken = Environment.GetEnvironmentVariable(EnvironmentVariable.TwilioAuthenticationToken)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.TwilioAuthenticationToken}` is not defined.");

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
