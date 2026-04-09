#:package CsvHelper@33.1.0
#:package Newtonsoft.Json@13.0.4

using System;
using System.Globalization;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        string allText = File.ReadAllText(args[0]);

        static string EscapePoString(string? value)
        {
            return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", "\\n");
        }

        static IEnumerable<string> SplitPoString(string? value, int maxLen = 100)
        {
            var text = EscapePoString(value ?? string.Empty);

            var chunks = new List<string>();
            var start = 0;

            while (start < text.Length)
            {
                var end = Math.Min(start + maxLen, text.Length);

                if (end < text.Length)
                {
                    var split = -1;
                    for (var i = end - 1; i > start; i--)
                    {
                        if (char.IsWhiteSpace(text[i]))
                        {
                            split = i;
                            break;
                        }
                    }

                    if (split > start)
                    {
                        end = split + 1; // keep whitespace at end of current chunk
                    }
                }

                chunks.Add(text[start..end]);
                start = end;
            }

            return chunks;
        }


        string creation_date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm+0000");

        var poLines = new List<string>
        {
            // insert header
            "# This file is part of Peak Neptunian.",
            "# Copyright (C) YEAR THE COPYRIGHT OWNER",
            "# This file is distributed under the same license as the Peak Neptunian mod.",
            "# ",
            "# Translators:",
            "# - FIRST AUTHOR <EMAIL@ADDRESS>, YEAR",
            "msgid \"\"",
            "msgstr \"\"",
            "\"Project-Id-Version: Peak Neptunian\\n\"",
            "\"Report-Msgid-Bugs-To: <littlegreenmantis@gmail.com>\\n\"",
            $"\"POT-Creation-Date: {creation_date}\\n\"",
            "\"Last-Translator: FULL NAME <EMAIL@ADDRESS>, YEAR\\n\"",
            "\"Language-Team: LANGUAGE\\n\"",
            "\"Language: LANGUAGE_REGION@VARIANT\\n\"",
            "\"Content-Type: text/plain; charset=UTF-8\\n\"",
            "\"Content-Transfer-Encoding: 8bit\\n\"",
            "",
        };

        var records = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(allText);

        // Custom ones we've added!
        records.Add("BB_MAYNEPTUNEBLESSUSALL", ["may Neptune bless us all..."]);
        records.Add("BB_TODAYISFRIDAY", ["today is friday, in california... shoot!"]);
        records.Add("__NATIONALITY", ["CRABLAND"]);

        foreach (var (key, record) in records)
        {
            poLines.Add($"#. Key: {key}"); // Extracted comment. We're using the key as a reference here, since we need to include it somewhere for the mod to know which string to replace.

            string english = record[0];

            if (english.Length > 100)
            {
                poLines.Add("msgid \"\"");
                foreach (var part in SplitPoString(english))
                {
                    poLines.Add($"\"{part}\"");
                }
            }
            else
            {
                poLines.Add($"msgid \"{EscapePoString(english)}\"");
            }

            poLines.Add("msgstr \"\"");
            poLines.Add("");
        }

        File.WriteAllLines("src/PeakNeptunian/localizations/peak.pot", poLines);
    }
}
