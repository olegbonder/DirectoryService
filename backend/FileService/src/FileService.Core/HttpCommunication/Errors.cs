using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.Result;

namespace FileService.Core.HttpCommunication;

[JsonConverter(typeof(ErrorsJsonConverter))]
public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors(IEnumerable<Error> errors)
    {
        _errors = [.. errors];
    }

    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator Errors(Error[] errors) => new(errors);

    public static implicit operator Errors(Error error) => new([error]);
}

public class ErrorsJsonConverter : JsonConverter<Errors>
{
    public override Errors? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options);
        return errors is null ? null : new Errors(errors);
    }

    public override void Write(Utf8JsonWriter writer, Errors value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}