namespace Calligraphy.Services
{
    //轉台灣時區用的
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo TaipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

        //DateTime轉台灣時間
        public static DateTime GetTaipeiTimeNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
        }

        //DateTimeOffset轉台灣時間
        public static DateTimeOffset GetTaipeiTimeNowOffset(DateTimeOffset input)
        {
            return TimeZoneInfo.ConvertTime(input, TaipeiTimeZone);
        }
    }
}
