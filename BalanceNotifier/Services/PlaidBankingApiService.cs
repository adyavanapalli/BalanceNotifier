using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using BalanceNotifier.Constants;
using BalanceNotifier.Models.Banking;

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
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new(BASE_URL)
    };

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
    public PlaidBankingApiService()
    {
        _plaidClientId = Environment.GetEnvironmentVariable(EnvironmentVariable.PlaidClientId)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.PlaidClientId}` is not defined.");

        _plaidClientSecret = Environment.GetEnvironmentVariable(EnvironmentVariable.PlaidClientSecret)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.PlaidClientSecret}` is not defined.");

        _plaidClientAccessToken = Environment.GetEnvironmentVariable(EnvironmentVariable.PlaidClientAccessToken)
            ?? throw new ConfigurationErrorsException($"The environment variable `{EnvironmentVariable.PlaidClientAccessToken}` is not defined.");
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
