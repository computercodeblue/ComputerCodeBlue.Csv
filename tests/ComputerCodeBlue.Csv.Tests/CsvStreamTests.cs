using System.Text;
using CsvHelper;

namespace ComputerCodeBlue.Csv.Tests;

public class CsvStreamTests
{
    private static readonly List<Person> People =
    [
        new Person("Alice", 30),
        new Person("Bob", 42)
    ];

    [Fact]
    public void Write_Then_Read_RoundTrips_Records_OnTheSameStream()
    {
        using var stream = new MemoryStream();

        CsvStream.Write(stream, People);
        stream.Position = 0;
        var result = CsvStream.Read<Person>(stream).ToList();

        Assert.Equal(People, result);
    }

    [Fact]
    public async Task WriteAsync_Then_ReadAsync_RoundTrips_Records_OnTheSameStream()
    {
        using var stream = new MemoryStream();

        await CsvStream.WriteAsync(stream, People);
        stream.Position = 0;
        var result = new List<Person>();
        await foreach (var person in CsvStream.ReadAsync<Person>(stream))
        {
            result.Add(person);
        }

        Assert.Equal(People, result);
    }

    // Regression: Write/Read must not close the caller's stream (e.g. an ASP.NET response body
    // or a stream the caller wants to reuse) - only the caller owns it, so only the caller should
    // dispose it.
    [Fact]
    public void Write_And_Read_LeaveTheStreamOpen()
    {
        using var stream = new MemoryStream();

        CsvStream.Write(stream, People);
        Assert.True(stream.CanWrite);

        stream.Position = 0;
        CsvStream.Read<Person>(stream);
        Assert.True(stream.CanRead);
    }

    // Regression: switching the internal StreamWriter to the fully-specified constructor (to pass
    // leaveOpen: true) must keep using UTF-8 *without* a byte order mark. Encoding.UTF8 emits one,
    // which would land as 3 extra bytes before the first header, corrupting it in strict CSV readers.
    [Fact]
    public void Write_DoesNotEmitByteOrderMark()
    {
        using var stream = new MemoryStream();

        CsvStream.Write(stream, People);

        var bytes = stream.ToArray();
        var utf8Bom = Encoding.UTF8.GetPreamble();
        Assert.False(bytes.Length >= utf8Bom.Length && bytes.AsSpan(0, utf8Bom.Length).SequenceEqual(utf8Bom));
    }

    [Fact]
    public void Read_HeaderOnly_ReturnsEmptySequence()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\n"));

        var result = CsvStream.Read<Person>(stream);

        Assert.Empty(result);
    }

    [Fact]
    public void ReadAnonymous_MapsByPropertyName_RegardlessOfTemplatePropertyOrder()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        // Template declares Age before Name, deliberately reversed from the CSV column order,
        // to prove matching is by name, not position.
        var result = CsvStream.ReadAnonymous(stream, new { Age = 0, Name = "" }).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(30, result[0].Age);
    }

    [Fact]
    public async Task ReadAnonymousAsync_MapsByPropertyName()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        var result = new List<(string Name, int Age)>();
        await foreach (var record in CsvStream.ReadAnonymousAsync(stream, new { Name = "", Age = 0 }))
        {
            result.Add((record.Name, record.Age));
        }

        Assert.Equal([("Alice", 30)], result);
    }

    // Regression test: a template shape that doesn't match the CSV's actual headers must fail
    // loudly (HeaderValidationException) rather than silently producing wrong/default data.
    [Fact]
    public void ReadAnonymous_TemplateWithPropertyNotInCsv_ThrowsHeaderValidationException()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        Assert.Throws<HeaderValidationException>(() =>
            CsvStream.ReadAnonymous(stream, new { Name = "", Age = 0, City = "" }).ToList());
    }
}
