using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MccSoft.Logging;

/// <summary>
/// Creates an activity and a corresponding logging scope to enrich the log messages
/// with the activity properties (like TraceId).
/// </summary>
internal class TopLevelActivityScope : IDisposable
{
    /// <summary>
    /// Stores SessionId, TraceId, SpanId and ParentId to be attached to log messages.
    /// </summary>
    private class TraceLoggingScope : IReadOnlyList<KeyValuePair<string, object>>
    {
        const int _activityFieldCount = 3;
        private readonly Activity _activity;
        private readonly List<KeyValuePair<string, object>> _additionalParams;

        public TraceLoggingScope(
            Activity activity,
            List<KeyValuePair<string, object>> additionalParams
        )
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            _activity = activity;
            _additionalParams = additionalParams;
        }

        /// <summary>
        /// Borrowed from
        /// https://github.com/dotnet/aspnetcore/blob/3af94f18cb841a27cc9e607f45b880df64201f70/src/Hosting/Hosting/src/Internal/HostingLoggerExtensions.cs#L96
        /// </summary>
        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return new KeyValuePair<string, object>(
                            "SpanId",
                            _activity.SpanId.ToHexString()
                        );
                    case 1:
                        return new KeyValuePair<string, object>(
                            "TraceId",
                            _activity.TraceId.ToHexString()
                        );
                    case 2:
                        return new KeyValuePair<string, object>(
                            "ParentId",
                            _activity.ParentSpanId.ToHexString()
                        );
                    default:
                        if (index < 0 || index > _activityFieldCount + _additionalParams.Count - 1)
                        {
                            throw new ArgumentOutOfRangeException(nameof(index));
                        }

                        return _additionalParams[index - _activityFieldCount];
                }
            }
        }

        public int Count => _activityFieldCount + _additionalParams.Count;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private readonly Activity _activityToRestore;
    private readonly Activity _activity;
    private readonly IDisposable _innerScope;

    public TopLevelActivityScope(
        ILogger logger,
        string activityName,
        string sessionId,
        List<KeyValuePair<string, object>> paramList
    )
    {
        _activityToRestore = Activity.Current;

        // We don't want to inherit a TraceId from the global root activity.
        Activity.Current = null;
        _activity = new Activity(activityName);
        if (sessionId != null)
        {
            _activity.AddBaggage(LoggingHeaders.SessionId, sessionId);
            paramList.Add(new KeyValuePair<string, object>(LoggingHeaders.SessionId, sessionId));
        }

        _activity.Start();
        _innerScope = logger.BeginScope(new TraceLoggingScope(_activity, paramList));
    }

    public void Dispose()
    {
        _innerScope?.Dispose();
        _activity.Stop();

        // Restore the previous (global) activity just in case something is logged by libraries
        // on this thread.
        Activity.Current = _activityToRestore;
    }
}
