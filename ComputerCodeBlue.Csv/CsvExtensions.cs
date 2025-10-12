using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace ComputerCodeBlue.Csv
{
    public static class CsvExtensions
    {
        public static IEnumerable<T> ReadCsv<T>(string filePath, CsvConfiguration? config = null)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config ?? new CsvConfiguration(CultureInfo.InvariantCulture));

            return csv.GetRecords<T>() ?? Enumerable.Empty<T>();
        }

        public static async IAsyncEnumerable<T> ReadCsvAsync<T>(string filePath, CsvConfiguration? config = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config ?? new CsvConfiguration(CultureInfo.InvariantCulture));

            await foreach (var record in csv.GetRecordsAsync<T>().WithCancellation(ct).ConfigureAwait(false))
            {
                yield return record;
            }
        }

        public static void WriteCsv<T>(string filePath, IEnumerable<T> items, CsvConfiguration? config = null)
        {
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config ?? new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteRecords(items);
        }

        public static async Task WriteCsvAsync<T>(string filePath, IEnumerable<T> items, CsvConfiguration? config = null, CancellationToken ct = default)
        {
            using var stream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config ?? new CsvConfiguration(CultureInfo.InvariantCulture));

            await csv.WriteRecordsAsync(items, ct);
        }
    }
}

