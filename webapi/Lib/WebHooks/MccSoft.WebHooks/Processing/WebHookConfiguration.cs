public class WebHookConfiguration
{
    /// <summary>
    /// Delay between checking is there any new web hooks to process
    /// </summary>
    public int PollingDelay { get; set; } = 10000;

    /// <summary>
    /// Delays to retry web hooks if it fails (StatusCode is not 200-299)
    /// </summary>
    public int[] SendingDelaysInMinutes { get; set; } =
        new[]
        {
            1,
            1,
            2,
            4,
            8,
            15,
            30,
            60,
            120,
            300,
            600,
            1400,
            2100,
            2800,
            4200,
            5000,
            6000,
            7000,
            8000,
            9000,
            10000
        };
}
