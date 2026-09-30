namespace SunamoPerformance._sunamo;

/// <summary>
/// File helpers used by the benchmarks (reduced copies of SunamoFileIO.TF and SunamoFileSystem.FS to keep this package flat).
/// </summary>
internal static class BenchmarkFileHelper
{
    /// <summary>
    /// Reads the whole file synchronously; a missing file is created empty and an empty string is returned.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    internal static string ReadAllTextSync(string path)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "");
            return "";
        }

        return File.ReadAllText(path);
    }

    /// <summary>
    /// Reads the whole file asynchronously; a missing file is created empty and an empty string is returned.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    internal static async Task<string> ReadAllText(string path)
    {
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, "");
            return "";
        }

        return await File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    /// <summary>
    /// Reads the file asynchronously as lines, dropping empty lines; a missing file is created empty.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    internal static async Task<List<string>> ReadAllLines(string path)
    {
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, "");
            return new List<string>();
        }

        return BenchmarkTextHelper.GetLines(await File.ReadAllTextAsync(path))
            .Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
    }

    /// <summary>
    /// Writes the content to the file synchronously.
    /// </summary>
    /// <param name="content">Content to write.</param>
    /// <param name="path">Path to the file.</param>
    internal static void SaveFile(string content, string path)
    {
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Writes the content to the file asynchronously.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="content">Content to write.</param>
    internal static Task WriteAllText(string path, string content)
    {
        return File.WriteAllTextAsync(path, content);
    }

    /// <summary>
    /// Inserts a text between the file name and its extension, keeping the directory.
    /// </summary>
    /// <param name="original">Original file path.</param>
    /// <param name="whatInsert">Text to insert.</param>
    internal static string InsertBetweenFileNameAndExtension(string original, string whatInsert)
    {
        var fileName = Path.GetFileNameWithoutExtension(original);
        var extension = Path.GetExtension(original);
        if (original.Contains('/') || original.Contains('\\'))
        {
            var directory = Path.GetDirectoryName(original);
            return Path.Combine(directory!, fileName + whatInsert + extension);
        }

        return fileName + whatInsert + extension;
    }
}
