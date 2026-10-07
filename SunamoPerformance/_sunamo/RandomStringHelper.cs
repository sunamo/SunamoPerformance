namespace SunamoPerformance._sunamo;

/// <summary>
/// Random string generation for the benchmarks (copy of SunamoRandom.RandomStringHelper to keep this package flat).
/// </summary>
internal static class RandomStringHelper
{
    private static readonly Random random = new();
    private const string alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static readonly char[] specialCharsAll =
    {
        '!', '@', '#', '$', '%', '^', '&', '*', '?', '_', '~',
        '\u201C', '\u201D', '-', '\u2018', '\u2019', ',', '.', ':', '\'', ')', '/', '<', '>', '{', '}', '[', '|', ';', '+', ']', '\u2013', '/',
        ' ',
        (char)160, '\u00A9'
    };

    /// <summary>
    /// Generates a random string of the given length starting with the given number of special (non-alphanumeric) characters.
    /// </summary>
    /// <param name="length">Total length of the generated string.</param>
    /// <param name="numberOfNonAlphanumericCharacters">Number of special characters to include.</param>
    internal static string RandomString(int length, int numberOfNonAlphanumericCharacters)
    {
        var stringChars = new char[length];
        var index = 0;

        for (; index < numberOfNonAlphanumericCharacters; index++)
            stringChars[index] = specialCharsAll[random.Next(specialCharsAll.Length)];

        for (; index < length; index++) stringChars[index] = alphanumericChars[random.Next(alphanumericChars.Length)];

        return new string(stringChars);
    }
}
