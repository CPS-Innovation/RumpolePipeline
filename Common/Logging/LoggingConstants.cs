namespace Common.Logging;

public class LoggingConstants
{
    // Template for consisted structured logging across multiple functions, each field is described below: 
    // CorrelationId: Unique identifier of the message that can be processed by more than one component. 
    // EventDescription is a short description of the Event being logged. 
    // EntityType: Business Entity Type being processed.
    // EntityId: Id of the custom item being processed by the business entity - caseId or documentId, for example
    // Status: Status of the Log Event, e.g. Succeeded, Failed, Discarded.
    // Description: A detailed description of the log event or custom message. 
    public const string StructuredTemplate = "{CorrelationId}, {EventDescription}, {EntityType}, {TargetId}, {Status}, {Description}";
}