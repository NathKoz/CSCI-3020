using System.Text;
using aegis_3020_p2.src.commands;
using aegis_3020_p2.src.commands.department.compare;
using aegis_3020_p2.src.commands.member;
using aegis_3020_p2.src.commands.member.compare;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Spectre.Console;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src
{
    class Program
    {
        private static readonly TypeRegistrar registrar = new(new ServiceCollection());
        private static readonly CommandApp commandApp = new(registrar);

        static void Main()
        {
            AnsiConsole.Clear();

            var (username, password) = PromptUserNameAndPassword();

            try
            {
                AuthenticateDatabase(username, password);
            }
            catch
            {
                AnsiConsole.WriteLine("ERORR 1");
                Environment.Exit(0);
            }

            AnsiConsole.Clear();
            ConfigureCommandApp();
            StartVirtualShellLoop();
        }

        private static void ConfigureCommandApp()
        {
            commandApp.Configure(config =>
            {
                config.SetApplicationName(GlobalSettings.SHIP_NAME);
                config.SetApplicationVersion("🐺 7.2.2511"); // INFO: Easter Egg.
                config.SetExceptionHandler(
                    (exception, resolver) =>
                        AnsiConsole.WriteException(exception, ExceptionFormats.NoStackTrace)
                );

                config
                    .AddBranch(
                        "department",
                        d =>
                            d.AddBranch(
                                "compare",
                                dc =>
                                {
                                    dc.AddCommand<DepartmentCompareCPDCommand>("cpd")
                                        .WithAlias("casualties-per-department");
                                }
                            )
                    )
                    .WithAlias("departments");

                config
                    .AddBranch(
                        "member",
                        m =>
                        {
                            m.AddCommand<MemberExportCommand>("export");
                            m.AddCommand<MemberFindCommand>("find");
                            m.AddCommand<MemberUpdateCommand>("update");

                            m.AddBranch(
                                "compare",
                                mc =>
                                {
                                    mc.AddCommand<MemberCompareUSRVNMCommand>("usrvnm")
                                        .WithAlias("unitology-survival-rates-versus-non-members");
                                }
                            );
                        }
                    )
                    .WithAlias("members");

                config.AddCommand<ClearCommand>("clear");
                config.AddCommand<SearchCommand>("search");
            });
        }

        private static (string username, string password) PromptUserNameAndPassword()
        {
            var rule = new Rule(
                $"[blue]:rocket: {GlobalSettings.SHIP_NAME}[/] - :stop_sign: Authentication Check :stop_sign:"
            )
                .RuleStyle(Style.Parse("red"))
                .LeftJustified();

            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            var usernamePromptStyle = new Style();
            var usernamePrompt = new TextPrompt<string>("Username >").PromptStyle(
                usernamePromptStyle
            );
            var username = AnsiConsole.Prompt(usernamePrompt);

            var passwordPromptStyle = new Style();
            var passwordPrompt = new TextPrompt<string>("Password >")
                .PromptStyle(passwordPromptStyle)
                .Secret();
            var password = AnsiConsole.Prompt(passwordPrompt);

            return (username, password);
        }

        private static void AuthenticateDatabase(string username, string password)
        {
            AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Grenade)
                .Start(
                    "[yellow]Authenticating details[/]... Please wait..",
                    context =>
                    {
                        context.Status = "[yellow]Generating connection string..[/]";
                        if (GlobalSettings.WE_WANT_TO_SEE_STATUS_MESSAGES)
                            Thread.Sleep(1000);
                        var connectionString =
                            $"mongodb+srv://{username}:{password}@{GlobalSettings.DATABASE_HOSTNAME}";
                        context.Status = "[green]Generated Connection String..[/]";

                        if (GlobalSettings.WE_WANT_TO_SEE_STATUS_MESSAGES)
                            Thread.Sleep(1000);

                        context.Status = "[yellow]Creating new database connection..[/]";
                        if (GlobalSettings.WE_WANT_TO_SEE_STATUS_MESSAGES)
                            Thread.Sleep(1000);
                        var mongoClient = new MongoClient(connectionString);
                        registrar.RegisterInstance(typeof(MongoClient), mongoClient);
                        context.Status = "[green]Created new database connection..[/]";

                        if (GlobalSettings.WE_WANT_TO_SEE_STATUS_MESSAGES)
                            Thread.Sleep(1000);

                        context.Status = "[yellow]Testing database connection..[/]";
                        if (GlobalSettings.WE_WANT_TO_SEE_STATUS_MESSAGES)
                            Thread.Sleep(1000);
                        var database = mongoClient.GetDatabase(GlobalSettings.DATABASE_NAME);
                        var collections = database.ListCollections().ToList();
                        context.Status = "[green]Database connection test successfull..[/]";
                    }
                );
        }

        private static void StartVirtualShellLoop()
        {
            while (true)
            {
                AnsiConsole.WriteLine();

                var rule = new Rule($"[blue]:rocket: {GlobalSettings.SHIP_NAME}[/] ")
                    .RuleStyle(Style.Parse("white"))
                    .LeftJustified();
                AnsiConsole.Write(rule);

                var input = ReadLine.Read("> ");
                var parsedArguments = ParseArguments(input);

                commandApp.Run(parsedArguments);
            }
        }

        private static string[] ParseArguments(string input)
        {
            var inQuotes = false;
            var args = new List<string>();
            var currentArg = new StringBuilder();

            foreach (var character in input)
            {
                if (character == '"')
                    inQuotes = !inQuotes;
                else if (character == ' ' && !inQuotes)
                {
                    if (currentArg.Length > 0)
                    {
                        args.Add(currentArg.ToString());
                        currentArg.Clear();
                    }
                }
                else
                    currentArg.Append(character);
            }

            if (currentArg.Length > 0)
                args.Add(currentArg.ToString());

            return [.. args];
        }
    }
}
