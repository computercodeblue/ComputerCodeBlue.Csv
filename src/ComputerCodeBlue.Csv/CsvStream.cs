using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    /// <remarks>
    /// None of the methods on this class close or dispose the <see cref="Stream"/> passed to them.
    /// The caller creates the stream, so the caller owns its lifetime.
    /// </remarks>
    public static class CsvStream
    {
        // netstandard2.1 lacks the StreamReader/StreamWriter overloads with defaulted encoding/
        // bufferSize, so leaveOpen must be requested via the fully-specified constructor. These
        // constants reproduce what the parameterless constructors default to, so behavior is
        // otherwise unchanged - in particular, StreamWriter's real default is UTF-8 *without* a
        // byte order mark (Encoding.UTF8 would add one and corrupt the first field of the CSV).
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        private const int DefaultBufferSize = 1024;

        /// <summary>
        /// Reads records from <paramref name="stream"/>. The stream is left open; the caller
        /// remains responsible for disposing it.
        /// </summary>
        public static IEnumerable<T> Read<T>(Stream stream, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvReader(reader, config);
            return (csv.GetRecords<T>() ?? Array.Empty<T>()).ToList();
        }

        /// <inheritdoc cref="Read{T}(Stream, CsvOptions?)"/>
        public static async IAsyncEnumerable<T> ReadAsync<T>(Stream stream, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: true);
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
        public static IEnumerable<T> ReadAnonymous<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null)
            => Read<T>(stream, options);

        /// <inheritdoc cref="ReadAnonymous{T}(Stream, T, CsvOptions?)"/>
        public static IAsyncEnumerable<T> ReadAnonymousAsync<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)
            => ReadAsync<T>(stream, options, ct);

        /// <summary>
        /// Reads <paramref name="stream"/> into a dictionary per row, keyed by the file's actual
        /// header names, with every value as the raw field string (no type conversion). Intended
        /// for loosely-typed uses like template/merge-field substitution, where there's no fixed
        /// record type to read into.
        /// </summary>
        public static IEnumerable<IDictionary<string, string>> ReadDynamic(Stream stream, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            return CsvDynamicReader.ReadRecords(csv);
        }

        /// <inheritdoc cref="ReadDynamic(Stream, CsvOptions?)"/>
        public static async IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(Stream stream, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            await foreach (var row in CsvDynamicReader.ReadRecordsAsync(csv, ct).ConfigureAwait(false))
            {
                yield return row;
            }
        }

        /// <summary>
        /// Writes <paramref name="items"/> to <paramref name="stream"/> using an explicit column
        /// list instead of a fixed record type. Each <paramref name="headers"/> entry becomes a
        /// column, and for every item the value is looked up by that column name - not by
        /// enumeration order - so items with different shapes (or the same keys added in a
        /// different order) still land in the right columns. A missing key writes a blank cell,
        /// or throws a <see cref="WriterException"/> if <see cref="CsvOptions.MissingField"/> is
        /// <see cref="CsvMissingFieldBehavior.Throw"/>. The header row is always written, even for
        /// an empty <paramref name="items"/>. The stream is left open, as with the other methods
        /// on this class.
        /// </summary>
        public static void WriteDynamic(Stream stream, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null)
        {
            var effectiveOptions = options ?? CsvOptions.Default;
            var config = CsvOptionsAdapter.ToCsvConfiguration(effectiveOptions);
            using var writer = new StreamWriter(stream, Utf8NoBom, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvWriter(writer, config);

            CsvDynamicWriter.WriteRecords(csv, headers, items, effectiveOptions);
        }

        /// <inheritdoc cref="WriteDynamic(Stream, IEnumerable{string}, IEnumerable{IDictionary{string, object?}}, CsvOptions?)"/>
        public static async Task WriteDynamicAsync(Stream stream, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var effectiveOptions = options ?? CsvOptions.Default;
            var config = CsvOptionsAdapter.ToCsvConfiguration(effectiveOptions);
            using var writer = new StreamWriter(stream, Utf8NoBom, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvWriter(writer, config);

            await CsvDynamicWriter.WriteRecordsAsync(csv, headers, items, effectiveOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes records to <paramref name="stream"/>, flushing before returning. The stream is
        /// left open; the caller remains responsible for disposing it.
        /// </summary>
        public static void Write<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var writer = new StreamWriter(stream, Utf8NoBom, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(items);
        }

        /// <inheritdoc cref="Write{T}(Stream, IEnumerable{T}, CsvOptions?)"/>
        public static async Task WriteAsync<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var writer = new StreamWriter(stream, Utf8NoBom, DefaultBufferSize, leaveOpen: true);
            using var csv = new CsvWriter(writer, config);
            await csv.WriteRecordsAsync(items, ct).ConfigureAwait(false);
        }
    }
}
