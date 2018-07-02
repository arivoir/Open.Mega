using Open.Net.Http;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Mega
{
    public class MegaRetryFilter : RetryMessageHandler
    {
        private static Random _rand = new Random();

        public MegaRetryFilter(IHttpMessageHandlerFactory messageHandlerFactory = null, int retriesCount = 5)
            : base(messageHandlerFactory, retriesCount)
        {
        }

        public MegaRetryFilter(HttpMessageHandler innerFilter, int retriesCount = 5)
            : base(innerFilter, retriesCount)
        {
        }

        protected override async Task<TimeSpan?> ShouldRetry(HttpResponseMessage response, int retries, CancellationToken cancellationToken)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            int responseValue;
            if (int.TryParse(responseString, out responseValue))
            {
                if (responseValue == -3)
                {
                    var timeSpan = Math.Pow(2, retries) * 1000 + _rand.Next(0, 1000);
                    return TimeSpan.FromMilliseconds(timeSpan);
                }
            }
            return await base.ShouldRetry(response, retries, cancellationToken);
        }
    }
}
