namespace TuringMachineEmulator;

public class SerializableTuringMachine
{
    public string Tape { get; set; } = "";
    public string State { get; set; } = "";
    public int Position { get; set; }
    public List<Command> Commands { get; set; } = [];

    public int Steps { get; set; }
    public char EmptyCharacter { get; set; } = TuringMachine.DEFAULT_EMPTY_CHAR;
    public int ChunkSize { get; set; } = TuringMachine.DEFAULT_CHUNK_SIZE;

    public TuringMachine ToTuringMachine()
    {
        return new TuringMachine(
            initialState: State,
            initialTape: Tape,
            initialPosition: Position,
            chunkSize: ChunkSize,
            emptyChar: EmptyCharacter)
        {
            commands = Commands,
        };
    }
}
