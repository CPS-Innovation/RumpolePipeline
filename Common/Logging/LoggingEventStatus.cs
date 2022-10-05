using Ardalis.SmartEnum;

namespace Common.Logging;

public class LoggingEventStatus : SmartEnum<LoggingEventStatus>
{
    public static readonly LoggingEventStatus Succeeded = new LoggingEventStatus("Succeeded", 0);
    public static readonly LoggingEventStatus Failed = new LoggingEventStatus("Failed", 1);
    public static readonly LoggingEventStatus Discarded = new LoggingEventStatus("Discarded", 2);
    public static readonly LoggingEventStatus Ignored = new LoggingEventStatus("Ignored", 3);
    
    public LoggingEventStatus(string name, int value) : base(name, value)
    {
    }
}