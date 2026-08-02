# ComputerCodeBlue.Csv

[![NuGet](https://img.shields.io/nuget/v/ComputerCodeBlue.Csv.svg)](https://www.nuget.org/packages/ComputerCodeBlue.Csv/)
[![Downloads](https://img.shields.io/nuget/dt/ComputerCodeBlue.Csv.svg)](https://www.nuget.org/packages/ComputerCodeBlue.Csv/)

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/) that make it easy to read and write CSV files with synchronous or asynchronous APIs.

---

## Installation

```powershell
dotnet add package ComputerCodeBlue.Csv
```

---

## Quick Start

### Define a model
```csharp
public class Person
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public int Age { get; set; }
}
```

### Read CSV (file)
```csharp
using ComputerCodeBlue.Csv;

// Synchronous
var people = CsvFile.Read<Person>("people.csv");

// Asynchronous streaming
await foreach (var person in CsvFile.ReadAsync<Person>("people.csv"))
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Write CSV (file)
```csharp
using ComputerCodeBlue.Csv;

var people = new List<Person>
{
    new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    new() { FirstName = "Bob", LastName = "Johnson", Age = 42 }
};

// Synchronous
CsvFile.Write("people.csv", people);

// Asynchronous
await CsvFile.WriteAsync("people.csv", people);
```

### Read/write CSV (stream)

`CsvStream` mirrors `CsvFile` for callers that already have a `Stream` (an HTTP
response body, a `MemoryStream`, a network stream, ...). It never closes the
stream you pass in — you retain ownership and are responsible for disposing it.

```csharp
using ComputerCodeBlue.Csv;

// Write
using var stream = new MemoryStream();
CsvStream.Write(stream, people);

// Read (from the same stream, since it's still open)
stream.Position = 0;
var peopleFromStream = CsvStream.Read<Person>(stream);
```

### Anonymous types

`ReadAnonymous`/`ReadAnonymousAsync` read into an anonymous type. Since you
can't spell an anonymous type as a generic argument, pass a throwaway
instance of the desired shape — its values are ignored, only its property
names/types are used to match CSV headers:

```csharp
using ComputerCodeBlue.Csv;

var rows = CsvFile.ReadAnonymous("people.csv", new { FirstName = "", LastName = "", Age = 0 });
```

### Dynamic/loosely-typed rows

`ReadDynamic`/`WriteDynamic` (and their async variants) are for cases with
no fixed record type at all — e.g. reading an arbitrary CSV for template or
merge-field substitution, where the columns aren't known until runtime.

`ReadDynamic` returns each row as a `Dictionary<string, string>` keyed by
the file's actual header names, with raw (unconverted) field values — a zip
code like `"07030"` stays a string, not `7030`:

```csharp
using ComputerCodeBlue.Csv;

foreach (var row in CsvFile.ReadDynamic("people.csv"))
{
    Console.WriteLine($"{row["FirstName"]} {row["LastName"]}");
}
```

`WriteDynamic` takes an explicit column list plus items shaped as
`IDictionary<string, object?>` (e.g. `Dictionary<string, object?>`, or an
`ExpandoObject` cast to that interface). This exists because CsvHelper's own
dynamic write support has two sharp edges: writing an empty
`IEnumerable<object>`/`IEnumerable<dynamic>` produces no header at all (there's
no record left to infer columns from), and even where it does write, it
matches fields by enumeration order, not by name — two records with the same
keys added in a different order silently land in the wrong columns.
`WriteDynamic` always writes the given headers, and looks each item's value
up by column name, so neither of those failure modes can happen:

```csharp
using ComputerCodeBlue.Csv;

var headers = new[] { "FirstName", "LastName", "Age" };
var rows = new List<IDictionary<string, object?>>
{
    new Dictionary<string, object?> { ["FirstName"] = "Alice", ["LastName"] = "Smith", ["Age"] = 30 },
};

CsvFile.WriteDynamic("people.csv", headers, rows);
```

A row missing a declared column writes a blank cell, or throws if
`CsvOptions.MissingField` is `CsvMissingFieldBehavior.Throw`.

---

## API Surface

### `CsvFile` — reads/writes a file at a given path

- `IEnumerable<T> Read<T>(string filePath, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAsync<T>(string filePath, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<T> ReadAnonymous<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAnonymousAsync<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<IDictionary<string, string>> ReadDynamic(string filePath, CsvOptions? options = null)`
- `IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(string filePath, CsvOptions? options = null, CancellationToken ct = default)`
- `void Write<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null)`
- `Task WriteAsync<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)`
- `void WriteDynamic(string filePath, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null)`
- `Task WriteDynamicAsync(string filePath, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null, CancellationToken ct = default)`

### `CsvStream` — reads/writes a caller-supplied `Stream` (never closed by this library)

- `IEnumerable<T> Read<T>(Stream stream, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAsync<T>(Stream stream, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<T> ReadAnonymous<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAnonymousAsync<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<IDictionary<string, string>> ReadDynamic(Stream stream, CsvOptions? options = null)`
- `IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(Stream stream, CsvOptions? options = null, CancellationToken ct = default)`
- `void Write<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null)`
- `Task WriteAsync<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)`
- `void WriteDynamic(Stream stream, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null)`
- `Task WriteDynamicAsync(Stream stream, IEnumerable<string> headers, IEnumerable<IDictionary<string, object?>> items, CsvOptions? options = null, CancellationToken ct = default)`

### `CsvOptions` — optional configuration, forwarded to CsvHelper

| Property | Default | Meaning |
|---|---|---|
| `Culture` | `CultureInfo.InvariantCulture` | Culture used for parsing/formatting. |
| `DetectDelimiter` | `true` | Auto-detect the delimiter. |
| `Trim` | `CsvTrimOptions.Trim` | How fields are trimmed. |
| `IgnoreBlankLines` | `true` | Skip blank lines. |
| `MissingField` | `CsvMissingFieldBehavior.Ignore` | Throw or ignore missing fields. |
| `BadData` | `CsvBadDataBehavior.Ignore` | Throw or ignore bad data. |
| `HasHeaderRecord` | `true` | Whether the CSV has a header row. |
| `PrepareHeader` | `null` | Optional header normalizer used when matching properties to columns. |
| `Delimiter` | `null` | Explicit delimiter; unused when `DetectDelimiter` is `true`. |

---

## License

MIT © Computer Code Blue LLC
