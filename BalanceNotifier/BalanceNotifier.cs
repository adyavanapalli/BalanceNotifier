using System;
using System.Linq;
using System.Threading.Tasks;
using BalanceNotifier.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BalanceNotifier;

/// <summary>
/// Contains the entry point for this Azure Function App that notifies a user what their bank balance is every
/// morning.
/// </summary>
public class BalanceNotifier
{
    /// <summary>
    /// A NCRON expression for everyday at 8 AM ET.
    /// <para>
    /// TODO: Note that this will not be correct when daylight savings is not active. Also note that there is an
    /// existing TODO to fix this at TODO[1] in main.tf.
    /// </para>
    /// </summary>
    private const string EverydayAt8AmEt = "0 0 12 * * *";

    /// <summary>
    /// A service used for getting banking information via a banking API.
    /// </summary>
    private readonly IBankingApiService _bankingApiService;

    /// <summary>
    /// A service used for sending SMS messages via an SMS API.
    /// </summary>
    private readonly ISmsApiService _smsApiService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="bankingApiService">A service used for getting banking information via a banking API.</param>
    /// <param name="smsApiService">A service used for sending SMS messages via an SMS API.</param>
    public BalanceNotifier(IBankingApiService? bankingApiService = null, ISmsApiService? smsApiService = null)
    {
        _bankingApiService = bankingApiService ?? new PlaidBankingApiService();
        _smsApiService = smsApiService ?? new TwilioSmsApiService();
    }

    /// <summary>
    /// The entry point for this Azure Function App invocation.
    /// </summary>
    /// <param name="timerInfo">An object that provides access to timer schedule information for whenever this
    /// function is triggered.</param>
    /// <param name="logger">A logger object for logging information.</param>
    [FunctionName(nameof(BalanceNotifier))]
    public async Task Run([TimerTrigger(EverydayAt8AmEt)] TimerInfo timerInfo, ILogger logger)
    {
        logger.LogInformation($"Timer trigger function started execution at: {DateTime.UtcNow}.");

        var container = await _bankingApiService.GetAccountBalancesAsync();

        var depositoryAccountBalance = container?.Accounts
                                                ?.FirstOrDefault(account => account.Type == "depository")
                                                ?.Balances
                                                ?.Current;

        var creditCardAccountBalance = container?.Accounts
                                                ?.FirstOrDefault(account => account.Type == "credit")
                                                ?.Balances
                                                ?.Current;

        await _smsApiService.SendMessageAsync($"[ ${depositoryAccountBalance} | (${creditCardAccountBalance}) ]");
    }
}
