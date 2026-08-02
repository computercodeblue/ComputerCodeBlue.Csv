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

---

## API Surface

### `CsvFile` — reads/writes a file at a given path

- `IEnumerable<T> Read<T>(string filePath, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAsync<T>(string filePath, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<T> ReadAnonymous<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAnonymousAsync<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)`
- `void Write<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null)`
- `Task WriteAsync<T>(string filePath, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)`

### `CsvStream` — reads/writes a caller-supplied `Stream` (never closed by this library)

- `IEnumerable<T> Read<T>(Stream stream, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAsync<T>(Stream stream, CsvOptions? options = null, CancellationToken ct = default)`
- `IEnumerable<T> ReadAnonymous<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null)`
- `IAsyncEnumerable<T> ReadAnonymousAsync<T>(Stream stream, T anonymousTypeTemplate, CsvOptions? options = null, CancellationToken ct = default)`
- `void Write<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null)`
- `Task WriteAsync<T>(Stream stream, IEnumerable<T> items, CsvOptions? options = null, CancellationToken ct = default)`

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
