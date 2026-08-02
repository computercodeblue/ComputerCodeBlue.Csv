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

