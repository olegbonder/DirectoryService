using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderDirection
    {
        Asc,
        Desc
    }