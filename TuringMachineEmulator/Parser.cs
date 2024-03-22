namespace TuringMachineEmulator;

public class Parser
{
    public class ParseException : Exception
    {
        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidDirectionException : ParseException
    {
        public InvalidDirectionException() { }
        public InvalidDirectionException(string message) : base(message) { }
        public InvalidDirectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidSymbolException : ParseException
    {
        public InvalidSymbolException() { }
        public InvalidSymbolException(string message) : base(message) { }
        public InvalidSymbolException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static TuringMachine Parse(string filename)
    {
        using StreamReader file = File.OpenText(filename);
        return Parse(file);
    }

    public static TuringMachine Parse(StreamReader stream)
    {
        string tape = NextNonEmptyLine(stream);
        string initialState = NextNonEmptyLine(stream);
        int initialPosition = int.Parse(NextNonEmptyLine(stream));
        List<Command> commands = ParseCommands(stream);

        return new()
        {
            Tape = tape,
            CurrentState = initialState,
            commands = commands,
            CurrentPosition = initialPosition,
        };
    }

    public static List<Command> ParseCommands(StreamReader stream)
    {
        List<Command> commands = [];
        string? line;

        while ((line = NextNonEmptyLineOptional(stream)) != null)
        {
            commands.Add(ParseCommand(line));
        }

        return commands;
    }

    public static Command ParseCommand(string data)
    {
        string[] tokens = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new Command(
            CurrentState: tokens[0],
            CurrentSymbol: ParseSymbol(tokens[1]),
            NewSymbol: ParseSymbol(tokens[2]),
            Direction: ParseDirection(tokens[3]),
            NewState: tokens[4]
        );
    }

    public static char ParseSymbol(string data)
    {
        if (data.Length > 1) throw new InvalidSymbolException();

        return data[0];
    }

    public static TuringMachine.Direction ParseDirection(string data)
    {
        return data.ToLower() switch
        {
            "l" or "left" or "<" => TuringMachine.Direction.L,
            "r" or "right" or ">" => TuringMachine.Direction.R,
            _ => throw new InvalidDirectionException(),
        };
    }

    private static string NextNonEmptyLine(StreamReader stream) =>
        NextNonEmptyLineOptional(stream) ?? throw new EndOfStreamException();

    private static string? NextNonEmptyLineOptional(StreamReader stream)
    {
        string? line;

        while ((line = stream.ReadLine()) is not null)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

            return line;
        }

        return null;
    }
}
