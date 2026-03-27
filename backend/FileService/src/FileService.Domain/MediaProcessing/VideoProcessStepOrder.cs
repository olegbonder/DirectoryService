using FileService.Domain.Shared;
using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing;

public sealed record VideoProcessStepOrder
{
    private VideoProcessStepOrder(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<VideoProcessStepOrder> Create(int order)
    {
        if (order <= 0)
        {
            return VideoProcessStepErrors.OrderMustBePositive();
        }

        return new VideoProcessStepOrder(order);
    }
}