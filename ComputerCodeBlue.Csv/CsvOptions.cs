using System;
using System.Globalization;

namespace ComputerCodeBlue.Csv
{
    /// <summary>
    /// Csv options exposed to consumers without referencing CsvHelper types.
    /// </summary>
    public sealed class CsvOptions
    {
        /// <summary>Culture used for parsing/formatting. Defaults to InvariantCulture.</summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>If true, CsvHelper will try to detect the delimiter.</summary>
        public bool DetectDelimiter { get; set; } = true;

        /// <summary>How to trim fields.</summary>
        public CsvTrimOptions Trim { get; set; } = CsvTrimOptions.Trim;

        /// <summary>Ignore blank lines in the file.</summary>
        public bool IgnoreBlankLines { get; set; } = true;

        /// <summary>How to handle missing fields.</summary>
        public CsvMissingFieldBehavior MissingField { get; set; } = CsvMissingFieldBehavior.Ignore;

        /// <summary>How to handle bad data.</summary>
        public CsvBadDataBehavior BadData { get; set; } = CsvBadDataBehavior.Ignore;

        /// <summary>Whether the CSV has a header record.</summary>
        public bool HasHeaderRecord { get; set; } = true;

        /// <summary>
        /// Optional header normalizer (e.g., h => h.Trim()) used for matching properties to columns.
        /// </summary>
        public Func<string, string>? PrepareHeader { get; set; }

        /// <summary>Optional explicit delimiter. If null/empty, CsvHelper's default/detection is used.</summary>
        public string? Delimiter { get; set; }

        public static CsvOptions Default
        {
            get
            {
                return new CsvOptions();
            }
        }
    }

    [Flags]
    public enum CsvTrimOptions
    {
        None = 0,
        Trim = 1,
        /// <summary>Trim inside quotes (mirrors CsvHelper TrimOptions.InsideQuotes)</summary>
        InsideQuotes = 2
    }

    public enum CsvMissingFieldBehavior
    {
        /// <summary>Ignore missing fields (no exception).</summary>
        Ignore,
        /// <summary>Throw when a field is missing.</summary>
        Throw
    }

    public enum CsvBadDataBehavior
    {
        /// <summary>Ignore bad data (no exception).</summary>
        Ignore,
        /// <summary>Throw when bad data is encountered.</summary>
        Throw
    }
}
