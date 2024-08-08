using MongoDB.Bson;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src.commands.member
{
    public class MemberFindCommand(MongoClient mongoClient)
        : Spectre.Console.Cli.Command<MemberFindCommand.Settings>
    {
        private readonly MongoClient _mongoClient = mongoClient;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[employeeId|name|department]")]
            public string? FindQuery { get; set; }

            [CommandOption("-q|--query <employeeId|name|department>")]
            public string[]? FindQueries { get; set; }

            // Info: Filters.

            [CommandOption("--filterByStatus")]
            public string? FilterByStatus { get; set; }

            // Info: Ordering.

            [CommandOption("--orderBySecurityAccessLevel")]
            public bool? OrderBySecurityAccessLevel { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine();

            var database = _mongoClient.GetDatabase(GlobalSettings.DATABASE_NAME);
            var collection = database.GetCollection<BsonDocument>(GlobalSettings.COLLECTION_NAME);

            // Info: Finding.

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(settings.FindQuery))
            {
                filter &= filterBuilder.Or(
                    filterBuilder.Eq("employeeId", settings.FindQuery),
                    filterBuilder.Regex("name", new BsonRegularExpression(settings.FindQuery, "i")),
                    filterBuilder.Eq("department", settings.FindQuery)
                );
            }

            if (settings.FindQueries != null && settings.FindQueries.Length > 0)
            {
                filter &= filterBuilder.Or(
                    filterBuilder.In("employeeId", settings.FindQueries.ToArray()),
                    filterBuilder.Regex(
                        "name",
                        new BsonRegularExpression(string.Join("|", settings.FindQueries), "i")
                    ),
                    filterBuilder.In("department", settings.FindQueries.ToArray())
                );
            }

            if (!string.IsNullOrEmpty(settings.FilterByStatus))
            {
                string status =
                    char.ToUpper(settings.FilterByStatus[0]) + settings.FilterByStatus[1..];
                filter &= filterBuilder.Eq("status", status);
            }

            // Info: Ordering.

            var ordering = Builders<BsonDocument>.Sort;
            var order = ordering.Ascending("name");

            if (
                settings.OrderBySecurityAccessLevel != null
                && settings.OrderBySecurityAccessLevel == true
            )
            {
                order = ordering.Descending("securityAccessLevel");
            }

            // Info: Core.

            var members = collection.Find(filter).Sort(order).ToList();

            if (members.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Query not valid![/]");
                return 1;
            }

            var table = new Table();

            members
                .SelectMany(m => m.Elements.Select(e => e.Name.ToString()))
                .Distinct()
                .ToList()
                .ForEach(c => table.AddColumn(Colours.AddColourToBsonDocumentNameOrValue(c)));

            members.ForEach(m =>
                table.AddRow(
                    m.Select(me =>
                            Colours.AddColourToBsonDocumentNameOrValue(
                                me.Name,
                                me.Value.ToString()!
                            )
                        )
                        .ToArray()
                )
            );

            AnsiConsole.Write(new Rule($"[yellow]Find Report:[/]").LeftJustified());
            AnsiConsole.Write(table);

            return 0;
        }
    }
}
