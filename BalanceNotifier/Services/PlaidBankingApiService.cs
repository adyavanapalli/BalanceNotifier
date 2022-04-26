using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using BalanceNotifier.Constants;
using BalanceNotifier.Models.Banking;
using Microsoft.Extensions.Logging;
using Polly;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="IBankingApiService" /> using the Plaid API.
/// </summary>
public class PlaidBankingApiService : IBankingApiService
{
    /// <summary>
    /// An HTTP client for making HTTP requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// A logger used for logging information.
    /// </summary>
    private readonly ILogger<PlaidBankingApiService> _logger;

    /// <summary>
    /// A service used to manage secrets.
    /// </summary>
    private readonly ISecretsService _secretsService;

    /// <summary>
    /// A Plaid client access token used to make Plaid API requests related to a specific Plaid Item.
    /// </summary>
    private readonly string _plaidClientAccessToken;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="httpClient">An HTTP client for making HTTP requests.</param>
    /// <param name="logger">A logger used for logging information.</param>
    /// <param name="secretsService">A service used to manage secrets.</param>
    public PlaidBankingApiService(HttpClient httpClient,
                                  ILogger<PlaidBankingApiService> logger,
                                  ISecretsService secretsService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _secretsService = secretsService ?? throw new ArgumentNullException(nameof(secretsService));

        _plaidClientAccessToken = _secretsService.GetSecret(SecretVariable.PlaidClientAccessToken)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientAccessToken}` is not defined.");

        var plaidClientId = _secretsService.GetSecret(SecretVariable.PlaidClientId)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientId}` is not defined.");

        var plaidClientSecret = _secretsService.GetSecret(SecretVariable.PlaidClientSecret)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientSecret}` is not defined.");

        _httpClient.BaseAddress = new(BaseUrl.Plaid);
        _httpClient.DefaultRequestHeaders.Add(RequestHeader.PlaidClientId, plaidClientId);
        _httpClient.DefaultRequestHeaders.Add(RequestHeader.PlaidClientSecret, plaidClientSecret);

        _logger.LogInformation("[{Source}] Exiting constructor.", nameof(PlaidBankingApiService));
    }

    /// <inheritdoc />
    public async Task<Container> GetAccountBalancesAsync()
    {
        _logger.LogInformation("[{Source}] Attempting to get account balances.", nameof(GetAccountBalancesAsync));

        var response = await Policy.HandleResult<HttpResponseMessage>(httpResponseMessage => !httpResponseMessage.IsSuccessStatusCode)
                                   .RetryAsync(3,
                                               (context, _) =>
                                               {
                                                   _logger.LogInformation("[{Source}] Request returned with response:{NewLine}{Response}",
                                                                          Environment.NewLine,
                                                                          nameof(GetAccountBalancesAsync),
                                                                          context.Result);
                                               })
                                   .ExecuteAsync(() =>
                                   {
                                       return _httpClient.PostAsJsonAsync("accounts/balance/get",
                                                                          new { access_token = _plaidClientAccessToken });
                                   });

        _logger.LogInformation("[{Source}] Request returned with response:{NewLine}{Response}",
                               Environment.NewLine,
                               nameof(GetAccountBalancesAsync),
                               response);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<Container>();
    }
}
