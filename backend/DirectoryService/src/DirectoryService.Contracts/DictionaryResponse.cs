namespace DirectoryService.Contracts
{
    public record DictionaryResponse(IReadOnlyList<DictionaryItemResponse> Items);

    public record DictionaryItemResponse(Guid Id, string Name);
}