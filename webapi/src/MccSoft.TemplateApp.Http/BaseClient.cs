using System;
using System.Threading.Tasks;

namespace MccSoft.TemplateApp.Http
{
    public class BaseClient
    {
        protected void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings)
        {
            // settings.NullValueHandling = NullValueHandling.Ignore;
        }

        // public async Task<TResult> CallWithIncludeNulls<TResult>(Func<Task<TResult>> func)
        // {
        //     try
        //     {
        //         ChangeNullValueHandling(NullValueHandling.Include);
        //         return await func();
        //     }
        //     finally
        //     {
        //         ChangeNullValueHandling(NullValueHandling.Ignore);
        //     }
        // }
        //
        // public void ChangeNullValueHandling(NullValueHandling nullValueHandling)
        // {
        //     var baseClient = (IBaseClient)this;
        //     baseClient.JsonSerializerSettings.NullValueHandling = nullValueHandling;
        // }
    }
}
