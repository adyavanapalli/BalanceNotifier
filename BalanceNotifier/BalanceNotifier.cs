using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
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
    /// An NCRON expression for every hour between 8 AM and 8 PM ET.
    /// <para>
    /// TODO: Note that this will not be correct when daylight savings is not active. Also note that there is an
    /// existing TODO to fix this at TODO[1] in main.tf.
    /// </para>
    /// </summary>
    private const string EveryHourBetween8AmAnd8PmEt = "0 0 12-23,0 * * *";

    /// <summary>
    /// An NCRON expression for every minute. This constant is provided for debugging purposes.
    /// </summary>
    private const string EveryMinute = "0 * * * * *";

    /// <summary>
    /// A service used for getting banking information via a banking API.
    /// </summary>
    private readonly IBankingApiService _bankingApiService;

    /// <summary>
    /// A logger used for logging information.
    /// </summary>
    private readonly ILogger<BalanceNotifier> _logger;

    /// <summary>
    /// A service used for sending SMS messages via an SMS API.
    /// </summary>
    private readonly ISmsApiService _smsApiService;

    /// <summary>
    /// A service used for managing storage data.
    /// </summary>
    private readonly IStorageService _storageService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="bankingApiService">A service used for getting banking information via a banking API.</param>
    /// <param name="logger">A logger used for logging information.</param>
    /// <param name="smsApiService">A service used for sending SMS messages via an SMS API.</param>
    /// <param name="storageService">A service used for managing storage data.</param>
    public BalanceNotifier(IBankingApiService bankingApiService,
                           ILogger<BalanceNotifier> logger,
                           ISmsApiService smsApiService,
                           IStorageService storageService)
    {
        _bankingApiService = bankingApiService ?? throw new ArgumentNullException(nameof(bankingApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smsApiService = smsApiService ?? throw new ArgumentNullException(nameof(smsApiService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

        _logger.LogInformation("[{Source}] Exiting constructor.", nameof(BalanceNotifier));
    }

    /// <summary>
    /// The entry point for this Azure Function App invocation.
    /// </summary>
    /// <para>
    /// This needs additional logging.
    /// </para>
    /// <param name="timerInfo">An object that provides access to timer schedule information for whenever this
    /// function is triggered.</param>
    /// <param name="logger">A logger object for logging information.</param>
    [FunctionName(nameof(BalanceNotifier))]
    public async Task Run([TimerTrigger(EveryMinute)] TimerInfo timerInfo)
    {
        _logger.LogInformation("[{Source}] Timer trigger function started execution at: {UtcNow}.",
                               nameof(Run),
                               DateTime.UtcNow);

        var container = await _bankingApiService.GetAccountBalancesAsync();

        var cultureInfo = CultureInfo.GetCultureInfo("en-US");

        var depositoryAccountBalance = container?.Accounts
                                                ?.FirstOrDefault(account => account.Type == "depository")
                                                ?.Balances
                                                ?.Current
                                                ?.ToString("C", cultureInfo);

        var creditCardAccountBalance = container?.Accounts
                                                ?.FirstOrDefault(account => account.Type == "credit")
                                                ?.Balances
                                                ?.Current
                                                ?.ToString("C", cultureInfo);

        _logger.LogInformation("[{Source}] Successfully obtained and parsed account balances [depository = {DepositoryAccountBalance:C} | {CreditCardAccountBalance:C}].",
                               nameof(Run),
                               depositoryAccountBalance,
                               creditCardAccountBalance);

        var tableEntity = await _storageService.GetTableEntity();

        var previousDepositoryAccountBalance = tableEntity?[nameof(depositoryAccountBalance)] as string;
        var previousCreditCardAccountBalance = tableEntity?[nameof(creditCardAccountBalance)] as string;

        // If the previous balances differ from the current ones, then only should we update the ones in storage and
        // additionally send a SMS message.
        if (depositoryAccountBalance != previousDepositoryAccountBalance ||
            creditCardAccountBalance != previousCreditCardAccountBalance)
        {
            await _storageService.StoreTableEntity(new TableEntity(string.Empty, string.Empty)
                                                   {
                                                       {nameof(depositoryAccountBalance), depositoryAccountBalance},
                                                       {nameof(creditCardAccountBalance), creditCardAccountBalance}
                                                   });

            await _smsApiService.SendMessageAsync($"[ {depositoryAccountBalance} | {creditCardAccountBalance} ]");
        }
    }
}
