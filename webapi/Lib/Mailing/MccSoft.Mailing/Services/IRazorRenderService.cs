using System.Threading.Tasks;

namespace MccSoft.Mailing
{
    public interface IRazorRenderService
    {
        Task<string> RenderToStringAsync<T>(string viewName, T model);
    }
}
