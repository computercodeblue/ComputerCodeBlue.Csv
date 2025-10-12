using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    public static class CsvExtensions
    {
        public static IEnumerable<T> ReadCsv<T>(string filePath, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);

            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<T>() ?? Enumerable.Empty<T>();
        }

        public static async IAsyncEnumerable<T> ReadCsvAsync<T>(string filePath, CsvOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
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

        public static void WriteCsv<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecords(items);
        }

        public static async Task WriteCsvAsync<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)
        {
            var config = CsvOptionsAdapter.ToCsvConfiguration(options ?? CsvOptions.Default);
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            await csv.WriteRecordsAsync(items, ct);
        }
    }
}

