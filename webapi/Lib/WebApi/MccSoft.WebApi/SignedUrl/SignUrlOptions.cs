using System;

namespace MccSoft.WebApi.SignedUrl;

public class SignUrlOptions
{
    /// <summary>
    /// Secret to sign Url tokens
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Interval during which the generated token will be valid
    /// </summary>
    public TimeSpan TokenDuration { get; set; } = TimeSpan.FromMinutes(20);
}
