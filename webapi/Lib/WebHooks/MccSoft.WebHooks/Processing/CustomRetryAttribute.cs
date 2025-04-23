using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

public class CustomRetryAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    private readonly AutomaticRetryAttribute _automaticRetryAttribute;

    private static IEnumerable<int> _delayIntervalsInSeconds = [];

    public static IEnumerable<int> GetDelayIntervals()
    {
        return _delayIntervalsInSeconds;
    }

    public static void SetDelayIntervals(IEnumerable<int> delayIntervalsInMinutes)
    {
        _delayIntervalsInSeconds = delayIntervalsInMinutes.Select(x => x * 60);
    }

    public CustomRetryAttribute()
    {
        _automaticRetryAttribute = new AutomaticRetryAttribute() { };
    }

    public void OnStateElection(ElectStateContext context)
    {
        _automaticRetryAttribute.Attempts = _delayIntervalsInSeconds.Count();
        _automaticRetryAttribute.DelaysInSeconds = _delayIntervalsInSeconds.ToArray();
        // NOTE: It doesn't work with AttemptsExceededAction.Fail
        _automaticRetryAttribute.OnAttemptsExceeded = AttemptsExceededAction.Delete;
        _automaticRetryAttribute.OnlyOn = [typeof(HttpRequestException)];

        _automaticRetryAttribute.OnStateElection(context);
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        _automaticRetryAttribute.OnStateApplied(context, transaction);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        _automaticRetryAttribute.OnStateUnapplied(context, transaction);
    }
}
