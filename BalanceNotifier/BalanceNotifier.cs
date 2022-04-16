using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BalanceNotifier
{
    /// <summary>
    /// Contains the entry point for this Azure Function App that notifies a user what their bank balance is every
    /// morning.
    /// </summary>
    public class BalanceNotifier
    {
        /// <summary>
        /// An HTTP client used for making HTTP requests.
        /// </summary>
        private readonly static HttpClient _httpClient = new();

        /// <summary>
        /// The entry point for this Azure Function App invocation.
        /// </summary>
        /// <param name="timerInfo">An object that provides access to timer schedule information for whenever this
        /// function is triggered.</param>
        /// <param name="logger">A logger object for logging information.</param>
        [FunctionName(nameof(BalanceNotifier))]
        public void Run([TimerTrigger("0 0 12 * * *")] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation($"Timer trigger function started execution at: {DateTime.UtcNow}.");

            // TODO: This URI is safe and non-private information. It serves as a placeholder and will later be removed.
            _httpClient.GetAsync("https://webhook.site/6d2bf0e3-7845-4636-8dd3-0f4d66d6c70d");
        }
    }
}
