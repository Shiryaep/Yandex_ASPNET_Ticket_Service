namespace YaContracts;

public class Constants
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    public const int MaxBookingsPerUser = 10;

    public const string BookingConfirmedTopicName = "booking-confirmed";
    public const string BookingCancelledTopicName = "booking-cancelled";

    public const string TopEventsCacheKey = "events:top10";
    public const string GetEventByIdCacheKey = "event:";
}
