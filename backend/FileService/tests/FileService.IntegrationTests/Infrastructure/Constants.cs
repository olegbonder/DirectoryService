namespace FileService.IntegrationTests.Infrastructure;

public static class Constants
{
    public const string TEST_FILE_DIRECTORY = "Resources";
    public const string TEST_VIDEO_FILE_NAME = "test-file.mp4";
    public const string TEST_IMAGE_FILE_NAME = "test-image.jpg";

    public const string BASE_URL = "/api";
    public const string START_MULTIPART_UPLOAD_URL = $"{BASE_URL}/files/multipart/start";
    public const string GET_CHUNK_UPLOAD_URL = $"{BASE_URL}/files/multipart/url";
    public const string COMPLETE_MULTIPART_UPLOAD_URL = $"{BASE_URL}/files/multipart/end";
    public const string ABORT_MULTIPART_UPLOAD_URL = $"{BASE_URL}/files/multipart/cancel";
    public const string GET_MEDIA_ASSET_INFO_URL = $"{BASE_URL}/files/";
    public const string GET_MEDIA_ASSETS_INFO_URL = $"{BASE_URL}/files/batch";
    public const string GET_DOWNLOAD_URL = $"{BASE_URL}/files/url";
    public const string UPLOAD_FILE_URL = $"{BASE_URL}/file/upload";
    public const string DOWNLOAD_FILE_URL = $"{BASE_URL}/file/";
    public const string DELETE_FILE_URL = $"{BASE_URL}/files/";
}