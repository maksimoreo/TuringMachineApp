using System.Text.Json;

namespace TuringMachineEmulator.Tests;

public class TuringMachineTests
{
    [Fact]
    public void TuringMachine_CreatesWithSpecifiedTape()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789",
            initialPosition: 7,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("0123456789", tm.FullTape);
        Assert.Equal(7, tm.Position);
    }
    [Fact]
    public void TuringMachine_WhenInitialTapeIsLargerThanChunkSize_CreatesWithSpecifiedTape()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "012345678901234567890123456789",
            initialPosition: 13,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("012345678901234567890123456789", tm.FullTape);
        Assert.Equal(13, tm.Position);
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenLeftDoesNotExceedChunk()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789",
            initialPosition: 7,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("4567", tm.ExtractTapeAroundCursor(left: 3, right: 0));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenLeftSpans2Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "012345678901234567890123456789",
            initialPosition: 15,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("789012345", tm.ExtractTapeAroundCursor(left: 8, right: 0));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenLeftSpans3Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789012345678901234567890123456789",
            initialPosition: 22,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("234567890123456789012", tm.ExtractTapeAroundCursor(left: 20, right: 0));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenLeftSpans4Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "01234567890123456789012345678901234567890123456789",
            initialPosition: 31,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("890123456789012345678901", tm.ExtractTapeAroundCursor(left: 23, right: 0));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenLeftSpansIntoEmptyChunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789",
            initialPosition: 4,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal(".............................01234", tm.ExtractTapeAroundCursor(left: 33, right: 0));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenRightDoesNotExceedChunk()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789",
            initialPosition: 7,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("789", tm.ExtractTapeAroundCursor(left: 0, right: 2));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenRightSpans2Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "012345678901234567890123456789",
            initialPosition: 4,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("4567890123", tm.ExtractTapeAroundCursor(left: 0, right: 9));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenRightSpans3Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "012345678901234567890123456789",
            initialPosition: 6,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("6789012345678901234", tm.ExtractTapeAroundCursor(left: 0, right: 18));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenRightSpans4Chunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "01234567890123456789012345678901234567890123456789",
            initialPosition: 2,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("23456789012345678901234567890123", tm.ExtractTapeAroundCursor(left: 0, right: 31));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WhenRightSpansIntoEmptyChunks()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789",
            initialPosition: 7,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("789........................", tm.ExtractTapeAroundCursor(left: 0, right: 26));
    }

    [Fact]
    public void ExtractTapeAroundCursor_WithPartialInitialTape()
    {
        TuringMachine tm = new(
            initialState: "INITIAL",
            initialTape: "0123456789012",
            initialPosition: 6,
            chunkSize: 10,
            emptyChar: '.');

        Assert.Equal("..............0123456789012..............", tm.ExtractTapeAroundCursor(20));
    }

    [Fact]
    public void Step_SampleMachine1_10Steps()
    {
        TuringMachine tm = Parser.Parse("Samples/SampleMachine1.txt");

        for (int i = 0; i < 10; i++)
        {
            tm.Step();
        }

        Assert.Multiple(
            () => Assert.Equal(10, tm.Steps),
            () => Assert.Equal("0000010_000", tm.ExtractTapeAroundCursor(5)),
            () => Assert.Equal(-2, tm.Position),
            () => Assert.Equal("1", tm.State)
        );
    }

    [Fact]
    public void Step_SampleMachine1_100Steps()
    {
        TuringMachine tm = Parser.Parse("Samples/SampleMachine1.txt");

        for (int i = 0; i < 100; i++)
        {
            tm.Step();
        }

        Assert.Multiple(
            () => Assert.Equal(100, tm.Steps),
            () => Assert.Equal("0011011_0000000", tm.ExtractTapeAroundCursor(7)),
            () => Assert.Equal(0, tm.Position),
            () => Assert.Equal("0", tm.State)
        );
    }

    [Fact]
    public void Step_WorksWithSampleMachine2()
    {
        TuringMachine tm = Parser.Parse("Samples/SampleMachine2.txt");

        for (int i = 0; i < 2000; i++)
        {
            tm.Step();
        }

        Assert.Multiple(
            () => Assert.Equal(1165, tm.Steps),
            () => Assert.Equal("0000000M00000000N00000034P00000", tm.ExtractTapeAroundCursor(15)),
            () => Assert.Equal(8, tm.Position),
            () => Assert.Equal("H", tm.State)
        );
    }

    [Fact]
    public void WorksWith_BusyBeaver3Json()
    {
        TuringMachine tm =
            JsonSerializer
            .Deserialize<SerializableTuringMachine>(File.ReadAllText("Samples/BusyBeaver3.json"))!
            .ToTuringMachine();

        for (int i = 0; i < 14; i++)
        {
            tm.Step();
        }

        Assert.Equal(13, tm.Steps);
        Assert.Equal("00011111100", tm.ExtractTapeAroundCursor(left: 5, right: 5));
        Assert.Equal(-1, tm.Position);
        Assert.Equal("HALT", tm.State);
    }
}
