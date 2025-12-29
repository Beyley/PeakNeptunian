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
        ["USE"] = new Localization("HAI", "hai"), // use
        ["ASK"] = new Localization("PYEI", "pyei"), // ask
        ["DROP"] = new Localization("YA", "ya"), // drop
        ["THROW"] = new Localization("YADEHEIPI", "ya deheipi"), // drop+push away
        ["HOLD"] = new Localization("DH", "dah"), // hold
        ["LUNGE"] = new Localization("PIGWF", "pi ngwv"), // move+fast
        ["GRAB"] = new Localization("DH", "dah"), // hold
        ["LOOK"] = new Localization("NYEI", "nyei"), // see
        ["LICK"] = new Localization("FWMCI", "fwm chi"), // eat+small
        ["EAT"] = new Localization("FWM", "fwm"), // eat
        
        ["ROPE"] = new Localization("DEF", "DEV"), // rope

        ["NATIONALITY"] = new Localization("HENMWFHN", "Henmwvaht"), // island+birth eg. starting island/nationality
        ["PASSPORT"] = new Localization("NWRAMPIHEN", "Nwrambihen"), // passport
        ["NAME"] = new Localization("PRENNAI", "Prennai"), // name
        ["PASSPORTNO"] = new Localization("FACACINWRAMPIHEN", "Fachachi nwrambihen"), // passport+number

        ["NAME_BING BONG"] = new Localization("PIGPAG", "PING PANG"), // bing bong
        ["NAME_PASSPORT"] = new Localization("NWRAMPIHEN", "NWRAMBIHEN"), // passport

        // ["BB_IMBINGBONG"] = new("PHPWPIGPAG", "phpw ping pang"), // i'm bing bong
        // ["BB_IMBINGBONG"] = new Localization("PHPWPHDEGRIMIGI", "phpw pahdegrimi ngi"), // i'm fellow ngi

        ["BB_ASKYOURFRIENDS"] = new Localization("CHAVSHPYEIPHPEDEGSHREFINYADHFHAI", "chav sahbyei pahbedeng shre finya dahf hai!"),
        ["BB_IFYOUWANNA"] = new Localization("PEIFHNSHNWG!", "peifaht sahnwng..."),
        ["BB_BADIDEA"] = new Localization("PHGYE,HEIMWHEIKANYAF!", "pahgyw, hei mw heika nyav..."),
        ["BB_NAH"] = new Localization("KRH!", "krah..."),
        ["BB_NONONO"] = new Localization("KRHKRHKRHKRHKRH", "krh krh krh krh krh"),
        ["BB_SURE"] = new Localization("N,NYAI", "n...nyai"),
        ["BB_YES"] = new Localization("FI", "fi!"),
        ["BB_DEFINITELYNOT"] = new Localization("KRHNYAN", "krh nyan"),
        ["BB_IMBINGBONG"] = new Localization("PHPWPIGPAG", "phpw ping pang"),
        ["BB_MAYBE"] = new Localization("ME,,,,,,,PI", "meeeeeeeebi"),
        ["BB_NO"] = new Localization("KRH.", "krh."),
        ["BB_IFISAYYES"] = new Localization("SHRHFPEIPYEIPANIRHSA,PEIHNPHDHFI", "shrahfbei pyei pa nirh sa, peifaht pahdh fi"),
        ["BB_YEAH"] = new Localization("F,FI!", "f.. fi..."),
        ["BB_IDUNNO"] = new Localization("PHGYEKHR", "pahgywkrh"),
        ["BB_FINE"] = new Localization("PHGYEKAMWNYAI!", "pahgyw ka mw nyai..."),
        ["BB_IMISSMYWIFE"] = new Localization("PHNWGFH", "pahnwng fh"),
        ["BB_INTENSENO"] = new Localization("KRH!", "KRH!!!!!!"),
        ["BB_OKAY"] = new Localization("NYAI!", "nyai..."),
        ["BB_DEFINITELY"] = new Localization("F,FI.NYAN!", "f-fi, nyan!"),
        ["BB_MAYNEPTUNEBLESSUSALL"] = new Localization("CHAVYHGFINYEMYHFANIVAP!", "chav yhng finyemyh fanivap..."),
        
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