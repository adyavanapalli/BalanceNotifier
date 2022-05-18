using BalanceNotifier.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BalanceNotifier.Startup))]

namespace BalanceNotifier;

/// <summary>
/// The startup class for the function host.
/// </summary>
public class Startup : FunctionsStartup
{
    /// <summary>
    /// Configures the function host's request pipeline.
    /// </summary>
    /// <param name="functionsHostBuilder">The function host builder.</param>
    public override void Configure(IFunctionsHostBuilder functionsHostBuilder)
    {
        functionsHostBuilder.Services.AddHttpClient()
                                     .AddSingleton<ISecretsService, KeyVaultSecretsService>()
                                     .AddSingleton<ISmsApiService, TwilioSmsApiService>()
                                     .AddSingleton<IStorageService, StorageAccountTableService>()
                                     .AddSingleton<IBankingApiService, PlaidBankingApiService>();
    }
}
