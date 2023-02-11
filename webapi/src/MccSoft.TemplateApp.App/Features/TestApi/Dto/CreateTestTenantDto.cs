namespace MccSoft.TemplateApp.App.Features.TestApi;

public record CreateTestTenantDto
{
    public string UserEmail { get; set; }
    public string UserPassword { get; set; }
}
