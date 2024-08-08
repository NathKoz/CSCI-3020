using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src.commands
{
    public class ClearCommand() : Command
    {
        public override int Execute(CommandContext context)
        {
            AnsiConsole.Clear();
            return 0;
        }
    }
}
