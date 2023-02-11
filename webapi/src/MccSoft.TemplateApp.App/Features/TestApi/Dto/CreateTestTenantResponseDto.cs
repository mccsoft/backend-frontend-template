namespace MccSoft.TemplateApp.App.Features.TestApi;

public class CreateTestTenantResponseDto
{
    public CreateTestTenantResponseDto(int tenantId)
    {
        TenantId = tenantId;
    }

    public int TenantId { get; set; }
}
