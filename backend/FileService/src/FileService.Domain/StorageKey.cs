using SharedKernel.Result;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Key { get; }
    public string Prefix { get; }
    public string Bucket { get; }
    public string Value { get; }
    public string FullPath { get; }

    private StorageKey(string bucket, string key, string prefix)
    {
        Bucket = bucket;
        Key = key;
        Prefix = prefix;

        Value = string.IsNullOrEmpty(prefix) ? Key : $"{Prefix}/{Key}";
        FullPath = $"{Bucket}/{Value}";
    }

    public static Result<StorageKey> Create(string bucket, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return GeneralErrors.ValueIsRequired("bucket");
        }

        var normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
        {
            return normalizedKeyResult.Errors;
        }

        var normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
        {
            return normalizedPrefixResult.Errors;
        }

        return new StorageKey(bucket.Trim(), normalizedKeyResult.Value, normalizedPrefixResult.Value);
    }

    public Result<StorageKey> AppendSegment(string segment)
    {
        var normalizedKeyResult = NormalizeSegment(segment);
        if (normalizedKeyResult.IsFailure)
        {
            return normalizedKeyResult.Errors;
        }

        return new StorageKey(Bucket, normalizedKeyResult.Value, Value);
    }

    public static StorageKey None = new(string.Empty, string.Empty, string.Empty);

    private static Result<string> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return string.Empty;
        }

        string[] parts = prefix.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            var normalizedPart = NormalizeSegment(part);
            if (normalizedPart.IsFailure)
            {
                return normalizedPart.Value;
            }

            if (!string.IsNullOrWhiteSpace(normalizedPart.Value))
            {
                normalizedParts.Add(normalizedPart.Value);
            }
        }

        return string.Join("/", normalizedParts);
    }

    private static Result<string> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("key");
        }

        string trimmed = value.Trim();

        if (trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
        {
            return GeneralErrors.ValueIsRequired("key");
        }

        return trimmed;
    }
}