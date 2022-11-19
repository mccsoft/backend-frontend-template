//Taken from: https://lachlanbarclay.net/2018/02/throttling-your-api-in-asp-dot-net
//source code: https://gist.github.com/rocklan/8a20c52431efe083603f9f0f2a18e6f3#file-throttler-cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MccSoft.WebApi.Throttler;

public class Throttler
{
    /// <summary>
    /// Request will be defined by such unique entity, called ThrottleGroup.
    /// </summary>
    public string ThrottleGroup { get; set; }

    /// <summary>
    /// Defines amount of requests in timeout after request will be throttled.
    /// </summary>
    private int RequestLimit { get; set; }

    private readonly ConcurrentDictionary<string, ThrottleInfo> _cache =
        new ConcurrentDictionary<string, ThrottleInfo>();

    private readonly TimeSpan _timeout;

    public Throttler(string key, int requestLimit, TimeSpan timeout)
    {
        RequestLimit = requestLimit;
        _timeout = timeout;
        ThrottleGroup = key;
    }

    /// <summary>
    /// Determines whether current request should be throttled or not.
    /// </summary>
    public bool ShouldRequestBeThrottled(out ThrottleInfo throttleInfo)
    {
        throttleInfo = GetThrottleInfoFromCache();
        return (throttleInfo.RequestCount >= RequestLimit);
    }

    /// <summary>
    /// Increment request count for current <see cref="ThrottleGroup"/>.
    /// </summary>
    public void IncrementRequestCount()
    {
        _cache.AddOrUpdate(
            ThrottleGroup,
            new ThrottleInfo { ExpiresAt = DateTimeOffset.UtcNow.Add(_timeout), RequestCount = 1 },
            (retrievedKey, throttleInfo) =>
            {
                throttleInfo.RequestCount++;
                return throttleInfo;
            }
        );
    }

    /// <summary>
    /// Returns limit-headers for request.
    /// </summary>
    public Dictionary<string, string> GetRateLimitHeaders()
    {
        ThrottleInfo throttleInfo = GetThrottleInfoFromCache();

        int requestsRemaining = Math.Max(RequestLimit - throttleInfo.RequestCount, 0);

        var headers = new Dictionary<string, string>
        {
            { "X-RateLimit-Limit", RequestLimit.ToString() },
            { "X-RateLimit-Remaining", requestsRemaining.ToString() },
            { "X-RateLimit-Reset", throttleInfo.ExpiresAt.ToUnixTimeSeconds().ToString() }
        };
        return headers;
    }

    private ThrottleInfo GetThrottleInfoFromCache()
    {
        _cache.TryGetValue(ThrottleGroup, out ThrottleInfo throttleInfo);

        if (throttleInfo == null || throttleInfo.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throttleInfo = new ThrottleInfo
            {
                ExpiresAt = DateTimeOffset.UtcNow.Add(_timeout),
                RequestCount = 0
            };
            _cache[ThrottleGroup] = throttleInfo;
        }

        return throttleInfo;
    }

    public class ThrottleInfo
    {
        public DateTimeOffset ExpiresAt { get; set; }

        public int RequestCount { get; set; }
    }
}
