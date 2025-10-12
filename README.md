# ComputerCodeBlue.Csv

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/) that make it easy to read and write CSV files with synchronous or asynchronous APIs.

This package is intended as a **small utility library** you can reuse across projects, instead of rewriting boilerplate around CsvHelper.

---

## Features

- **Read CSV (sync/async)**
  - `ReadCsv<T>(filePath)` → `IEnumerable<T>`
  - `ReadCsvAsync<T>(filePath)` → `IAsyncEnumerable<T>`

- **Write CSV (sync/async)**
  - `WriteCsv<T>(filePath, items)`
  - `WriteCsvAsync<T>(filePath, items)`

- Built on [CsvHelper](https://github.com/JoshClose/CsvHelper) with sensible defaults (`CultureInfo.InvariantCulture`).
- Optional `CsvConfiguration` parameter for full control.

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

### Reading (synchronous)
```csharp
using ComputerCodeBlue.Csv;

var people = CsvExtensions.ReadCsv<Person>("people.csv");

foreach (var person in people)
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Reading (asynchronous streaming)
```csharp
using ComputerCodeBlue.Csv;

await foreach (var person in CsvExtensions.ReadCsvAsync<Person>("people.csv"))
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Writing (synchronous)
```csharp
using ComputerCodeBlue.Csv;

var people = new List<Person>
{
    new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    new() { FirstName = "Bob", LastName = "Johnson", Age = 42 }
};

CsvExtensions.WriteCsv("people.csv", people);
```

### Writing (asynchronous)
```csharp
using ComputerCodeBlue.Csv;

await CsvExtensions.WriteCsvAsync("people.csv", people);
```

---

## API Reference

```csharp
IEnumerable<T> ReadCsv<T>(string filePath, CsvConfiguration? config = null);

IAsyncEnumerable<T> ReadCsvAsync<T>(
    string filePath,
    CsvConfiguration? config = null,
    CancellationToken ct = default);

void WriteCsv<T>(
    string filePath,
    IEnumerable<T> items,
    CsvConfiguration? config = null);

Task WriteCsvAsync<T>(
    string filePath,
    IEnumerable<T> items,
    CsvConfiguration? config = null,
    CancellationToken ct = default);
```

---

## License

MIT © Computer Code Blue LLC
