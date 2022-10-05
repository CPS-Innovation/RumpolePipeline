using Ardalis.SmartEnum;

namespace Common.Logging;

public sealed class LoggingCheckpoint : SmartEnum<LoggingCheckpoint>
{
    public static readonly LoggingCheckpoint PipelineTriggered = new LoggingCheckpoint("Pipeline Triggered", 0);
    public static readonly LoggingCheckpoint PipelineRunCompleted = new LoggingCheckpoint("Pipeline Run Completed", 1);
    public static readonly LoggingCheckpoint PipelineRunFailed = new LoggingCheckpoint("Pipeline Run Failed", 2);
    public static readonly LoggingCheckpoint PipelineStatusCheck = new LoggingCheckpoint("Pipeline Status Check", 3);
    public static readonly LoggingCheckpoint CaseDocumentsRequested = new LoggingCheckpoint("Case Documents Requested", 4);
    public static readonly LoggingCheckpoint CaseDocumentsRetrieved = new LoggingCheckpoint("Case Documents Retrieved", 5);
    public static readonly LoggingCheckpoint DocumentConversionBegun = new LoggingCheckpoint("Document Conversion Begun", 6);
    public static readonly LoggingCheckpoint DocumentConversionCompleted = new LoggingCheckpoint("Document Conversion Completed", 7);
    public static readonly LoggingCheckpoint TextExtractionBegun = new LoggingCheckpoint("Text Extraction Begun", 8);
    public static readonly LoggingCheckpoint TextExtractionCompleted = new LoggingCheckpoint("Text Extraction Completed", 9);
    public static readonly LoggingCheckpoint SearchIndexUpdateBegun = new LoggingCheckpoint("Search Index Update Begun", 10);
    public static readonly LoggingCheckpoint SearchIndexUpdateCompleted = new LoggingCheckpoint("Search Index Update Completed", 11);
    public static readonly LoggingCheckpoint DocumentConversionFailed = new LoggingCheckpoint("Document Conversion Failed", 12);
    public static readonly LoggingCheckpoint TextExtractionFailed = new LoggingCheckpoint("Text Extraction Failed", 13);
    public static readonly LoggingCheckpoint SearchIndexUpdateFailed = new LoggingCheckpoint("Search Index Updated Failed", 14);
    public static readonly LoggingCheckpoint PipelineTaskFailed = new LoggingCheckpoint("Pipeline Task Failed", 15);

    private LoggingCheckpoint(string name, int value) : base(name, value)
    {
        
    }
}