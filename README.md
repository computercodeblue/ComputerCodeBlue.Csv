# ComputerCodeBlue.Csv

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/) that make it easy to read and write CSV files with synchronous or asynchronous APIs.

This package is intended as a **small utility library** you can reuse across projects, instead of rewriting boilerplate around CsvHelper.

---

## Features

- **Read CSV (sync/async)**
  - `CsvFile.Read<T>(filePath)` → `IEnumerable<T>`
  - `CsvFile.ReadAsync<T>(filePath)` → `IAsyncEnumerable<T>`

- **Write CSV (sync/async)**
  - `CsvFile.Write<T>(filePath, items)`
  - `CsvFile.WriteAsync<T>(filePath, items)`

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

var people = CsvFile.Read<Person>("people.csv");

foreach (var person in people)
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Reading (asynchronous streaming)
```csharp
using ComputerCodeBlue.Csv;

await foreach (var person in CsvFile.ReadAsync<Person>("people.csv"))
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

CsvFile.Write("people.csv", people);
```

### Writing (asynchronous)
```csharp
using ComputerCodeBlue.Csv;

await CsvFile.WriteAsync("people.csv", people);
```

---

## API Reference

```csharp
IEnumerable<T> Read<T>(string filePath, CsvConfiguration? config = null);

IAsyncEnumerable<T> ReadAsync<T>(
    string filePath,
    CsvConfiguration? config = null,
    CancellationToken ct = default);

void Write<T>(
    string filePath,
    IEnumerable<T> items,
    CsvConfiguration? config = null);

Task WriteAsync<T>(
    string filePath,
    IEnumerable<T> items,
    CsvConfiguration? config = null,
    CancellationToken ct = default);
```

---

## License

MIT © Computer Code Blue LLC
