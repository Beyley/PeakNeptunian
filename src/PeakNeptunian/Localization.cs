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
    public static readonly Dictionary<string, Localization> NeptunianLocalizations = new()
    {
        ["HOSTGAME"] = new Localization("MYHFEIFFH", "MYH FEIFFH"), // start+shared
        ["PLAYOFFLINE"] = new Localization("MYHNIKRHKEI", "MYH NIKRKEI"), // start+alone
        ["SETTINGS"] = new Localization("NWNEG", "NWNENG"), // want.pl
        ["CREDITS"] = new Localization("PAPADANWFIG", "PABADA NWVING"), // people+development
        ["NEWISLANDIN"] = new Localization("HENYHDEI", "HEN YH DEI"), // island+new in
        ["QUIT"] = new Localization("PIPI", "PIPI"), // exit
        ["BACK"] = new Localization("HEYA", "HEYA"), // return
        ["ACCOLADES"] = new Localization("NWNENHFEG", "NWNENAHVENG"), // stamp.pl
        ["REBINDCONTROLS"] = new Localization("YEIHAKEINISE", "YEI HAKEINISE"), // change.pl control.pl
        ["LEAVEGAME"] = new Localization("PIPI", "PIPI"), // exit

        ["PICKUP"] = new Localization("DHF", "dahf"), // pick up
        ["START"] = new Localization("MYH", "MYH"), // start
        ["START2"] = new Localization("MYH", "myh"), // start
        ["OPEN"] = new Localization("FEI", "fei"), // open

        ["NATIONALITY"] = new Localization("HENMWFHN", "Henmwvaht"), // island+birth eg. starting island/nationality
        ["PASSPORT"] = new Localization("NWRAMPIHEN", "Nwrambihen"), // passport
        ["NAME"] = new Localization("PRENNAI", "Prennai"), // name
        ["PASSPORTNO"] = new Localization("NWRAMPIHENFACACI", "Fachachi nwrambihen"), // passport+number

        ["NAME_BING BONG"] = new Localization("PIGPAG", "PING PANG"), // bing bong
        ["NAME_PASSPORT"] = new Localization("NWRAMPIHEN", "NWRAMBIHEN"), // passport

        // ["BB_IMBINGBONG"] = new("PHPWPIGPAG", "phpw ping pang"), // i'm bing bong
        ["BB_IMBINGBONG"] = new Localization("PHPWPHDEGRIMIGI", "phpw pahdegrimi ngi"), // i'm fellow ngi

        ["__NATIONALITY"] = new Localization("YHGNYEM", "YHNGNYEM") // neptunian island
    };

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
                if (NeptunianLocalizations.TryGetValue(index, out var localization))
                {
                    localized = localization.Nahnya;
                    return true;
                }

                break;
            }
            case LocalizationType.Roman:
            {
                if (NeptunianLocalizations.TryGetValue(index, out var localization))
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