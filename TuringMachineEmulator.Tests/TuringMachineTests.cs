namespace TuringMachineEmulator.Tests;

public class TuringMachineTests
{
    [Fact]
    public void ReadFromFile_InitializesMachine()
    {
        TuringMachine tm = new();
        var error = tm.ReadFromFile("Samples/SampleMachine1.txt");

        Assert.Null(error);
    }

    [Fact]
    public void Step_UpdatesTape()
    {
        TuringMachine tm = new();
        Assert.Null(tm.ReadFromFile("Samples/SampleMachine1.txt"));

        for (int i = 0; i < 16; i++)
        {
            tm.Step();
        }

        Assert.Equal(16, tm.Steps);
        Assert.Equal("0100_", tm.Tape);
        Assert.Equal(2, tm.CurrentPosition);
        Assert.Equal("0", tm.CurrentState);
    }

    [Fact]
    public void Step_StopsWhenReachesEnd()
    {
        TuringMachine tm = new();
        Assert.Null(tm.ReadFromFile("Samples/SampleMachine1.txt"));

        for (int i = 0; i < 1000; i++)
        {
            tm.Step();
        }

        Assert.Equal(60, tm.Steps);
        Assert.Equal("0000_", tm.Tape);
        Assert.Equal(0, tm.CurrentPosition);
        Assert.Equal("1", tm.CurrentState);
        Assert.Equal(TuringMachine.Status.TapeBorder, tm.status);
    }

    [Fact]
    public void Step_WorksWithSampleMachine2()
    {
        TuringMachine tm = new();
        Assert.Null(tm.ReadFromFile("Samples/SampleMachine2.txt"));

        for (int i = 0; i < 2000; i++)
        {
            tm.Step();
        }

        Assert.Equal(1165, tm.Steps);
        Assert.Equal("M00000000N00000034P", tm.Tape);
        Assert.Equal(8, tm.CurrentPosition);
        Assert.Equal("H", tm.CurrentState);
        Assert.Equal(TuringMachine.Status.NoInstruction, tm.status);
    }
}
