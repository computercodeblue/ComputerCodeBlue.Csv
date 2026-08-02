using System.Data;
using System.Text;
using CsvHelper;

namespace ComputerCodeBlue.Csv.Tests;

public class DataTableExtensionsTests
{
    private static readonly List<Person> People =
    [
        new Person("Alice", 30),
        new Person("Bob", 42)
    ];

    [Fact]
    public void LoadCsv_FilePath_IntoEmptyTable_CreatesStringColumns()
    {
        using var file = new TempCsvFile();
        CsvFile.Write(file.Path, People);

        var table = new DataTable().LoadCsv(file.Path);

        Assert.Equal(typeof(string), table.Columns["Name"]!.DataType);
        Assert.Equal(typeof(string), table.Columns["Age"]!.DataType);
        Assert.Equal(2, table.Rows.Count);
        Assert.Equal("Alice", table.Rows[0]["Name"]);
        Assert.Equal("30", table.Rows[0]["Age"]);
    }

    [Fact]
    public void LoadCsv_Stream_IntoEmptyTable_CreatesStringColumns()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        var table = new DataTable().LoadCsv(stream);

        Assert.Equal(typeof(string), table.Columns["Name"]!.DataType);
        Assert.Single(table.Rows);
        Assert.Equal("Alice", table.Rows[0]["Name"]);
    }

    // A zip code with a leading zero must round-trip as text, not get coerced to a number.
    [Fact]
    public void LoadCsv_PreservesLeadingZeroValues()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Zip\r\n07030\r\n"));

        var table = new DataTable().LoadCsv(stream);

        Assert.Equal("07030", table.Rows[0]["Zip"]);
    }

    [Fact]
    public void LoadCsv_IntoPreTypedTable_ConvertsValuesToDeclaredColumnTypes()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        table.LoadCsv(stream);

        Assert.Equal(typeof(int), table.Columns["Age"]!.DataType);
        Assert.Equal(30, table.Rows[0]["Age"]);
    }

    [Fact]
    public void LoadCsv_HasHeaderRecordFalse_UsesDefaultColumnNames()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Alice,30\r\nBob,42\r\n"));
        var options = new CsvOptions { HasHeaderRecord = false };

        var table = new DataTable().LoadCsv(stream, options);

        Assert.Equal(["Column1", "Column2"], table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.Equal(2, table.Rows.Count);
    }

    [Fact]
    public void LoadCsv_HeaderOnly_ReturnsColumnsWithNoRows()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\n"));

        var table = new DataTable().LoadCsv(stream);

        Assert.Equal(["Name", "Age"], table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.Empty(table.Rows);
    }

    // Same failure mode as CsvFile.Read<T> on a completely empty file - not a new edge case.
    [Fact]
    public void LoadCsv_CompletelyEmptyFile_ThrowsReaderException()
    {
        using var stream = new MemoryStream();

        Assert.Throws<ReaderException>(() => new DataTable().LoadCsv(stream));
    }

    // Regression: LoadCsv must not close the caller's stream - only the caller owns it.
    [Fact]
    public void LoadCsv_Stream_LeavesTheStreamOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        new DataTable().LoadCsv(stream);

        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task LoadCsvAsync_FilePath_RoundTripsSameDataAsSyncMethod()
    {
        using var file = new TempCsvFile();
        CsvFile.Write(file.Path, People);

        var table = await new DataTable().LoadCsvAsync(file.Path);

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal("Bob", table.Rows[1]["Name"]);
        Assert.Equal("42", table.Rows[1]["Age"]);
    }

    [Fact]
    public async Task LoadCsvAsync_Stream_RoundTripsSameDataAsSyncMethod()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Age\r\nAlice,30\r\n"));

        var table = await new DataTable().LoadCsvAsync(stream);

        Assert.Single(table.Rows);
        Assert.Equal("Alice", table.Rows[0]["Name"]);
    }

    [Fact]
    public async Task LoadCsvAsync_AlreadyCancelledToken_ThrowsBeforeWorkStarts()
    {
        using var file = new TempCsvFile();
        CsvFile.Write(file.Path, People);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => new DataTable().LoadCsvAsync(file.Path, ct: cts.Token));
    }
}
