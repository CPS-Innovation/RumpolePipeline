using Ardalis.SmartEnum;

namespace Common.Logging;

public sealed class LoggingEvent : SmartEnum<LoggingEvent>
{
    public static readonly LoggingEvent SubmissionSucceeded = new LoggingEvent("Submission Succeeded", 1000);
    public static readonly LoggingEvent SubmissionFailed = new LoggingEvent("Submission Failed", 1001);
    public static readonly LoggingEvent ProcessingSucceeded = new LoggingEvent("Processing Succeeded", 1002);
    public static readonly LoggingEvent ProcessingFailedInvalidData = new LoggingEvent("Processing Failed - Invalid Data", 1003);
    public static readonly LoggingEvent ProcessingFailedUnhandledException = new LoggingEvent("Processing Failed - Unhandled Exception", 1004);
    
    public LoggingEvent(string name, int value) : base(name, value)
    {
    }
}