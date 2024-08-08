using Spectre.Console;

namespace aegis_3020_p2.src
{
    public class Colours
    {
        public static readonly Dictionary<string, Color> Shades =
            new()
            {
                { "Black", Color.Black },
                { "Gray", Color.Grey },
                { "White", Color.White },
            };

        public static readonly Dictionary<string, Color> PrimaryAndSecondaryColours =
            new()
            {
                { "Yellow", Color.Yellow },
                { "Orange", Color.Orange1 },
                { "Red", Color.Red },
                { "Purple", Color.Purple }, // Important: This is fucking megenta? - Wolfy
                { "Blue", Color.Blue },
                { "Green", Color.Green },
            };

        public static readonly Dictionary<string, Color> DarkPrimaryAndSecondaryColours =
            new()
            {
                { "DarkYellow", Color.Yellow4 },
                { "DarkOrange", Color.DarkOrange3 },
                { "DarkRed", Color.DarkRed },
                { "DarkPurple", Color.Purple4 }, // Important: Looks like the colour of my heart? wtf? - Wolfy
                { "DarkBlue", Color.DarkBlue },
                { "DarkGreen", Color.DarkGreen },
            };

        public static readonly Dictionary<string, Color> LightPrimaryAndSecondaryColours =
            new()
            {
                { "LightRed", Color.IndianRed1 },
                { "LightBlue", Color.DodgerBlue1 },
                { "LightGreen", Color.LightGreen },
            };

        public static readonly Dictionary<
            string,
            Color
        > PrimaryAndSecondaryLightAndDarkColoursCombined = PrimaryAndSecondaryColours
            .Concat(DarkPrimaryAndSecondaryColours)
            .Concat(LightPrimaryAndSecondaryColours)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        public static Color GetRandomPrimaryAndSecondaryLightAndDarkColoursCombinedColour()
        {
            var random = new Random();
            var randomIndex = random.Next(PrimaryAndSecondaryLightAndDarkColoursCombined.Count);

            var enumerator = PrimaryAndSecondaryLightAndDarkColoursCombined.Values.GetEnumerator();
            for (int i = 0; i <= randomIndex; i++)
                enumerator.MoveNext();

            return enumerator.Current;
        }

        // INFO: Department Colours.

        private static readonly Dictionary<string, Color> BsonDocumentNamesColours =
            new()
            {
                { "_id", Shades["White"] },
                { "employeeId", PrimaryAndSecondaryColours["Green"] },
                { "name", PrimaryAndSecondaryColours["Blue"] },
                { "status", DarkPrimaryAndSecondaryColours["DarkBlue"] },
                { "unitologyMember", PrimaryAndSecondaryColours["Orange"] },
                { "department", DarkPrimaryAndSecondaryColours["DarkRed"] },
                { "position", DarkPrimaryAndSecondaryColours["DarkGreen"] },
                { "securityAccessLevel", Shades["Gray"] },
            };

        public static Color ResolveBsonDocumentNameToColour(string name) =>
            BsonDocumentNamesColours.TryGetValue(name, out var tempColour)
                ? tempColour
                : Shades["White"];

        public static string AddColourToBsonDocumentNameOrValue(
            string name,
            string? value = null
        ) => $"[{ResolveBsonDocumentNameToColour(name)}]{value ?? name}[/]";
    }
}
