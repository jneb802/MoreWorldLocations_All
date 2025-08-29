using System;

namespace More_World_Locations_AIO;

public static class NameGenerator
{
    private static readonly string[] Prefixes =
    {
        "Skjold", "Ravn", "Drakkar", "Storm", "Mist", "Iron", "Njord", "Vargr",
        "Jotun", "Hrafn", "Skald", "Seidr", "Frost", "Ulf", "Elder", "Orm", "Dyr",
        "Bjorn", "Thor", "Loki", "Fenr", "Gald", "Svein", "Eik", "Grim", "Kald"
    };

    private static readonly string[] Middles =
    {
        "", "", "", "ar", "en", "ul", "ir", "or", "an", "und", "vin", "str",
        "gard", "heim", "skar", "vik", "lod", "oth", "brand", "stein"
    };

    private static readonly string[] Suffixes =
    {
        "havn", "vik", "fjord", "sund", "holm", "nes", "strand",
        "gard", "heim", "lund", "berg", "skog", "fjordr", "torp"
    };

    private static readonly Random rng = new Random();

    public static string GenerateName()
    {
        string prefix = Prefixes[rng.Next(Prefixes.Length)];
        string middle = Middles[rng.Next(Middles.Length)];
        string suffix = Suffixes[rng.Next(Suffixes.Length)];

        string name = prefix + middle + suffix;

        return name;
    }
}