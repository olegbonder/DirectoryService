using System.Text.Json.Serialization;

namespace SharedKernel.PaginationAndOrder
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderDirection
    {
        Asc,
        Desc
    }
}