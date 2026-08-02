using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace ComputerCodeBlue.Csv
{
    /// <summary>
    /// Core write loop shared by the <c>WriteDynamic</c>/<c>WriteDynamicAsync</c> methods on
    /// <see cref="CsvFile"/> and <see cref="CsvStream"/>: writes an explicit header list, then
    /// looks each item's fields up by column name rather than relying on enumeration order.
    /// </summary>
    internal static class CsvDynamicWriter
    {
        internal static void WriteRecords(CsvWriter csv, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions options)
        {
            var headerList = headers as IReadOnlyList<string> ?? headers.ToList();

            foreach (var header in headerList)
            {
                csv.WriteField(header);
            }
            csv.NextRecord();

            foreach (var item in items)
            {
                WriteRow(csv, headerList, item, options);
                csv.NextRecord();
            }
        }

        internal static async Task WriteRecordsAsync(CsvWriter csv, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions options, CancellationToken ct)
        {
            var headerList = headers as IReadOnlyList<string> ?? headers.ToList();

            foreach (var header in headerList)
            {
                csv.WriteField(header);
            }
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();
                WriteRow(csv, headerList, item, options);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }

        private static void WriteRow(CsvWriter csv, IReadOnlyList<string> headers, IDictionary<string, object?> item, CsvOptions options)
        {
            foreach (var header in headers)
            {
                var found = item.TryGetValue(header, out var value);
                if (!found && options.MissingField == CsvMissingFieldBehavior.Throw)
                {
                    throw new WriterException(csv.Context, $"Field '{header}' does not exist on the record.");
                }

                csv.WriteField(value);
            }
        }
    }
}
