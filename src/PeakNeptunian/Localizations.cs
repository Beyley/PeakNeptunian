using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Karambolo.PO;
using Unity.Collections.LowLevel.Unsafe;

namespace PeakNeptunian;

public static class Localizations
{
    public static readonly Dictionary<Language, Dictionary<string, Localization>> LoadedLocalizations = [];
    public static readonly Dictionary<Language, HashSet<string>> BackwardsLocalizations = [];

    public static void LoadLanguages()
    {
        LoadLanguage(Language.NeptunianRomanized);
    }

    public static void LoadLanguage(Language language)
    {
        var content = Helpers.GetEmbeddedResourceContent($"localizations.{language.ToLanguageCode()}.po");
        if (content == null) return;

        var parser = new POParser();
        var result = parser.Parse(content);

        if (!result.Success)
            throw new InvalidDataException(
                $"Failed to parse localization for language {language}: {result.Diagnostics.Select(d => d.ToString()).Aggregate((a, b) => $"{a}\n{b}")}");

        var catalog = result.Catalog;

        Dictionary<string, Localization> localizations = [];

        foreach (var entry in catalog)
        {
            var id = entry.Key;
            if (entry is not POSingularEntry singularEntry)
            {
                Plugin.Log.LogWarning($"Skipping non-singular entry with id {id} in language {language}");
                continue;
            }

            if (string.IsNullOrEmpty(singularEntry.Translation))
            {
                continue;
            }

            if (entry.Comments.FirstOrDefault(c => c is POExtractedComment) is not POExtractedComment key)
            {
                Plugin.Log.LogWarning(
                    $"Skipping entry with id {id} in language {language} because it has no extracted comment");
                continue;
            }

            const string KEY_PREFIX = "Key: ";

            if (!key.Text.StartsWith(KEY_PREFIX))
            {
                Plugin.Log.LogWarning(
                    $"Skipping entry with id {id} in language {language} because its extracted comment does not start with '{KEY_PREFIX}'");
                continue;
            }

            string originalTranslation = singularEntry.Translation;

            // Remap U+F1E00..U+F1EFF to U+E000..U+E0FF
            byte[] utf32 = Encoding.UTF32.GetBytes(originalTranslation);
            for (int i = 0; i < utf32.Length; i += 4)
            {
                uint codepoint = BitConverter.ToUInt32(utf32, i);
                if (codepoint >= 0xF1E00 && codepoint <= 0xF1EFF)
                {
                    codepoint = (codepoint - 0xF1E00) + 0xE000;
                    byte[] remapped = BitConverter.GetBytes(codepoint);
                    Array.Copy(remapped, 0, utf32, i, 4);
                }
            }
            string translation = Encoding.UTF32.GetString(utf32);

            localizations[key.Text.Substring(KEY_PREFIX.Length)] =
                new Localization(key.Text, translation);
        }

        LoadedLocalizations[language] = localizations;

        // Create backwards localizations for to determine if a string is already localized (e.g. for pre-localized strings that we need to detect and set the font)
        BackwardsLocalizations[language] = localizations.Values.Select(l => l.Str).ToHashSet();
    }
}