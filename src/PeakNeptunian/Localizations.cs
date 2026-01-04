using System.Collections.Generic;
using System.IO;

namespace PeakNeptunian;

public static class Localizations
{
    public static void Load()
    {
        var translations = Helpers.GetEmbeddedResourceContent("translations.csv")!;

        TextReader reader = new StringReader(translations);
        while (reader.ReadLine() is { } line)
        {
            var split = CSVReader.SplitCsvLine(line);
            Neptunian[split[0]] = new Localization(split[2], split[3]);
        }
    }

    public static readonly Dictionary<string, Localization> Neptunian = new();
}