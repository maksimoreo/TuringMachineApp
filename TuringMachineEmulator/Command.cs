namespace TuringMachineEmulator;

class Command
{
    private string currentState;
    public string CurrentState => currentState;

    private char currentSymbol;
    public char CurrentSymbol => currentSymbol;

    private char newSymbol;
    public char NewSymbol => newSymbol;

    private TuringMachine.Direction direction;
    public TuringMachine.Direction Direction => direction;

    private string newState;
    public string NewState => newState;

    public override string ToString()
    {
        return currentState + ' ' + currentSymbol + ' ' + newSymbol + ' ' + direction.ToString() + ' ' + newState;
    }

    #region Command Parsing
    public static TuringMachine.ParseError ParseCommand(string s, ref Command command)
    {
        string[] tokenArray = s.Split();

        if (tokenArray.Length < 5)
        {
            return new TuringMachine.ParseError("Expected to see 5 tokens, got " + tokenArray.Length + '.');
        }

        command = new Command();
        TuringMachine.ParseError commandElementParseError = null;

        int commandIndex;
        for (commandIndex = 0; commandIndex < 5; commandIndex++)
        {
            string currentToken = tokenArray[commandIndex];

            switch (commandIndex)
            {
                case 0:
                    // Current state
                    commandElementParseError = ParseState(currentToken, out command.currentState);
                    break;
                case 1:
                    // Current Symbol
                    commandElementParseError = ParseSymbol(currentToken, out command.currentSymbol);
                    break;
                case 2:
                    // New Symbol
                    commandElementParseError = ParseSymbol(currentToken, out command.newSymbol);
                    break;
                case 3:
                    // Direction
                    commandElementParseError = ParseDirection(currentToken, out command.direction);
                    break;
                case 4:
                    // New State
                    commandElementParseError = ParseState(currentToken, out command.newState);
                    break;
            }

            // If catched error during parsing...
            if (commandElementParseError != null)
            {
                break;
            }
        }

        if (commandElementParseError == null) // Success
        {
            return null;
        }
        else // Fail
        {
            return new TuringMachine.ParseError("Error occured while parsing command element (" + (commandIndex + 1).ToString() + ").", commandElementParseError);
        }
    }
    #endregion

    #region Token Parsing

    private static TuringMachine.ParseError ParseState(string parseString, out string state)
    {
        state = parseString;
        return null;
    }

    private static TuringMachine.ParseError ParseSymbol(string parseString, out char symbol)
    {
        if (parseString.Length == 1)
        {
            symbol = parseString[0];
            return null;
        }
        else
        {
            symbol = '?';
            return new TuringMachine.ParseError("Multiple simbols found when reading symbol: \"" + parseString + "\"");
        }
    }

    private static TuringMachine.ParseError ParseDirection(string parseString, out TuringMachine.Direction direction)
    {
        switch (parseString)
        {
            case "L":
                direction = TuringMachine.Direction.L;
                return null;
            case "R":
                direction = TuringMachine.Direction.R;
                return null;
            default:
                direction = TuringMachine.Direction.L;
                return new TuringMachine.ParseError("Invalid direction symbol: \"" + parseString + "\"");
        }
    }
    #endregion
}
