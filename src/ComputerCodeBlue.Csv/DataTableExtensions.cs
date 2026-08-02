using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    /// <summary>
    /// Extension methods for loading CSV data into a <see cref="DataTable"/>, mirroring
    /// <see cref="DataTable.Load(System.Data.IDataReader)"/>.
    /// </summary>
    public static class DataTableExtensions
    {
        // netstandard2.1 lacks the StreamReader overload with defaulted encoding/bufferSize, so
        // leaveOpen must be requested via the fully-specified constructor. See CsvStream for the
        // same pattern.
        private const int DefaultBufferSize = 1024;


        /// <summary>
        /// Loads the file at <paramref name="filePath"/> into <paramref name="table"/> via
        /// <see cref="DataTable.Load(System.Data.IDataReader)"/>. If <paramref name="table"/> has
        /// no columns, columns are created (all typed <see cref="string"/>) from the CSV header.
        /// If <paramref name="table"/> already has columns, values are matched by column name and
        /// converted to each column's existing type. Returns <paramref name="table"/> for chaining.
        /// </summary>
        public static DataTable LoadCsv(this DataTable table, string filePath, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);

            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);
            using var dataReader = new CsvDataReader(csv);

            table.Load(dataReader);
            return table;
        }

        /// <summary>
        /// Loads <paramref name="stream"/> into <paramref name="table"/>. The stream is left open;
        /// the caller remains responsible for disposing it.
        /// </summary>
        /// <inheritdoc cref="LoadCsv(DataTable, string, CsvOptions?)"/>
        public static DataTable LoadCsv(this DataTable table, Stream stream, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvReader(reader, config);
            using var dataReader = new CsvDataReader(csv);

            table.Load(dataReader);
            return table;
        }

        /// <summary>
        /// Offloads <see cref="LoadCsv(DataTable, string, CsvOptions?)"/> to a thread-pool thread
        /// via <see cref="Task.Run(System.Action)"/> - there is no async path through
        /// <see cref="CsvDataReader"/>/<see cref="DataTable.Load(System.Data.IDataReader)"/>, so
        /// this offers no I/O overlap or server-side throughput benefit, only a way to keep a
        /// calling UI thread responsive. <paramref name="ct"/> is honored only before the load
        /// starts; once running, the load is not cancellable mid-way.
        /// </summary>
        public static Task<DataTable> LoadCsvAsync(this DataTable table, string filePath, CsvOptions? options = null, CancellationToken ct = default)
            => Task.Run(() => table.LoadCsv(filePath, options), ct);

        /// <inheritdoc cref="LoadCsvAsync(DataTable, string, CsvOptions?, CancellationToken)"/>
        /// <remarks>The stream is left open; the caller remains responsible for disposing it.</remarks>
        public static Task<DataTable> LoadCsvAsync(this DataTable table, Stream stream, CsvOptions? options = null, CancellationToken ct = default)
            => Task.Run(() => table.LoadCsv(stream, options), ct);
    }
}
