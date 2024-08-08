using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src.commands.member
{
    public class MemberExportCommand(MongoClient mongoClient)
        : Spectre.Console.Cli.Command<MemberExportCommand.Settings>
    {
        private readonly MongoClient _mongoClient = mongoClient;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[folderPath]")]
            public string OutputPath { get; set; } = Utilities.GetUserDownloadsFolder();

            public override ValidationResult Validate()
            {
                OutputPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(OutputPath));
                if (!Directory.Exists(OutputPath))
                {
                    return ValidationResult.Error($"Path not valid - {OutputPath}");
                }

                return base.Validate();
            }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine();

            var database = _mongoClient.GetDatabase(GlobalSettings.DATABASE_NAME);
            var collection = database.GetCollection<BsonDocument>(GlobalSettings.COLLECTION_NAME);

            // Info: Core.

            var filename = Path.GetRandomFileName();
            var filenameWithExtension = Path.ChangeExtension(filename, ".csv");
            var filepath = Path.Join(settings.OutputPath, filenameWithExtension);

            var streamWriter = new StreamWriter(filepath);
            var csvWriterConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            var csvWriter = new CsvWriter(streamWriter, csvWriterConfiguration);

            var members = collection.Find(new BsonDocument()).ToList();

            members
                .SelectMany(m => m.Elements.Select(e => e.Name))
                .Distinct()
                .ToList()
                .ForEach(csvWriter.WriteField);

            csvWriter.NextRecord();

            members.ForEach(m =>
            {
                var fields = m.Elements.Select(e => e.Value.ToString()).ToList();
                fields.ForEach(f => csvWriter.WriteField(f));
                csvWriter.NextRecord();
            });

            AnsiConsole.Write(new Rule($"[yellow]Member Export Result:[/]").LeftJustified());
            AnsiConsole.MarkupLine($"[green]Members successfully exported to {filepath}[/]");

            return 0;
        }
    }
}
