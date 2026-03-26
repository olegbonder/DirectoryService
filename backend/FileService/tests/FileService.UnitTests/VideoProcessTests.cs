using FileService.Domain;
using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests;

public class VideoProcessTests
{
    private const int ValidStepCount = 3;

    private StorageKey CreateValidRawKey() => StorageKey.Create("bucket", null, "raw-key").Value;

    private StorageKey CreateValidHlsKey() => StorageKey.Create("bucket", null, "hls-key").Value;

    private IEnumerable<VideoProcessStep> CreateValidSteps(int count = ValidStepCount)
    {
        var steps = new List<VideoProcessStep>();
        for (int i = 1; i <= count; i++)
        {
            var name = VideoProcessStepName.Create($"Шаг {i}").Value;
            var order = VideoProcessStepOrder.Create(i).Value;
            steps.Add(new VideoProcessStep(name, order));
        }

        return steps;
    }

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rawKey = CreateValidRawKey();
        var hlsKey = CreateValidHlsKey();
        var steps = CreateValidSteps();

        // Act
        var result = VideoProcess.Create(id, rawKey, hlsKey, steps);

        // Assert
        Assert.True(result.IsSuccess);
        var process = result.Value;
        Assert.Equal(id, process.Id);
        Assert.Equal(rawKey, process.RawKey);
        Assert.Equal(hlsKey, process.HlsKey);
        Assert.Equal(ValidStepCount, process.Steps.Count);
        Assert.Equal(VideoProcessStatus.PENDING, process.Status);
    }

    [Fact]
    public void Create_WithEmptySteps_ShouldFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rawKey = CreateValidRawKey();
        var hlsKey = CreateValidHlsKey();

        // Act
        var result = VideoProcess.Create(id, rawKey, hlsKey, Enumerable.Empty<VideoProcessStep>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("must have more one step", result.Errors.First().Message.ToLower());
    }

    [Fact]
    public void Create_WithDuplicateOrder_ShouldFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rawKey = CreateValidRawKey();
        var hlsKey = CreateValidHlsKey();
        var name1 = VideoProcessStepName.Create("Шаг 1").Value;
        var name2 = VideoProcessStepName.Create("Шаг 2").Value;
        var order = VideoProcessStepOrder.Create(1).Value;
        var steps = new List<VideoProcessStep>
        {
            new VideoProcessStep(name1, order),
            new VideoProcessStep(name2, order)
        };

        // Act
        var result = VideoProcess.Create(id, rawKey, hlsKey, steps);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("unique", result.Errors.First().Message.ToLower());
    }

    #endregion

    #region PrepareForExecution

    [Fact]
    public void PrepareForExecution_FromPending_ShouldTransitionToRunning()
    {
        // Arrange
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            CreateValidSteps()).Value;

        // Act
        var result = process.PrepareForExecution();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.RUNNING, process.Status);
    }

    [Fact]
    public void PrepareForExecution_AfterFailed_ShouldResetStepsAndRunAgain()
    {
        // Arrange
        var steps = CreateValidSteps().ToList();
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            steps).Value;

        process.PrepareForExecution();
        process.Fail("Первая ошибка");

        // Act
        var result = process.PrepareForExecution();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.RUNNING, process.Status);
        // Шаги должны быть сброшены
        foreach (var step in process.Steps)
        {
            Assert.Equal(VideoProcessStatus.PENDING, step.Status);
        }
    }

    [Fact]
    public void PrepareForExecution_FromCanceled_ShouldFail()
    {
        // Arrange
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            CreateValidSteps()).Value;

        process.PrepareForExecution();
        process.Cancel("Отмена");

        // Act
        var result = process.PrepareForExecution();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("CANCELED", result.Errors.First().Message);
    }

    #endregion

    #region HappyPath

    [Fact]
    public void HappyPath_PrepareForExecution_StartAllSteps_CompleteAllSteps_FinishProcessing_ShouldSucceed()
    {
        // Arrange
        var steps = CreateValidSteps().ToList();
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            steps).Value;

        // Act & Assert - PrepareForExecution
        var prepareResult = process.PrepareForExecution();
        Assert.True(prepareResult.IsSuccess);
        Assert.Equal(VideoProcessStatus.RUNNING, process.Status);

        // Act & Assert - Start all steps
        for (int i = 1; i <= ValidStepCount; i++)
        {
            var startStepName = $"Шаг {i}";
            var startResult = process.StartStep(i, startStepName);
            Assert.True(startResult.IsSuccess);
        }

        // Act & Assert - Report progress for each step
        for (int i = 0; i < ValidStepCount; i++)
        {
            var progressResult = process.ReportStepProgress(100);
            Assert.True(progressResult.IsSuccess);

            if (i < ValidStepCount - 1)
            {
                // Завершим текущий шаг
                var completeResult = process.CompleteStep(i + 1);
                Assert.True(completeResult.IsSuccess);

                // Начнем следующий
                if (i + 2 <= ValidStepCount)
                {
                    var nextStartResult = process.StartStep(i + 2, $"Шаг {i + 2}");
                    Assert.True(nextStartResult.IsFailure);
                }
            }
            else
            {
                // Завершим последний шаг
                var completeResult = process.CompleteStep(ValidStepCount);
                Assert.True(completeResult.IsSuccess);
            }
        }

        // Assert - проверим финальный статус
        Assert.True(process.Steps.All(s => s.Status == VideoProcessStatus.SUCCEEDED));

        var finishResult = process.FinishProcessing();
        Assert.True(finishResult.IsSuccess);
        Assert.Equal(VideoProcessStatus.SUCCEEDED, process.Status);
        Assert.Equal(100, process.TotalProgress);
    }

    #endregion

    #region Fail handling

    [Fact]
    public void Fail_WithMessageOnRunningProcess_ShouldSucceed()
    {
        // Arrange
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            CreateValidSteps()).Value;

        process.PrepareForExecution();
        var errorMessage = "Критическая ошибка кодирования";

        // Act
        var result = process.Fail(errorMessage);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.FAILED, process.Status);
        Assert.Equal(errorMessage, process.ErrorMessage);
    }

    #endregion

    #region TotalProgress

    [Fact]
    public void ReportStepProgress_ShouldCalculateTotalProgressCorrectly()
    {
        // Arrange
        var steps = CreateValidSteps(4).ToList();
        var process = VideoProcess.Create(
            Guid.NewGuid(),
            CreateValidRawKey(),
            CreateValidHlsKey(),
            steps).Value;

        process.PrepareForExecution();

        // Начнем первый шаг
        process.StartStep(1, "Шаг 1");

        // Act & Assert - доведем прогресс до конца и завершим шаги
        for (int i = 1; i <= 4; i++)
        {
            var progressResult = process.ReportStepProgress(100);
            Assert.True(progressResult.IsSuccess);

            if (i < 4)
            {
                var completeResult = process.CompleteStep(i);
                Assert.True(completeResult.IsSuccess);

                // Начнем следующий шаг
                var nextStartResult = process.StartStep(i + 1, $"Шаг {i + 1}");
                Assert.True(nextStartResult.IsSuccess);

                // Проверим расчет общего прогресса: i_завершено / 4_всего * 100
                double expectedProgress = i / 4.0 * 100;
                Assert.Equal(expectedProgress, process.TotalProgress, 2);
            }
        }

        // Завершим последний шаг
        var lastCompleteResult = process.CompleteStep(4);
        Assert.True(lastCompleteResult.IsSuccess);

        // Assert - после завершения всех шагов прогресс должен быть 100
        Assert.Equal(100, process.TotalProgress);
    }

    #endregion
}
