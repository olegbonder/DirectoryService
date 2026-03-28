namespace FileService.Domain.MediaProcessing;

public enum StepType
{
    INITIALIZATE,
    EXTRACT_METADATA,
    GENERATE_HLS,
    UPLOAD_HLS,
    GENERATE_PREVIEW,
    CLEANUP
}