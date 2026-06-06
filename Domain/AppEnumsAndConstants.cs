namespace Domain;

///<summary> Enum of Booking Statuses </summary>
public enum BookingStatus
{
    ///<summary> Booking In Progress </summary>
    Pending,
    ///<summary> Booking Finished </summary>
    Confirmed,
    ///<summary> Booking Failed </summary>
    Rejected
}

public static class AppConstants
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
}
