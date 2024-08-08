using MongoDB.Bson;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src.commands.member
{
    public class MemberUpdateCommand(MongoClient mongoClient)
        : Spectre.Console.Cli.Command<MemberUpdateCommand.Settings>
    {
        private readonly MongoClient _mongoClient = mongoClient;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[employeeId|name|department]")]
            public string? FindQuery { get; set; }

            [CommandOption("-q|--query <employeeId|name|department>")]
            public string[]? FindQueries { get; set; }

            // Info: Updating.

            [CommandOption("--status <Alive|Dead>")]
            public string? StatusUpdate { get; set; }
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

            // Info: Updating.

            var updateBuilder = Builders<BsonDocument>.Update;
            var update = updateBuilder.Combine();

            if (!string.IsNullOrEmpty(settings.StatusUpdate))
            {
                update = updateBuilder.Set("status", settings.StatusUpdate);
            }

            // Info: Core.

            var members = collection.Find(filter).ToList();

            if (members.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Query not valid![/]");
                return 1;
            }
            else if (members.Count > 1)
            {
                var choices = members
                    .Select(s => $"{s["name"].AsString} (ID: {s["employeeId"].AsString})")
                    .ToList();

                var selected = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("[yellow]Query found multiple entries, Specify:[/]")
                        .PageSize(20)
                        .AddChoices(choices)
                );

                members = members
                    .Where(s =>
                        selected.Contains($"{s["name"].AsString} (ID: {s["employeeId"].AsString})")
                    )
                    .ToList();
            }

            var table = new Table();

            members
                .SelectMany(m => m.Elements.Select(e => e.Name.ToString()))
                .Distinct()
                .ToList()
                .ForEach(c => table.AddColumn(Colours.AddColourToBsonDocumentNameOrValue(c)));

            members.ForEach(member =>
            {
                var memberEmployeeId = member.GetValue("employeeId");
                var memberFilter = filterBuilder.Eq("employeeId", memberEmployeeId);

                var updateResult = collection.UpdateOne(memberFilter, update);

                if (updateResult.ModifiedCount > 0)
                {
                    var updatedMembers = collection.Find(memberFilter).ToList();
                    updatedMembers.ForEach(m =>
                        table.AddRow(m.Select(me => $"[green]{me.Value}[/]").ToArray())
                    );
                }
                else
                {
                    table.AddRow(member.Select(m => $"[yellow]{m.Value}[/]").ToArray());
                }
            });

            AnsiConsole.Write(new Rule($"[yellow]Update Report:[/]").LeftJustified());
            AnsiConsole.Write(table);

            return 0;
        }
    }
}
