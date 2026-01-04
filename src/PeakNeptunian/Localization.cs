using System;
using System.Collections.Generic;

namespace PeakNeptunian;

public enum LocalizationType
{
    Off,
    Roman,
    Nahnya,
}

public record Localization(string Nahnya, string Roman)
{
    public readonly string Nahnya = Nahnya;
    public readonly string Roman = Roman;

    public virtual bool Equals(Localization? other)
    {
        return Nahnya == other?.Nahnya && Roman == other?.Roman;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Nahnya, Roman);
    }

    public static bool GetLocalizedString(LocalizationType type, string index, out string localized)
    {
        switch (type)
        {
            case LocalizationType.Nahnya:
            {
                if (Localizations.Neptunian.TryGetValue(index, out var localization))
                {
                    localized = localization.Nahnya;
                    return true;
                }

                break;
            }
            case LocalizationType.Roman:
            {
                if (Localizations.Neptunian.TryGetValue(index, out var localization))
                {
                    localized = localization.Roman;
                    return true;
                }

                break;
            }
        }

        localized = null!;
        return false;
    }
}