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
