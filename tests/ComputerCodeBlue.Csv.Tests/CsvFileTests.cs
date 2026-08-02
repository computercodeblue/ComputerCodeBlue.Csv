using CsvHelper;

namespace ComputerCodeBlue.Csv.Tests;

public class CsvFileTests
{
    private static readonly List<Person> People =
    [
        new Person("Alice", 30),
        new Person("Bob", 42)
    ];

    [Fact]
    public void Write_Then_Read_RoundTrips_Records()
    {
        using var file = new TempCsvFile();

        CsvFile.Write(file.Path, People);
        var result = CsvFile.Read<Person>(file.Path);

        Assert.Equal(People, result);
    }

    [Fact]
    public async Task WriteAsync_Then_ReadAsync_RoundTrips_Records()
    {
        using var file = new TempCsvFile();

        await CsvFile.WriteAsync(file.Path, People);

        var result = new List<Person>();
        await foreach (var person in CsvFile.ReadAsync<Person>(file.Path))
        {
            result.Add(person);
        }

        Assert.Equal(People, result);
    }

    [Fact]
    public void ReadAnonymous_MapsByPropertyName()
    {
        using var file = new TempCsvFile();
        CsvFile.Write(file.Path, People);

        var result = CsvFile.ReadAnonymous(file.Path, new { Name = "", Age = 0 }).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(30, result[0].Age);
    }

    // Same regression as CsvStreamTests, exercised through the file-based entry point too:
    // a template shape that doesn't match the file's headers must fail loudly.
    [Fact]
    public void ReadAnonymous_TemplateWithPropertyNotInCsv_ThrowsHeaderValidationException()
    {
        using var file = new TempCsvFile();
        CsvFile.Write(file.Path, People);

        Assert.Throws<HeaderValidationException>(() =>
            CsvFile.ReadAnonymous(file.Path, new { Name = "", Age = 0, City = "" }).ToList());
    }
}
