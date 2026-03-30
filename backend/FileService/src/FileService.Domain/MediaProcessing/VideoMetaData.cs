using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing
{
    public class VideoMetaData
    {
        public TimeSpan Duration { get; }

        public int Width { get; }

        public int Height { get; }

        private VideoMetaData(TimeSpan duration, int width, int height)
        {
            Duration = duration;
            Width = width;
            Height = height;
        }

        public static Result<VideoMetaData> Create(TimeSpan duration, int width, int height)
        {
            if (duration <= TimeSpan.Zero)
                return GeneralErrors.ValueIsRequired("duration");

            return new VideoMetaData(duration, width, height);
        }
    }
}