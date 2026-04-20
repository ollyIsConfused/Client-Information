namespace ClientInformation.Data.Models;

public class ClientEntry
{
    public Guid     Id         { get; set; } = Guid.NewGuid();
    public string   Name       { get; set; } = string.Empty;
    public string   Notes      { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
