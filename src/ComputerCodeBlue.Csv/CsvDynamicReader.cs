using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    /// <summary>
    /// Core read loop shared by the <c>ReadDynamic</c>/<c>ReadDynamicAsync</c> methods on
    /// <see cref="CsvFile"/> and <see cref="CsvStream"/>: reads each row into a
    /// <see cref="Dictionary{TKey, TValue}"/> keyed by the file's actual header names.
    /// </summary>
    internal static class CsvDynamicReader
    {
        internal static List<IDictionary<string, string>> ReadRecords(CsvReader csv)
        {
            csv.Read();
            csv.ReadHeader();
            var headerRecord = csv.HeaderRecord ?? Array.Empty<string>();

            var results = new List<IDictionary<string, string>>();
            while (csv.Read())
            {
                results.Add(ReadRow(csv, headerRecord));
            }

            return results;
        }

        internal static async IAsyncEnumerable<IDictionary<string, string>> ReadRecordsAsync(CsvReader csv, [EnumeratorCancellation] CancellationToken ct)
        {
            await csv.ReadAsync().ConfigureAwait(false);
            csv.ReadHeader();
            var headerRecord = csv.HeaderRecord ?? Array.Empty<string>();

            while (await csv.ReadAsync().ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                yield return ReadRow(csv, headerRecord);
            }
        }

        private static Dictionary<string, string> ReadRow(CsvReader csv, string[] headerRecord)
        {
            var row = new Dictionary<string, string>(headerRecord.Length);
            foreach (var header in headerRecord)
            {
                row[header] = csv.GetField(header) ?? string.Empty;
            }

            return row;
        }
    }
}
