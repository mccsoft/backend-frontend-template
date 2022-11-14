using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class WebHookSender : IWebHookSender
{
    private readonly DbContext _dbContext;

    public WebHookSender(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendWebHook(
        string method,
        string url,
        string? body = null,
        Dictionary<string, string>? headers = null
    )
    {
        var webHook = new WebHook(method, url, body, headers);
        _dbContext.WebHooks().Add(webHook);
        await _dbContext.SaveChangesAsync();
    }
}
