using System.Collections.Generic;
using System.IO;

namespace PeakNeptunian;

public static class Localizations
{
    public static void Load()
    {
        var translations = Helpers.GetEmbeddedResourceContent("translations.csv")!;

        int total = 0;
        int notTranslated = 0;
        TextReader reader = new StringReader(translations);
        _ = reader.ReadLine(); // skip header
        while (reader.ReadLine() is { } line)
        {
            var split = CSVReader.SplitCsvLine(line);

            total++;
            
            if (string.IsNullOrEmpty(split[2]) || string.IsNullOrEmpty(split[3]))
            {
                notTranslated++;
                continue;
            }

            Neptunian[split[0].ToUpperInvariant()] = new Localization(split[2], split[3]);
        }
        
        Plugin.Log.LogDebug($"Loaded {total-notTranslated}/{total} ({(total-notTranslated)/(double)total*100.0:N2}%) translations. {notTranslated} to go.");
    }

    public static readonly Dictionary<string, Localization> Neptunian = new();
}