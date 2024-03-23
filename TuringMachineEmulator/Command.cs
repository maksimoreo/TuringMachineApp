using System.Text.Json;
using System.Text.Json.Serialization;

namespace TuringMachineEmulator;

[JsonConverter(typeof(CommandJsonConverter))]
public record class Command(
    string CurrentState,
    char CurrentSymbol,
    char NewSymbol,
    Direction Direction,
    string NewState)
{
    public override string ToString()
    {
        string directionText = Direction == Direction.Left ? "L" : "R";
        return $"{CurrentState} {CurrentSymbol} {NewSymbol} {directionText} {NewState}";
    }
}

public class CommandJsonConverter : JsonConverter<Command>
{
    public override Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? s = reader.GetString();
        ArgumentException.ThrowIfNullOrWhiteSpace(s, nameof(s));
        return Parser.ParseCommand(s);
    }

    public override void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}