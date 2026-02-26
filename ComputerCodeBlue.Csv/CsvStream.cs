using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    public static class CsvStream
    {
        public static IEnumerable<T> Read<T>(Stream stream, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);
            return (csv.GetRecords<T>() ?? Array.Empty<T>()).ToList();
        }

        public static async IAsyncEnumerable<T> ReadAsync<T>(Stream stream, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);
            await foreach (var record in csv.GetRecordsAsync<T>().WithCancellation(ct).ConfigureAwait(false))
            {
                yield return record;
            }
        }

        public static void Write<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(items);
        }

        public static async Task WriteAsync<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);
            await csv.WriteRecordsAsync(items, ct).ConfigureAwait(false);
        }
    }
}
