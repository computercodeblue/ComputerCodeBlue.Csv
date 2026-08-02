# ComputerCodeBlue.Csv

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/) that make it easy to read and write CSV files with synchronous or asynchronous APIs.

This package is intended as a **small utility library** you can reuse across projects, instead of rewriting boilerplate around CsvHelper.

---

## Repository layout

- `src/ComputerCodeBlue.Csv/` — the library
- `tests/ComputerCodeBlue.Csv.Tests/` — unit tests (xUnit)
- `artifacts/nupkgs/` — packed NuGet packages

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

void Write<T>(
    string filePath,
    IEnumerable<T> items,
    CsvOptions? options = null);

Task WriteAsync<T>(
    string filePath,
    IEnumerable<T> items,
    CsvOptions? options = null,
    CancellationToken ct = default);
```

### `CsvStream`

Same members as `CsvFile`, with `Stream stream` in place of `string filePath`.
None of them close or dispose the stream you pass in.

---

## License

MIT © Computer Code Blue LLC
