namespace GustavoTech.Implementation;

public static class Extensions
{
    public static (string cssClass, string text) GetElapsedTimeDisplay(this TimeSpan timespan)
    {
        var daysAgo = Math.Floor(timespan.TotalDays);
        var weeksAgo = Math.Floor(timespan.TotalDays / 7);
        if (daysAgo <= 3)
        {
            return ("text-success", "New!");
        }
        else if (daysAgo < 14)
        {
            return ("text-secondary", $"{daysAgo} days ago");
        }
        else
        {
            return ("text-secondary", $"{weeksAgo} weeks ago");
        }
    }
}