namespace ComputerCodeBlue.Csv.Tests;

/// <summary>Deletes itself on dispose so file-based tests don't leak temp files on failure.</summary>
internal sealed class TempCsvFile : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
