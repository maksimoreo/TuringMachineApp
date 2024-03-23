using System.Text;

namespace TuringMachineEmulator;

public class TuringMachine
{
    public const int DEFAULT_CHUNK_SIZE = 1024;
    public const char DEFAULT_EMPTY_CHAR = '\0';

    public enum Status { Ready, NoInstruction, TapeBorder }
    public Status status { get; private set; } = Status.Ready;

    public string CurrentState { get; set; }
    public int CurrentPosition { get; set; }

    public string FullTape { get => string.Join("", ChunkedTape.Select(chunk => new string(chunk))); }
    public LinkedList<char[]> ChunkedTape { get; } = new LinkedList<char[]>();
    public LinkedListNode<char[]> TapeNode { get; private set; }

    public int Steps { get; set; }

    public readonly int ChunkSize;
    public readonly char EmptyChar;

    public int LocalPosition { get; private set; }
    public int TapeNodePosition { get; private set; }
    public int Position { get => TapeNodePosition * ChunkSize + LocalPosition; }

    public TuringMachine(
        string initialState,
        int chunkSize = DEFAULT_CHUNK_SIZE,
        char emptyChar = DEFAULT_EMPTY_CHAR)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentException("must be greater than 0", nameof(chunkSize));
        }

        ChunkSize = chunkSize;
        EmptyChar = emptyChar;

        CurrentState = initialState;

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

        CurrentState = initialState;

        if (initialTape.Length >= 1)
        {
            // First full chunks
            var chunks = Enumerable
                .Range(0, initialTape.Length / ChunkSize)
                .Select(i =>
                    initialTape
                        .Substring(i * ChunkSize, ChunkSize)
                        .ToCharArray()
                );

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

            ChunkedTape = new LinkedList<char[]>(chunks);
            TapeNode = ChunkedTape.First!;
        }
        else
        {
            TapeNode = ChunkedTape.AddFirst(CreateChunk());
        }

        TapeNodePosition = initialPosition / ChunkSize;
        LocalPosition = initialPosition % ChunkSize;

        for (int i = 0; i < TapeNodePosition; i++)
            TapeNode = GetOrCreateNext(TapeNode);
    }

    public char CurrentSymbol
    {
        get => TapeNode.Value[LocalPosition];
        private set => TapeNode.Value[LocalPosition] = value;
    }

    public List<Command> commands = [];

    public bool Step()
    {
        if (status != Status.Ready) return false;

        // Find command with current state and symbol
        Command? command = commands.Find(x => x.CurrentState == CurrentState && x.CurrentSymbol == CurrentSymbol);

        if (command is null)
        {
            status = Status.NoInstruction;
            return false;
        }

        // Execute command
        CurrentSymbol = command.NewSymbol;
        CurrentState = command.NewState;
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

        Steps++;

        return true;
    }

    public string ExtractTapeAroundCursor(int extractInEachDirection) =>
        ExtractTapeAroundCursor(left: extractInEachDirection, right: extractInEachDirection);

    /// <summary>
    /// Extracts partial tape around cursor. Does not create additional chunks if steps out of existing chunks.
    /// </summary>
    /// <param name="left">How many characters to extract to the left</param>
    /// <param name="right">How many characters to extract to the right</param>
    /// <returns>Extracted tape part of size exactly `left + 1 + right`</returns>
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
            leftParts.Add(new string(TapeNode.Value[..LocalPosition]) ?? "");
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

            sb.Append(string.Join("", leftParts.AsEnumerable().Reverse()));
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

    public void MoveLeft(int steps)
    {
        throw new NotImplementedException();
    }

    private char[] CreateChunk() => Enumerable.Repeat(EmptyChar, ChunkSize).ToArray();
    private LinkedListNode<char[]> GetOrCreatePrevious(LinkedListNode<char[]> node) =>
        node.Previous ?? ChunkedTape.AddBefore(node, CreateChunk());
    private LinkedListNode<char[]> GetOrCreateNext(LinkedListNode<char[]> node) =>
            node.Next ?? ChunkedTape.AddAfter(node, CreateChunk());

}
