using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    /// <remarks>
    /// Each method opens its own <see cref="FileStream"/> for <paramref name="filePath"/> and
    /// closes it before returning (or, for the async-enumerable reads, once enumeration
    /// completes). Unlike <see cref="CsvStream"/>, there is no caller-supplied stream to manage.
    /// </remarks>
    public static class CsvFile
    {
        /// <summary>Reads records from the file at <paramref name="filePath"/>.</summary>
        public static IEnumerable<T> Read<T>(string filePath, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);

            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            return (csv.GetRecords<T>() ?? Enumerable.Empty<T>()).ToList();
        }

        /// <inheritdoc cref="Read{T}(string, CsvOptions?)"/>
        public static async IAsyncEnumerable<T> ReadAsync<T>(string filePath, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            await foreach (var record in csv.GetRecordsAsync<T>().WithCancellation(ct).ConfigureAwait(false))
            {
                yield return record;
            }
        }

        /// <summary>
        /// Reads records into instances of an anonymous type. Pass a throwaway instance of the
        /// desired shape (e.g. <c>new { Name = "", Age = 0 }</c>) as <paramref name="anonymousTypeTemplate"/>
        /// so the compiler can infer <typeparamref name="T"/>; its property values are never read,
        /// only its property names/types are used to match CSV headers.
        /// </summary>
        public static IEnumerable<T> ReadAnonymous<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null)
            => Read<T>(filePath, options);

        /// <inheritdoc cref="ReadAnonymous{T}(string, T, CsvOptions?)"/>
        public static IAsyncEnumerable<T> ReadAnonymousAsync<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)
            => ReadAsync<T>(filePath, options, ct);

        /// <summary>
        /// Reads the file at <paramref name="filePath"/> into a dictionary per row, keyed by the
        /// file's actual header names, with every value as the raw field string (no type
        /// conversion). Intended for loosely-typed uses like template/merge-field substitution,
        /// where there's no fixed record type to read into.
        /// </summary>
        public static IEnumerable<IDictionary<string, string>> ReadDynamic(string filePath, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            return CsvDynamicReader.ReadRecords(csv);
        }

        /// <inheritdoc cref="ReadDynamic(string, CsvOptions?)"/>
        public static async IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(string filePath, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            await foreach (var row in CsvDynamicReader.ReadRecordsAsync(csv, ct).ConfigureAwait(false))
            {
                yield return row;
            }
        }

        /// <summary>
        /// Writes <paramref name="items"/> to the file at <paramref name="filePath"/> using an
        /// explicit column list instead of a fixed record type. Each <paramref name="headers"/>
        /// entry becomes a column, and for every item the value is looked up by that column name
        /// - not by enumeration order - so items with different shapes (or the same keys added in
        /// a different order) still land in the right columns. A missing key writes a blank cell,
        /// or throws a <see cref="WriterException"/> if <see cref="CsvOptions.MissingField"/> is
        /// <see cref="CsvMissingFieldBehavior.Throw"/>. The header row is always written, even for
        /// an empty <paramref name="items"/>.
        /// </summary>
        public static void WriteDynamic(string filePath, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null)
        {
            var effectiveOptions = options ?? CsvOptions.Default;
            var config = CsvOptionsAdapter.ToCsvConfiguration(effectiveOptions);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            CsvDynamicWriter.WriteRecords(csv, headers, items, effectiveOptions);
        }

        /// <inheritdoc cref="WriteDynamic(string, IEnumerable{string}, IEnumerable{IDictionary{string, object?}}, CsvOptions?)"/>
        public static async Task WriteDynamicAsync(string filePath, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var effectiveOptions = options ?? CsvOptions.Default;
            var config = CsvOptionsAdapter.ToCsvConfiguration(effectiveOptions);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            await CsvDynamicWriter.WriteRecordsAsync(csv, headers, items, effectiveOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes records to the file at <paramref name="filePath"/>, creating it if it does not
        /// exist or overwriting it if it does.
        /// </summary>
        public static void Write<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecords(items);
        }

        /// <inheritdoc cref="Write{T}(string, IEnumerable{T}, CsvOptions?)"/>
        public static async Task WriteAsync<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            await csv.WriteRecordsAsync(items, ct);
        }
    }
}

