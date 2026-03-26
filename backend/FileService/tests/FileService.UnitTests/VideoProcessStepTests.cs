using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests;

public class VideoProcessStepTests
{
    private const string ValidStepName = "Кодирование";
    private const int ValidOrder = 1;

    private VideoProcessStepName CreateValidName() => VideoProcessStepName.Create(ValidStepName).Value;

    private VideoProcessStepOrder CreateValidOrder() => VideoProcessStepOrder.Create(ValidOrder).Value;

    #region Создание VideoProcessStep

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = CreateValidName();
        var order = CreateValidOrder();

        // Act
        var step = new VideoProcessStep(name, order);

        // Assert
        Assert.NotEqual(Guid.Empty, step.Id);
        Assert.Equal(name, step.Name);
        Assert.Equal(order, step.Order);
        Assert.Equal(VideoProcessStatus.PENDING, step.Status);
        Assert.Equal(0, step.Progress.Value);
    }

    #endregion

    #region Start

    [Fact]
    public void Start_FromPending_ShouldTransitionToRunning()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Act
        var result = step.Start();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.RUNNING, step.Status);
        Assert.NotNull(step.StartedAt);
        Assert.Empty(step.Error);
    }

    [Fact]
    public void Start_FromNonPendingStatus_ShouldFail()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        step.Start(); // Переведем в RUNNING
        step.Complete(); // Переведем в SUCCEEDED

        // Act
        var result = step.Start();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("PENDING", result.Errors.First().Message);
    }

    [Theory]
    [InlineData(VideoProcessStatus.RUNNING)]
    [InlineData(VideoProcessStatus.SUCCEEDED)]
    [InlineData(VideoProcessStatus.FAILED)]
    public void Start_FromIncorrectStatus_ShouldFail(VideoProcessStatus invalidStatus)
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Используем рефлексию для установки статуса в тестовых целях
        var statusField = typeof(VideoProcessStep).GetProperty(nameof(VideoProcessStep.Status));
        var statusProperty = typeof(VideoProcessStep).GetProperty(nameof(VideoProcessStep.Status),
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Предварительно переведем в корректное состояние для нужного статуса
        if (invalidStatus == VideoProcessStatus.RUNNING)
        {
            step.Start();
        }
        else if (invalidStatus == VideoProcessStatus.SUCCEEDED)
        {
            step.Start();
            step.Complete();
        }
        else if (invalidStatus == VideoProcessStatus.FAILED)
        {
            step.Start();
            step.Fail("Ошибка");
        }

        // Act
        var result = step.Start();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("PENDING", result.Errors.First().Message);
    }

    #endregion

    #region SetProgress

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void SetProgress_WithValidValues_ShouldUpdateProgress(double percent)
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Act
        var result = step.SetProgress(percent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(percent, step.Progress.Value);
    }

    [Fact]
    public void SetProgress_FromPendingStatus_ShouldTransitionToRunning()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Act
        var result = step.SetProgress(50);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.RUNNING, step.Status);
        Assert.Equal(50, step.Progress.Value);
    }

    [Fact]
    public void SetProgress_OnCompletedStep_ShouldFail()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        step.Start();
        step.Complete();

        // Act
        var result = step.SetProgress(50);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("SUCCEEDED", result.Errors.First().Message);
    }

    #endregion

    #region Complete

    [Fact]
    public void Complete_FromRunningStatus_ShouldTransitionToSucceeded()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        step.Start();

        // Act
        var result = step.Complete();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.SUCCEEDED, step.Status);
        Assert.Equal(100, step.Progress.Value);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void Complete_FromNonRunningStatus_ShouldFail()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Act
        var result = step.Complete();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("RUNNING", result.Errors.First().Message);
    }

    #endregion

    #region Fail

    [Fact]
    public void Fail_FromRunningStatus_WithMessage_ShouldTransitionToFailed()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        var errorMessage = "Ошибка кодирования";
        step.Start();

        // Act
        var result = step.Fail(errorMessage);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(VideoProcessStatus.FAILED, step.Status);
        Assert.Equal(errorMessage, step.Error);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void Fail_WithEmptyMessage_ShouldFail()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        step.Start();

        // Act
        var result = step.Fail("");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("required", result.Errors.First().Message);
    }

    [Fact]
    public void Fail_FromNonRunningStatus_ShouldFail()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());

        // Act
        var result = step.Fail("Ошибка");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("RUNNING", result.Errors.First().Message);
    }

    #endregion

    #region Reset

    [Fact]
    public void Reset_FromAnyStatus_ShouldReturnToPending()
    {
        // Arrange
        var step = new VideoProcessStep(CreateValidName(), CreateValidOrder());
        step.Start();
        step.SetProgress(50);

        // Act
        step.Reset();

        // Assert
        Assert.Equal(VideoProcessStatus.PENDING, step.Status);
        Assert.Null(step.StartedAt);
        Assert.Null(step.CompletedAt);
    }

    #endregion
}