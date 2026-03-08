using SharedKernel.Result;

namespace FileService.Domain;

public sealed record MediaOwner
{
    private static readonly HashSet<string> AllowedContexts = [
        "department",
        "user"
    ];
    public string Context { get; }
    public Guid EntityId { get; }

    // EF-Core
    private MediaOwner()
    {
    }

    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }

    public static Result<MediaOwner> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > 50)
            return GeneralErrors.ValueIsRequired(nameof(context));

        string normalizedContext = context.Trim().ToLowerInvariant();
        if (!AllowedContexts.Contains(normalizedContext))
            return GeneralErrors.ValueIsRequired(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsRequired(nameof(entityId));

        return new MediaOwner(context, entityId);
    }

    public static Result<MediaOwner> ForDepartment(Guid departmentId) => Create("department", departmentId);
}