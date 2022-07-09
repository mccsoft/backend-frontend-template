namespace MccSoft.TemplateApp.App.Setup;

/// <summary>
/// This class will not be touched during pulling changes from Template.
/// Consider putting your project-specific code here.
/// </summary>
public static partial class SetupAspNet
{
    static partial void AddProjectSpecifics(WebApplicationBuilder builder) { }

    static partial void UseProjectSpecifics(IApplicationBuilder app) { }
}
