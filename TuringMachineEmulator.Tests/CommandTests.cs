namespace TuringMachineEmulator.Tests;

public class CommandTests
{
    [Fact]
    public void ToString_ReturnsState()
    {
        Command command = new Command("0", 'A', 'B', TuringMachine.Direction.R, "1");
        Assert.Equal("0 A B R 1", command.ToString());
    }
}