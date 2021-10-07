using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MccSoft.WebApi.Serialization.DateTime
{
    /// <summary>
    /// This is a controller that allows to test [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))] attribute
    /// for POST and GET scenarios
    /// https://github.com/dotnet/aspnetcore/issues/11584#issuecomment-506007647
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DateTimeTestController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("ignoreTimezoneDate")]
        public string IgnoreTimezoneDate(
            [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))] System.DateTime date
        ) {
            return date.ToString();
        }

        public class IgnoreTimezoneDateDto
        {
            [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))]
            public System.DateTime Date { get; set; }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ignoreTimezoneDateInDto")]
        public string IgnoreTimezoneDateInDto([FromQuery] IgnoreTimezoneDateDto dto)
        {
            return dto?.Date.ToString();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("timezoneDate")]
        public string TimezonedDate(System.DateTime date)
        {
            return date.ToString();
        }

        public class IgnoreTimezonedDatePost
        {
            [JsonConverter(typeof(IgnoreTimezoneDateTimeConverter))]
            public System.DateTime DateTime { get; set; }
        }

        public class TimezonedDatePost
        {
            public System.DateTime DateTime { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ignoreTimezoneDate")]
        public string IgnoreTimezoneDate(IgnoreTimezonedDatePost date)
        {
            return date.DateTime.ToString();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("timezoneDate")]
        public string TimezonedDate(TimezonedDatePost date)
        {
            return date.DateTime.ToString();
        }
    }
}
