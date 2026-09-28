namespace SunamoPerformance;

/// <summary>
/// Thin wrapper around a string, used by FastStringLookupBenchmark to compare
/// looking up plain strings against looking up boxed reference-type objects
/// in a HashSet. Intentionally does not override Equals/GetHashCode, so a
/// HashSet of this type uses reference equality rather than value equality.
/// </summary>
public class WrapperStringObject
{
    private readonly string wrappedValue;

    /// <summary>Creates a wrapper holding the given string value.</summary>
    public WrapperStringObject(string wrappedValue)
    {
        this.wrappedValue = wrappedValue;
    }

    /// <summary>Returns the wrapped string value.</summary>
    public override string ToString()
    {
        return wrappedValue;
    }
}
