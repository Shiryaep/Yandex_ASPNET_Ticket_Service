namespace Yandex_ASPNET_Ticket_Service.Models;

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