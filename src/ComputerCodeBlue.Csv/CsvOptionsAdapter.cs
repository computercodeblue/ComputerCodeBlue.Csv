using System;
using CsvHelper.Configuration;

namespace ComputerCodeBlue.Csv
{
    internal static class CsvOptionsAdapter
    {
        internal static CsvConfiguration ToCsvConfiguration(CsvOptions options)
        {
            var cfg = new CsvConfiguration(options.Culture)
            {
                DetectDelimiter = options.DetectDelimiter,
                IgnoreBlankLines = options.IgnoreBlankLines,
                HasHeaderRecord = options.HasHeaderRecord
            };

            // Delimiter
            if (!string.IsNullOrWhiteSpace(options.Delimiter))
                cfg.Delimiter = options.Delimiter!;

            // Trim
            cfg.TrimOptions = options.Trim switch
            {
                CsvTrimOptions.None => TrimOptions.None,
                CsvTrimOptions.Trim => TrimOptions.Trim,
                CsvTrimOptions.InsideQuotes => TrimOptions.InsideQuotes,
                _ => TrimOptions.Trim
            };

            // Missing field behavior
            if (options.MissingField == CsvMissingFieldBehavior.Ignore)
            {
                // null means "do nothing" (no exception)
                cfg.MissingFieldFound = null;
            }

            // Bad data behavior
            if (options.BadData == CsvBadDataBehavior.Ignore)
            {
                // null means "do nothing" (no exception)
                cfg.BadDataFound = null;
            }

            // Header normalization
            if (options.PrepareHeader != null)
            {
                cfg.PrepareHeaderForMatch = args => options.PrepareHeader!(args.Header ?? string.Empty);
            }

            return cfg;
        }
    }
}
