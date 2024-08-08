using MongoDB.Bson;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src.commands.department.compare
{
    public class DepartmentCompareCPDCommand(MongoClient mongoClient) : Command
    {
        private readonly MongoClient _mongoClient = mongoClient;

        public override int Execute(CommandContext context)
        {
            AnsiConsole.WriteLine();

            var database = _mongoClient.GetDatabase(GlobalSettings.DATABASE_NAME);
            var collection = database.GetCollection<BsonDocument>(GlobalSettings.COLLECTION_NAME);

            var departments = collection
                .AsQueryable()
                .Where(c => c["status"].ToString() == "Deceased")
                .GroupBy(c => c["department"].ToString())
                .OrderByDescending(c => c.Count())
                .Select(g => new { name = g.Key, Count = g.Count() })
                .ToList();

            if (departments.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Query not valid![/]");
                return 1;
            }

            var breakdownChart = new BreakdownChart().Width(120);

            departments.ForEach(d =>
                breakdownChart.AddItem(
                    d.name!,
                    d.Count,
                    Colours.GetRandomPrimaryAndSecondaryLightAndDarkColoursCombinedColour()
                )
            );

            AnsiConsole.Write(
                new Rule($"[yellow]CPD (Casualties Per Department) Report:[/]").LeftJustified()
            );
            AnsiConsole.Write(breakdownChart);

            return 0;
        }
    }
}
