namespace GustavoTech;

#nullable enable

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; }

    public string? FromEmail { get; set; } = "letscooktime.com";
}