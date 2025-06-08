
namespace salarm.shared.models
{
    public class Command
    {
        public string Action { get; set; } = "";
        public Alarm? Alarm { get; set; }
        public Guid? AlarmId { get; set; }
    }
}