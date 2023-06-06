namespace osb.Models;

public readonly record struct CountryModel(string Code, string Name)
{
    public static readonly CountryModel Unknown = new("UNK", "UNKNOWN");
}