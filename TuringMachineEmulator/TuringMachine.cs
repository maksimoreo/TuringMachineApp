using System.Text;

namespace TuringMachineEmulator;

public class TuringMachine
{
    public class NoCommandException : Exception
    {
        public NoCommandException() { }
        public NoCommandException(string message) : base(message) { }
        public NoCommandException(string message, Exception innerException) : base(message, innerException) { }
    }

    public const int DEFAULT_CHUNK_SIZE = 1024;
    public const char DEFAULT_EMPTY_CHAR = '\0';

    public string State { get; set; }

    /// <summary>
    /// Gets full tape. Used for testing/debugging. For UI, use ExtractTapeAroundCursor instead.
    /// </summary>
    public string FullTape => string.Join(string.Empty, ChunkedTape.Select(chunk => new string(chunk)));

    public LinkedList<char[]> ChunkedTape { get; }

    public LinkedListNode<char[]> TapeNode { get; private set; }

    public int Steps { get; set; }

    public int ChunkSize { get; init; }

    public char EmptyChar { get; init; }

    public int LocalPosition { get; private set; }

    public int TapeNodePosition { get; private set; }

    public int Position => TapeNodePosition * ChunkSize + LocalPosition;

    public TuringMachine(
        string initialState,
        int chunkSize = DEFAULT_CHUNK_SIZE,
        char emptyChar = DEFAULT_EMPTY_CHAR)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1, nameof(chunkSize));

        ChunkSize = chunkSize;
        EmptyChar = emptyChar;

        State = initialState;

        TapeNode = ChunkedTape.AddFirst(CreateChunk());
        ChunkedTape.AddFirst(CreateChunk());
    }

    public TuringMachine(
        string initialState,
        string initialTape,
        int initialPosition,
        int chunkSize = DEFAULT_CHUNK_SIZE,
        char emptyChar = DEFAULT_EMPTY_CHAR)
    {
        if (initialPosition < 0)
        {
            // TODO: Allow to specify negative position (to the left)
            throw new NotImplementedException("Currently does not work with negative initial position (TODO)");
        }

        ChunkSize = chunkSize;
        EmptyChar = emptyChar;

        State = initialState;
        ChunkedTape = CreateTape(initialTape);
        TapeNode = ChunkedTape.First!;

        TapeNodePosition = initialPosition / ChunkSize;
        LocalPosition = initialPosition % ChunkSize;

        for (int i = 0; i < TapeNodePosition; i++)
            TapeNode = GetOrCreateNext(TapeNode);
    }

    private LinkedList<char[]> CreateTape(string initialTape)
    {
        if (initialTape.Length == 0)
        {
            return new LinkedList<char[]>([CreateChunk()]);
        }

        // First full chunks
        var chunks = Enumerable
            .Range(0, initialTape.Length / ChunkSize)
            .Select(i => initialTape.Substring(i * ChunkSize, ChunkSize).ToCharArray());

        // Last partial chunk
        int lastChunkSize = initialTape.Length % ChunkSize;
        if (lastChunkSize != 0)
        {
            var partialChunk = initialTape[^lastChunkSize..];
            char[] chunk = CreateChunk();
            for (int i = 0; i < partialChunk.Length; i++)
            {
                chunk[i] = partialChunk[i];
            }

            chunks = chunks.Append(chunk);
        }

        return new LinkedList<char[]>(chunks);
    }

    public char CurrentSymbol
    {
        get => TapeNode.Value[LocalPosition];
        private set => TapeNode.Value[LocalPosition] = value;
    }

    public List<Command> Commands { get; init; } = [];

    public Command? FindNextCommand() => Commands.Find(x => x.CurrentState == State && x.CurrentSymbol == CurrentSymbol);

    public void Step(int times)
    {
        if (TryStep(times) != times) throw new NoCommandException();
    }

    public void Step()
    {
        if (!TryStep()) throw new NoCommandException();
    }

    public int TryStep(int times)
    {
        int i;
        for (i = 0; i < times && TryStep(); i++)
        {
        }

        return i;
    }

    public bool TryStep()
    {
        Command? command = FindNextCommand();
        if (command is null) return false;

        ExecuteCommand(command);
        Steps++;
        return true;
    }

    private void ExecuteCommand(Command command)
    {
        CurrentSymbol = command.NewSymbol;
        State = command.NewState;
        int newPosition = LocalPosition + (command.Direction == Direction.Left ? -1 : 1);
        if (newPosition < 0)
        {
            TapeNode = GetOrCreatePrevious(TapeNode);
            LocalPosition = ChunkSize - 1;
            TapeNodePosition -= 1;
        }
        else if (newPosition >= ChunkSize)
        {
            TapeNode = GetOrCreateNext(TapeNode);
            LocalPosition = 0;
            TapeNodePosition += 1;
        }
        else
        {
            LocalPosition = newPosition;
        }
    }

    public string ExtractTapeAroundCursor(int extractInEachDirection) =>
        ExtractTapeAroundCursor(left: extractInEachDirection, right: extractInEachDirection);

    /// <summary>
    /// Extracts partial tape around cursor. Does not create additional chunks if steps out of existing chunks.
    /// </summary>
    /// <param name="left">How many characters to extract to the left.</param>
    /// <param name="right">How many characters to extract to the right.</param>
    /// <returns>Extracted tape part of size exactly `left + 1 + right`.</returns>
    public string ExtractTapeAroundCursor(int left, int right)
    {
        StringBuilder sb = new();

        if (left <= LocalPosition)
        {
            sb.Append(new string(TapeNode.Value[(LocalPosition - left)..LocalPosition]));
        }
        else
        {
            // Reversed. As we traverse tape to the left, we add items to the end (right side) of this list.
            List<string> leftParts = [];

            // First partial chunk
            leftParts.Add(new string(TapeNode.Value[..LocalPosition]) ?? string.Empty);
            left -= LocalPosition;

            // Middle full chunks
            LinkedListNode<char[]>? currentNode = TapeNode.Previous;
            while (currentNode is not null && left >= ChunkSize)
            {
                leftParts.Add(new string(currentNode.Value));

                left -= ChunkSize;
                currentNode = currentNode.Previous;
            }

            if (left > 0)
            {
                if (currentNode is not null)
                {
                    // Last partial chunk
                    leftParts.Add(new string(currentNode.Value[(ChunkSize - left)..]));
                }
                else
                {
                    // Hit last left node in LinkedList. Don't create new nodes, just return new characters that would have been here, if cursor actually went here.
                    leftParts.Add(new string(EmptyChar, left));
                }
            }

            sb.Append(string.Join(string.Empty, leftParts.AsEnumerable().Reverse()));
        }

        sb.Append(CurrentSymbol);

        if (right < ChunkSize - LocalPosition)
        {
            sb.Append(new string(TapeNode.Value[(LocalPosition + 1)..(LocalPosition + right + 1)]));
        }
        else
        {
            // First partial chunk
            sb.Append(new string(TapeNode.Value[(LocalPosition + 1)..ChunkSize]));
            right -= ChunkSize - LocalPosition - 1;

            // Middle full chunks
            LinkedListNode<char[]>? currentNode = TapeNode.Next;
            while (currentNode is not null && right >= ChunkSize)
            {
                sb.Append(new string(currentNode.Value));

                right -= ChunkSize;
                currentNode = currentNode.Next;
            }

            if (right > 0)
            {
                if (currentNode is not null)
                {
                    // Last partial chunk
                    sb.Append(new string(currentNode.Value[..right]));
                }
                else
                {
                    // Hit last right node in LinkedList
                    sb.Append(new string(EmptyChar, right));
                }
            }
        }

        return sb.ToString();
    }

    private char[] CreateChunk() => Enumerable.Repeat(EmptyChar, ChunkSize).ToArray();

    private LinkedListNode<char[]> GetOrCreatePrevious(LinkedListNode<char[]> node) =>
        node.Previous ?? ChunkedTape.AddBefore(node, CreateChunk());

    private LinkedListNode<char[]> GetOrCreateNext(LinkedListNode<char[]> node) =>
        node.Next ?? ChunkedTape.AddAfter(node, CreateChunk());
}
