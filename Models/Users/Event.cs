namespace babe_algorithms.Models.Users;

public class Event : IComparable<Event>
{
    [Key]
    public Guid Id { get; set; }

    public DateTime EventCreation { get; set; }

    public bool EventSeen { get; set; }

    public string Text { get; set; }

    public string Link { get; set; }

    public EventType Type { get; set; }

    public virtual ApplicationUser Creator { get; set; }

    public int CompareTo(Event other)
    {
        return this.EventCreation.CompareTo(other.EventCreation);
    }
}

public enum EventType
{
    Public,
    Private,
}