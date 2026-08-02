# Changelog

All notable changes to this project are documented in this file.

## [1.3.0] - 2026-08-02

### Added
- `CsvFile`/`CsvStream`: `ReadAnonymous`/`ReadAnonymousAsync` read into an anonymous type by
  passing a throwaway instance of the desired shape so the compiler can infer the type; CsvHelper
  matches its property names to the CSV headers.
- `CsvFile`/`CsvStream`: `ReadDynamic`/`ReadDynamicAsync` for cases with no fixed record type at
  all - each row comes back as `Dictionary<string, string>` keyed by the file's actual header
  names, with raw (unconverted) field values.
- `CsvFile`/`CsvStream`: `WriteDynamic`/`WriteDynamicAsync` write an explicit column list plus
  items shaped as `IDictionary<string, object?>`. Unlike CsvHelper's own dynamic write support,
  this always writes the given headers (even for an empty sequence) and looks each item's value
  up by column name rather than by enumeration order, so same-shaped or differently-shaped items
  can't silently land in the wrong column. A missing key writes a blank cell, or throws if
  `CsvOptions.MissingField` is `CsvMissingFieldBehavior.Throw`.

### Fixed
- `CsvStream`: `Read`/`ReadAsync`/`Write`/`WriteAsync` no longer close the caller-supplied
  `Stream` as a side effect. The caller creates the stream, so the caller now retains ownership
  of disposing it, matching `Write`/`WriteAsync`'s doc comments and typical .NET API conventions.

### Changed
- Repository restructured into `src/` and `tests/` directories, with an accompanying
  `ComputerCodeBlue.Csv.Tests` xUnit project.
- Bumped `Microsoft.SourceLink.GitHub` to 10.0.301.
