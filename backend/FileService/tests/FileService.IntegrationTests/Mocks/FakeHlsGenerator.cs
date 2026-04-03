using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Mocks
{
    public class FakeHlsGenerator : IFfmpegProcessRunner
    {
        private readonly TimeSpan _defaultDuration;
        private readonly int _defaultWidth;
        private readonly int _defaultHeight;
        private readonly bool _shouldFail;
        private readonly string? _failureMessage;
        private readonly int _segmentCount;

        public FakeHlsGenerator(
            TimeSpan? duration = null,
            int? width = null,
            int? height = null,
            bool shouldFail = false,
            string? failureMessage = null,
            int segmentCount = 10)
        {
            _defaultDuration = duration ?? TimeSpan.FromMinutes(5);
            _defaultWidth = width ?? 1920;
            _defaultHeight = height ?? 1080;
            _shouldFail = shouldFail;
            _failureMessage = failureMessage ?? "FFmpeg process failed";
            _segmentCount = segmentCount;
        }

        public Task<Result<VideoMetaData>> ExtractMetadataAsync(
            string inputFileUrl,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldFail)
            {
                return Task.FromResult(Result<VideoMetaData>.Failure(
                    Error.Failure("ffmpeg.metadata.extraction.failed", _failureMessage)));
            }

            var metadataResult = VideoMetaData.Create(_defaultDuration, _defaultWidth, _defaultHeight);
            return Task.FromResult(metadataResult.IsSuccess
                ? Result<VideoMetaData>.Success(metadataResult.Value)
                : Result<VideoMetaData>.Failure(metadataResult.Errors));
        }

        public Task<Result> GenerateHlsAsync(
            string inputFileUrl,
            string outputDirectory,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldFail)
            {
                return Task.FromResult(Result.Failure(
                    Error.Failure("ffmpeg.hls.generation.failed", _failureMessage)));
            }

            // Создаём директорию, если не существует
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Генерируем фейковые HLS-файлы
            GenerateFakeHlsFiles(outputDirectory);

            return Task.FromResult(Result.Success());
        }

        private void GenerateFakeHlsFiles(string outputDirectory)
        {
            // Генерируем master playlist
            var masterPlaylistPath = Path.Combine(outputDirectory, VideoAsset.MASTER_PLAYLIST_NAME);
            var masterPlaylistContent = GenerateMasterPlaylistContent();
            File.WriteAllText(masterPlaylistPath, masterPlaylistContent);

            // Генерируем stream playlists для каждого качества (360p, 720p, 1080p)
            var qualities = new[] { 360, 720, 1080 };
            foreach (var quality in qualities)
            {
                var streamPlaylistPath = Path.Combine(outputDirectory, $"{quality}_stream.m3u8");
                var streamPlaylistContent = GenerateStreamPlaylistContent(quality);
                File.WriteAllText(streamPlaylistPath, streamPlaylistContent);

                // Генерируем фейковые TS-сегменты
                for (int i = 0; i < _segmentCount; i++)
                {
                    var segmentPath = Path.Combine(outputDirectory, $"{quality}_{i:D6}.ts");
                    var segmentContent = GenerateFakeTsSegment();
                    File.WriteAllBytes(segmentPath, segmentContent);
                }
            }
        }

        private string GenerateMasterPlaylistContent()
        {
            return """
#EXTM3U
#EXT-X-VERSION:3

#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=640x360,NAME="360p"
360_stream.m3u8

#EXT-X-STREAM-INF:BANDWIDTH=1400000,RESOLUTION=1280x720,NAME="720p"
720_stream.m3u8

#EXT-X-STREAM-INF:BANDWIDTH=2500000,RESOLUTION=1920x1080,NAME="1080p"
1080_stream.m3u8
""";
        }

        private string GenerateStreamPlaylistContent(int quality)
        {
            double segmentDuration = 4.0; // 4 секунды на сегмент
            double totalDuration = _segmentCount * segmentDuration;

            var content = new System.Text.StringBuilder();
            content.AppendLine("#EXTM3U");
            content.AppendLine("#EXT-X-VERSION:3");
            content.AppendLine("#EXT-X-TARGETDURATION:4");
            content.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");
            content.AppendLine("#EXT-X-PLAYLIST-TYPE:VOD");

            for (int i = 0; i < _segmentCount; i++)
            {
                content.AppendLine($"#EXTINF:{segmentDuration:F3},");
                content.AppendLine($"{quality}_{i:D6}.ts");
            }

            content.AppendLine("#EXT-X-ENDLIST");

            return content.ToString();
        }

        private byte[] GenerateFakeTsSegment()
        {
            // Создаём минимальный валидный TS-пакет (188 байт)
            // TS-пакеты начинаются с синхро байта 0x47
            var packet = new byte[188];

            // Заголовок TS-пакета
            packet[0] = 0x47; // Sync byte
            packet[1] = 0x40; // Payload unit start indicator + PID
            packet[2] = 0x00; // PID continued
            packet[3] = 0x10; // Continuity counter + adaptation field control

            // PAT (Program Association Table) - минимальная структура
            packet[4] = 0x00; // table_id
            packet[5] = 0xB0; // section_syntax_indicator + section_length
            packet[6] = 0x0D; // section_length
            packet[7] = 0x00; // transport_stream_id
            packet[8] = 0x01; // transport_stream_id
            packet[9] = 0xC1; // version_number + current_next_indicator
            packet[10] = 0x00; // section_number
            packet[11] = 0x00; // last_section_number
            packet[12] = 0x00; // program_number
            packet[13] = 0xE1; // PID MSB
            packet[14] = 0x00; // PID LSB

            // Заполняем остальное случайными данными для реалистичности
            var random = new Random();
            random.NextBytes(packet.AsSpan(15, 173));

            return packet;
        }
    }
}