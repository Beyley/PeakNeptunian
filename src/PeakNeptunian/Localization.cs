using System;

namespace PeakNeptunian;

public record Localization(string Id, string Str)
{
    public readonly string Id = Id;
    public readonly string Str = Str;

    public virtual bool Equals(Localization? other)
    {
        return Id == other?.Id && Str == other?.Str;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Str);
    }
}