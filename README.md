# ComputerCodeBlue.Csv

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/) that make it easy to read and write CSV files with synchronous or asynchronous APIs.

This package is intended as a **small utility library** you can reuse across projects, instead of rewriting boilerplate around CsvHelper.

---

## Why This Exists

Reading and writing CSV files is surprisingly hard. There are many edge cases that will break any quick-and-dirty parser you write yourself. Thankfully, there's CsvHelper, which I've used for many years. To use CsvHelper, I was creating the same boilerplate methods in order to get an API like System.IO.File where I can just use `CsvFile.Read()` or `CsvFile.Write()`. This project aims to put all of that into a reusable package.

---

## Repository layout

- `src/ComputerCodeBlue.Csv/` — the library
- `tests/ComputerCodeBlue.Csv.Tests/` — unit tests (xUnit)

---

## Features

- **Read CSV (sync/async)**
  - `CsvFile.Read<T>(filePath)` → `IEnumerable<T>`
  - `CsvFile.ReadAsync<T>(filePath)` → `IAsyncEnumerable<T>`
  - `CsvStream.Read<T>(stream)` / `CsvStream.ReadAsync<T>(stream)` — same, for a caller-owned `Stream`

- **Write CSV (sync/async)**
  - `CsvFile.Write<T>(filePath, items)`
  - `CsvFile.WriteAsync<T>(filePath, items)`
  - `CsvStream.Write<T>(stream, items)` / `CsvStream.WriteAsync<T>(stream, items)`

- **Anonymous types**
  - `CsvFile.ReadAnonymous(filePath, template)` / `CsvStream.ReadAnonymous(stream, template)` (plus async variants) — read into an anonymous type by passing a throwaway instance of the desired shape.

- **Dynamic/loosely-typed rows** (no fixed record type at all — e.g. template/merge-field substitution)
  - `CsvFile.ReadDynamic(filePath)` / `CsvStream.ReadDynamic(stream)` (plus async variants) — each row as `Dictionary<string, string>`, keyed by header name, raw field values.
  - `CsvFile.WriteDynamic(filePath, headers, items)` / `CsvStream.WriteDynamic(stream, headers, items)` (plus async variants) — `items` are `IDictionary<string, object?>`. Exists because CsvHelper's own dynamic write support writes no header at all for an empty sequence, and matches fields by enumeration order rather than by name (two same-shaped records added in a different key order silently land in the wrong columns). This always writes the given headers and looks each value up by name.

- `CsvStream` never closes the `Stream` you pass it — you own its lifetime.
- Built on [CsvHelper](https://github.com/JoshClose/CsvHelper) with sensible defaults (`CultureInfo.InvariantCulture`).
- Optional `CsvOptions` parameter for full control.

---

## Installation

```PowerShell
dotnet add package ComputerCodeBlue.Csv
```

Or reference the project directly in your solution.

---

## Usage

### Define a model
```csharp
public class Person
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public int Age { get; set; }
}
```

### Reading a file (synchronous)
```csharp
using ComputerCodeBlue.Csv;

var people = CsvFile.Read<Person>("people.csv");

foreach (var person in people)
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Reading a file (asynchronous streaming)
```csharp
using ComputerCodeBlue.Csv;

await foreach (var person in CsvFile.ReadAsync<Person>("people.csv"))
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Writing a file (synchronous)
```csharp
using ComputerCodeBlue.Csv;

var people = new List<Person>
{
    new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    new() { FirstName = "Bob", LastName = "Johnson", Age = 42 }
};

CsvFile.Write("people.csv", people);
```

### Writing a file (asynchronous)
```csharp
using ComputerCodeBlue.Csv;

await CsvFile.WriteAsync("people.csv", people);
```

### Reading/writing a stream

`CsvStream` mirrors `CsvFile` for a caller-supplied `Stream` (an HTTP response
body, a `MemoryStream`, a network stream, ...). It never closes the stream —
you retain ownership and are responsible for disposing it.

```csharp
using ComputerCodeBlue.Csv;

using var stream = new MemoryStream();
CsvStream.Write(stream, people);

stream.Position = 0;
var peopleFromStream = CsvStream.Read<Person>(stream);
```

### Anonymous types

Pass a throwaway instance of the desired shape as a template; its values are
ignored, only its property names/types are used to match CSV headers:

```csharp
using ComputerCodeBlue.Csv;

var rows = CsvFile.ReadAnonymous("people.csv", new { FirstName = "", LastName = "", Age = 0 });
```

### Dynamic/loosely-typed rows

For cases with no fixed record type at all - e.g. reading an arbitrary CSV
for template/merge-field substitution, where the columns aren't known until
runtime. `ReadDynamic` returns each row as `Dictionary<string, string>`
keyed by the file's actual header names, with raw (unconverted) field
values - a zip code like `"07030"` stays a string, not `7030`:

```csharp
using ComputerCodeBlue.Csv;

foreach (var row in CsvFile.ReadDynamic("people.csv"))
{
    Console.WriteLine($"{row["FirstName"]} {row["LastName"]}");
}
```

`WriteDynamic` takes an explicit column list plus items shaped as
`IDictionary<string, object?>`. It exists because CsvHelper's own dynamic
write support has two sharp edges: an empty `IEnumerable<object>`/
`IEnumerable<dynamic>` writes no header at all, and even where it does
write, fields are matched by enumeration order rather than by name - two
records with the same keys added in a different order silently land in the
wrong columns. `WriteDynamic` always writes the given headers and looks
each item's value up by column name, so neither failure mode can happen:

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

## API Reference

### `CsvFile`

```csharp
IEnumerable<T> Read<T>(string filePath, CsvOptions? options = null);

IAsyncEnumerable<T> ReadAsync<T>(
    string filePath,
    CsvOptions? options = null,
    CancellationToken ct = default);

IEnumerable<T> ReadAnonymous<T>(string filePath, T anonymousTypeTemplate, CsvOptions? options = null);

IAsyncEnumerable<T> ReadAnonymousAsync<T>(
    string filePath,
    T anonymousTypeTemplate,
    CsvOptions? options = null,
    CancellationToken ct = default);

IEnumerable<IDictionary<string, string>> ReadDynamic(string filePath, CsvOptions? options = null);

IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(
    string filePath,
    CsvOptions? options = null,
    CancellationToken ct = default);

void Write<T>(
    string filePath,
    IEnumerable<T> items,
    CsvOptions? options = null);

Task WriteAsync<T>(
    string filePath,
    IEnumerable<T> items,
    CsvOptions? options = null,
    CancellationToken ct = default);

void WriteDynamic(
    string filePath,
    IEnumerable<string> headers,
    IEnumerable<IDictionary<string, object?>> items,
    CsvOptions? options = null);

Task WriteDynamicAsync(
    string filePath,
    IEnumerable<string> headers,
    IEnumerable<IDictionary<string, object?>> items,
    CsvOptions? options = null,
    CancellationToken ct = default);
```

### `CsvStream`

Same members as `CsvFile`, with `Stream stream` in place of `string filePath`.
None of them close or dispose the stream you pass in.

---

## License

MIT © Computer Code Blue LLC
