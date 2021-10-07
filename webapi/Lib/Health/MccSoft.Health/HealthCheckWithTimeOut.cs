using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using Microsoft.Extensions.Options;

namespace MccSoft.Health
{
    public abstract class HealthCheckWithTimeOut : HealthCheck
    {
        private const int DefaultTimeoutInSeconds = 5;
        private readonly TimeSpan _timeout;

        public HealthCheckWithTimeOut(IOptions<HealthCheckSetting> options, string name)
            : base(name)
        {
            var settings = options.Value;
            if (settings == null || !settings.TimeoutInSeconds.HasValue)
            {
                // Settings or timeout was not specified use default one
                _timeout = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);
            }
            else
            {
                _timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds.Value);
            }
        }

        /// <summary>
        /// Called at each health check with cancelation token source
        /// that is configured with timeout.
        /// Should be overriden by all ancestors of this class.
        /// </summary>
        protected abstract ValueTask CheckImpl(CancellationTokenSource tokenSource);

        /// <summary>
        /// Executes CheckFunc on each health call with given timeout
        /// </summary>
        /// <param name="cancellationToken">Cancelation token usually default</param>
        /// <returns>
        /// Healty if CheckFunc finished without exceptions in given time frame.
        /// Otherwise Unhealthy.
        /// </returns>
        protected override async ValueTask<HealthCheckResult> CheckAsync(
            CancellationToken cancellationToken = default
        ) {
            var sw = new Stopwatch();

            try
            {
                using (
                    CancellationTokenSource tokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                ) {
                    tokenSource.CancelAfter(_timeout);

                    sw.Start();
                    await CheckImpl(tokenSource);
                    return HealthCheckResult.Healthy(
                        $"OK. {this.Name} was finished in {sw.ElapsedMilliseconds}ms."
                    );
                }
            }
            catch (OperationCanceledException)
            {
                TimeSpan elapsed = sw.Elapsed;

                // The overall timeout (coming from the caller of CheckAsync) may be shorter than
                // _timeout, in this case we report the real duration, not the threshold.
                TimeSpan timeout = _timeout > elapsed ? elapsed : _timeout;
                return HealthCheckResult.Unhealthy(
                    $"Health check for {this.Name} was timed out ({timeout})."
                );
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex);
            }
        }
    }
}
