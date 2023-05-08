using NodaTime;

namespace Tweed.GenerateFakes;

internal static class DateHelper
{
    internal static ZonedDateTime DateTimeToZonedDateTime(DateTime dateTime)
    {
        var localDate = LocalDateTime.FromDateTime(dateTime);
        var berlinTimeZone = DateTimeZoneProviders.Tzdb["UTC"];
        var timeZonedDateTime = berlinTimeZone.AtLeniently(localDate);
        return timeZonedDateTime;
    }
}
