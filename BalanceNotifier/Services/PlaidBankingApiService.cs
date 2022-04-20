using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using BalanceNotifier.Constants;
using BalanceNotifier.Models.Banking;
using Microsoft.Extensions.Logging;

namespace BalanceNotifier.Services;

/// <summary>
/// Implements the interface specified in <see cref="IBankingApiService" /> using the Plaid API.
/// </summary>
public class PlaidBankingApiService : IBankingApiService
{
    /// <summary>
    /// The base URL of the Banking API.
    /// <para>
    /// TODO: This base URL needs to be eventually updated to a production API base URL.
    /// </para>
    /// </summary>
    private const string BASE_URL = "https://development.plaid.com";

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
    /// A Plaid client ID used to identify calls to the Plaid API.
    /// </summary>
    private readonly string _plaidClientId;

    /// <summary>
    /// A Plaid client secret used to authenticate calls to the Plaid API.
    /// </summary>
    private readonly string _plaidClientSecret;

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

        _httpClient.BaseAddress = new(BASE_URL);

        _plaidClientId = _secretsService.GetSecret(SecretVariable.PlaidClientId)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientId}` is not defined.");

        _plaidClientSecret = _secretsService.GetSecret(SecretVariable.PlaidClientSecret)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientSecret}` is not defined.");

        _plaidClientAccessToken = _secretsService.GetSecret(SecretVariable.PlaidClientAccessToken)
            ?? throw new ConfigurationErrorsException($"The secret variable `{SecretVariable.PlaidClientAccessToken}` is not defined.");

        _logger.LogInformation($"{nameof(PlaidBankingApiService)}: Exiting constructor.");
    }

    /// <inheritdoc />
    /// TODO: This call periodically fails, so it needs to be fortified in some way, perhaps using some
    /// resilience framework like Polly.
    public async Task<Container> GetAccountBalancesAsync()
    {
        var response = await _httpClient.PostAsJsonAsync("accounts/balance/get",
                                                         new
                                                         {
                                                             client_id = _plaidClientId,
                                                             secret = _plaidClientSecret,
                                                             access_token = _plaidClientAccessToken
                                                         });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<Container>();
    }
}
