namespace TuringMachineEmulator;

public class TuringMachine
{
    public enum Direction { L, R }; // Left, Right

    public enum Status { Ready, NoInstruction, TapeBorder }
    public Status status { get; private set; } = Status.Ready;

    // Information about current state
    private List<char> tape;
    public string Tape { get => new string(tape.ToArray()); }
    private string currentState;
    public string CurrentState => currentState;

    private int currentPosition;
    public int CurrentPosition => currentPosition;

    private int steps = 0;
    public int Steps => steps;

    public char currentSymbol
    {
        get
        {
            for (int i = tape.Count; i < currentPosition + 1; i++)
                tape.Add('\0');
            return tape[currentPosition];
        }
        private set
        {
            for (int i = tape.Count; i < currentPosition + 1; i++)
                tape.Add('\0');
            tape[currentPosition] = value;
        }
    }

    private List<Command> commands = new List<Command>();

    public bool Step()
    {
        if (status == Status.Ready)
        {
            // Find command with current state and symbol
            Command command = commands.Find(
                x => (x.CurrentState == currentState && x.CurrentSymbol == currentSymbol)
                );

            // If found the command...
            if (command != null)
            {

                // Execute command
                currentSymbol = command.NewSymbol;
                currentState = command.NewState;
                int newPosition = currentPosition + (command.Direction == Direction.L ? -1 : 1);
                if (newPosition >= 0 && newPosition < tape.Count)
                {
                    currentPosition = newPosition;
                }
                else
                {
                    status = Status.TapeBorder;
                    return false;
                }
                steps++;

                return true;
            }
            else // Halt
            {
                status = Status.NoInstruction;
                return false;
            }
        }
        else
            return false;
    }

    #region Parsing
    // Parsing info
    public string Path { get; private set; }

    public string FileName { get; private set; }

    public class ParseError
    {
        public readonly string message;
        public readonly ParseError causedBy;

        public ParseError(string message, ParseError causedBy = null)
        {
            this.message = message;
            this.causedBy = causedBy;
        }
    }

    public ParseError ReadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            return new ParseError("File doesn't exist.");
        }

        string[] fileLines = File.ReadAllLines(path);
        int lineIndex = 0;
        ParseError parseError;
        commands.Clear();

        this.Path = path;
        this.FileName = System.IO.Path.GetFileName(path);

        // Read tape
        parseError = ReadTape(fileLines, ref tape, ref lineIndex);
        if (parseError != null)
        {
            return new ParseError("Error while reading TAPE from file.", parseError);
        }

        // Read current position
        parseError = ReadCurrentPosition(fileLines, ref currentPosition, ref lineIndex);
        if (parseError != null)
        {
            return new ParseError("Error while reading CURRENT POSITION from file.", parseError);
        }

        // Read command list
        parseError = ReadCommandList(fileLines, ref commands, ref lineIndex);
        if (parseError != null)
        {
            return new ParseError("Error while reading COMMAND LIST from file.", parseError);
        }

        // Reset other parameters
        currentState = "0";

        // Success
        return null;
    }


    private static ParseError ReadTape(string[] fileLines, ref List<char> tape, ref int lineIndex)
    {
        SkipImptyLines(fileLines, ref lineIndex);

        if (lineIndex >= fileLines.Length)
        {
            return new ParseError("End of file reached before TAPE was readed.");
        }

        tape = new List<char>(fileLines[lineIndex]);
        lineIndex++;
        return null;
    }

    private static ParseError ReadCurrentPosition(string[] fileLines, ref int currentPosition, ref int lineIndex)
    {
        SkipImptyLines(fileLines, ref lineIndex);

        if (lineIndex >= fileLines.Length)
        {
            return new ParseError("End of file reached before CURRENT POSITION was readed.");
        }

        try
        {
            currentPosition = Convert.ToInt32(fileLines[lineIndex]) - 1;
        }
        catch (System.FormatException)
        {
            return new ParseError("Incorrect format for CURRENT POSITION (int): \"" + fileLines[lineIndex] + "\"");
        }

        lineIndex++;
        return null;
    }

    private static ParseError ReadCommandList(string[] filelines, ref List<Command> commands, ref int lineIndex)
    {
        commands = new List<Command>();

        while (lineIndex < filelines.Length)
        {
            SkipImptyLines(filelines, ref lineIndex);
            if (lineIndex >= filelines.Length) // eof
                break;

            string line = filelines[lineIndex];

            Command command = null;
            ParseError commandParseError = Command.ParseCommand(line, ref command);

            if (commandParseError == null)
            {
                commands.Add(command);
            }
            else
            {
                return new ParseError(
                    "Error while processing line " + (lineIndex + 1).ToString() + ": \"" + line + "\"",
                    commandParseError);
            }

            lineIndex++;
        }

        return null;
    }

    private static void SkipImptyLines(string[] filelines, ref int lineIndex)
    {
        for (; lineIndex < filelines.Length && string.IsNullOrWhiteSpace(filelines[lineIndex]); lineIndex++) { }
    }

    #endregion

    public void PrintCommandList()
    {
        Console.WriteLine("Printing this Turing Machine's command list:");

        foreach (Command com in commands)
        {
            Console.WriteLine(com.ToString());
        }
    }

    public void WriteLineCurrentState()
    {
        // Print name
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" Name: " + "UNNAMED123123123".PadLeft(15, ' ').Substring(0, 15) + ' ');

        // Space
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(' ');

        // Print current state
        Console.BackgroundColor = ConsoleColor.Cyan;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" State: " + currentState.PadLeft(15, ' ').Substring(0, 15) + ' ');

        // Space
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(' ');

        // Print current position
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.Write(" Pos: " + currentPosition.ToString().PadLeft(15, ' ').Substring(0, 15) + ' ');

        // New line
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write('\n');

        // Tape
        PrintTape(20);

        // New line
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write('\n');
    }

    public void PrintTape(int size)
    {
        if (size < 1)
            return;

        int pos = currentPosition;
        int tapeLen = tape.Count;

        int leftPos = currentPosition - size;
        int rightPos = currentPosition + size;

        if (rightPos > tapeLen - 1)
        {
            // Move to left
            int offset = rightPos - (tapeLen - 1);
            rightPos -= offset;
            leftPos -= offset;
        }

        if (leftPos < 0)
        {
            // Move to right
            int offset = -leftPos;
            rightPos += offset;
            leftPos += offset;
        }

        // Left border
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        if (leftPos == 0)
            Console.Write("   ");
        else
            Console.Write("...");

        // Characters before current position
        for (int i = leftPos; i < currentPosition; i++)
            Console.Write(tape[i]);

        // Character at current position
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write(tape[currentPosition]);

        // Characters after current position
        Console.BackgroundColor = ConsoleColor.Black;
        for (int i = currentPosition + 1; i < rightPos + 1 && i < tapeLen; i++)
            Console.Write(tape[i]);

        // Right border
        if (rightPos >= tapeLen - 1)
            Console.Write("   ");
        else
            Console.Write("...");
    }

}
