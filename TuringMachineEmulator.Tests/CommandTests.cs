namespace TuringMachineEmulator.Tests;

public class CommandTests
{
    [Fact]
    public void ToString_ReturnsState()
    {
        Command command = new Command("0", 'A', 'B', TuringMachine.Direction.R, "1");
        Assert.Equal("0 A B R 1", command.ToString());
    }

    [Fact]
    public void Static_ParseCommand_ReturnsParsedCommand()
    {
        Command command = null;
        var error = Command.ParseCommand("1111 A B L 2222", ref command);

        Assert.Equal("1111", command.CurrentState);
        Assert.Equal('A', command.CurrentSymbol);
        Assert.Equal('B', command.NewSymbol);
        Assert.Equal(TuringMachine.Direction.L, command.Direction);
        Assert.Equal("2222", command.NewState);

        Assert.Null(error);
    }

    [Fact]
    public void Static_ParseCommand_WhenNotEnoughTokens_ReturnsError()
    {
        Command command = null;
        var error = Command.ParseCommand("1111 A B L", ref command);

        Assert.NotNull(error);
        Assert.Equal("Expected to see 5 tokens, got 4.", error.message);
    }
}