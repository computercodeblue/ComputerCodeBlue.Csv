using CsvHelper;

namespace ComputerCodeBlue.Csv.Tests;

public class CsvDynamicTests
{
    private static readonly string[] Headers = ["Name", "Age"];

    private static string WriteDynamicToString(IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null)
    {
        using var stream = new MemoryStream();
        CsvStream.WriteDynamic(stream, headers, items, options);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact]
    public void WriteDynamic_EmptyItems_StillWritesHeader()
    {
        var result = WriteDynamicToString(Headers, []);

        Assert.Equal("Name,Age\r\n", result);
    }

    [Fact]
    public void WriteDynamic_MixedShapeItems_WriteBlanksForMissingKeys_NotMisalignedValues()
    {
        var items = new List<IDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Name"] = "Alice", ["Age"] = 30 },
            new Dictionary<string, object?> { ["City"] = "NYC", ["Zip"] = "10001" },
        };

        var result = WriteDynamicToString(Headers, items);

        Assert.Equal("Name,Age\r\nAlice,30\r\n,\r\n", result);
    }

    [Fact]
    public void WriteDynamic_SameKeys_DifferentInsertionOrder_StillAlignsByName()
    {
        var row1 = new Dictionary<string, object?> { ["Name"] = "Alice", ["Age"] = 30 };
        var row2 = new Dictionary<string, object?> { ["Age"] = 42, ["Name"] = "Bob" }; // reversed order

        var result = WriteDynamicToString(Headers, [row1, row2]);

        Assert.Equal("Name,Age\r\nAlice,30\r\nBob,42\r\n", result);
    }

    [Fact]
    public void WriteDynamic_MissingKey_WithMissingFieldThrow_Throws()
    {
        var items = new List<IDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Name"] = "Alice" }, // no "Age"
        };
        var options = new CsvOptions { MissingField = CsvMissingFieldBehavior.Throw };

        Assert.Throws<WriterException>(() => WriteDynamicToString(Headers, items, options));
    }

    [Fact]
    public void ReadDynamic_RoundTripsRows_KeyedByHeaderName()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\nBob,42\r\n"));

        var result = CsvStream.ReadDynamic(stream).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0]["Name"]);
        Assert.Equal("30", result[0]["Age"]);
        Assert.Equal("Bob", result[1]["Name"]);
        Assert.Equal("42", result[1]["Age"]);
    }

    // Regression: a zip code like "07030" must stay a string, not get parsed into 7030 and lose
    // its leading zero - this is exactly the fidelity template substitution depends on.
    [Fact]
    public void ReadDynamic_PreservesLeadingZeros()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Zip\r\n07030\r\n"));

        var result = CsvStream.ReadDynamic(stream).ToList();

        Assert.Equal("07030", result[0]["Zip"]);
    }

    [Fact]
    public async Task ReadDynamicAsync_RoundTripsRows()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        var result = new List<IDictionary<string, string>>();
        await foreach (var row in CsvStream.ReadDynamicAsync(stream))
        {
            result.Add(row);
        }

        Assert.Single(result);
        Assert.Equal("Alice", result[0]["Name"]);
        Assert.Equal("30", result[0]["Age"]);
    }

    [Fact]
    public void ReadDynamic_Then_WriteDynamic_RoundTrips_WithMutation()
    {
        using var readStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\nBob,42\r\n"));
        var rows = CsvStream.ReadDynamic(readStream).ToList();

        rows[0]["Age"] = "31"; // ReadDynamic values are strings, so mutate as a string

        var writeItems = rows.Select(r => (IDictionary<string, object?>)r.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));
        var result = WriteDynamicToString(Headers, writeItems);

        Assert.Equal("Name,Age\r\nAlice,31\r\nBob,42\r\n", result);
    }
}
