using MongoDB.Bson;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

// Important: USSR Approves this code.

namespace aegis_3020_p2.src.commands.member.compare
{
    public class MemberCompareUSRVNMCommand(MongoClient mongoClient) : Command
    {
        private readonly MongoClient _mongoClient = mongoClient;

        public override int Execute(CommandContext context)
        {
            AnsiConsole.WriteLine();

            var database = _mongoClient.GetDatabase(GlobalSettings.DATABASE_NAME);
            var collection = database.GetCollection<BsonDocument>(GlobalSettings.COLLECTION_NAME);

            // Info: Core.

            // Info: Unitology Members.
            var unitologyMembers = collection
                .AsQueryable()
                .Where(m => m["unitologyMember"].AsBoolean == true);
            var totalUnitologyMembersAlive = (double)
                unitologyMembers.Count(m => m["status"].ToString() == "Alive");
            var totalUnitologyMembersAivePercentage =
                totalUnitologyMembersAlive / unitologyMembers.Count() * 100;

            // Info: Non Unitology Members.
            var nonUnitologyMembers = collection
                .AsQueryable()
                .Where(m => m["unitologyMember"].AsBoolean == false);
            var totalNonUnitologyMembersAlive = (double)
                nonUnitologyMembers.Count(m => m["status"].ToString() == "Alive");
            var totalNonUnitologyMembersAlivePercentage =
                totalNonUnitologyMembersAlive / nonUnitologyMembers.Count() * 100;

            var breakdownChart = new BreakdownChart()
                .Width(120)
                .AddItem(
                    $"Members Alive",
                    totalUnitologyMembersAivePercentage,
                    Colours.PrimaryAndSecondaryColours["Green"]
                )
                .AddItem(
                    $"Non-Members Alive",
                    totalNonUnitologyMembersAlivePercentage,
                    Colours.PrimaryAndSecondaryColours["Red"]
                )
                .UseValueFormatter(v => $"{Math.Round(v * 10) / 10}%");

            AnsiConsole.Write(
                new Rule(
                    $"[yellow]USRVNM (Unitology survival rates versus non-members) Report:[/]"
                ).LeftJustified()
            );
            AnsiConsole.Write(breakdownChart);

            return 0;
        }
    }
}
