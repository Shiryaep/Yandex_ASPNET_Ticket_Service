using YaEvents.Domain.Exceptions;

namespace YaEvents.Domain;

/// <summary> Event entity </summary> 
public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    /// <summary>
    /// Check Available seats and try to reserve them
    /// </summary>
    /// <param name="count">Seats to reserve</param>
    /// <returns>True if there are avalible seats and false if no</returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats - count >= 0)
        {
            AvailableSeats -= count;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Release reserved seats
    /// </summary>
    /// <param name="count">Seats to release</param>
    public void ReleaseSeats(int count = 1)
    {
        if (TotalSeats >= AvailableSeats + count)
        {
            AvailableSeats += count;
        }
        else
        {
            AvailableSeats = TotalSeats;
        }
    }

    private Event() //Only for EF
    {
        Title = null!;
    }

    private Event(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public static Event Create(
        string? title,
        string? description,
        DateTime? startAt,
        DateTime? endAt,
        int? totalSeats)
    {
        ValidateModelFields(title, startAt, endAt, totalSeats);

        return new Event(Guid.NewGuid(), title!.Trim(), description, startAt!.Value, endAt!.Value, totalSeats!.Value);
    }

    public void Update(
       string? title,
       string? description,
       DateTime? startAt,
       DateTime? endAt)
    {
        ValidateModelFields(title, startAt, endAt, TotalSeats);

        Title = title!;
        Description = description;
        StartAt = startAt!.Value;
        EndAt = endAt!.Value;
    }

    private static void ValidateModelFields(string? title,
        DateTime? startAt,
        DateTime? endAt,
        int? totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("There is no Title!");

        if (!startAt.HasValue)
            throw new ValidationException("There is no StartAt!");

        if (!endAt.HasValue)
            throw new ValidationException("There is no EndAt!");

        if (startAt < DateTime.UtcNow)
            throw new ValidationException("Event cannot start in the past");

        if (endAt <= startAt)
            throw new ValidationException("EndAt must be later than StartAt.");

        if (!totalSeats.HasValue || totalSeats.Value <= 0)
            throw new ValidationException("Total Seats count must be more than zero");
    }
}