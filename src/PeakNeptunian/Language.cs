using System;

namespace PeakNeptunian;

public enum Language
{
    GameLanguage,
    NeptunianRomanized,
    NeptunianNahnya
}

public static class LanguageExtensions
{
    public static string ToLanguageCode(this Language language)
    {
        return language switch
        {
            Language.GameLanguage => "game",
            Language.NeptunianRomanized => "qhe_YH@latin",
            Language.NeptunianNahnya => "qhe_YH@nahnya",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
}