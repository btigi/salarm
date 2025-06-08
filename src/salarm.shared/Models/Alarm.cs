namespace salarm.shared.models;

public class Alarm
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TriggerTime { get; set; }
    public string? SoundFilePath { get; set; }
    public string? Message { get; set; }
    public bool IsTriggered { get; set; }
}