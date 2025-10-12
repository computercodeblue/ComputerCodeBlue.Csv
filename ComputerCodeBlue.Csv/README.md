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

### Read CSV
```csharp
using ComputerCodeBlue.Csv;

// Synchronous
var people = CsvExtensions.ReadCsv<Person>("people.csv");

// Asynchronous streaming
await foreach (var person in CsvExtensions.ReadCsvAsync<Person>("people.csv"))
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Write CSV
```csharp
using ComputerCodeBlue.Csv;

var people = new List<Person>
{
    new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
    new() { FirstName = "Bob", LastName = "Johnson", Age = 42 }
};

// Synchronous
CsvExtensions.WriteCsv("people.csv", people);

// Asynchronous
await CsvExtensions.WriteCsvAsync("people.csv", people);
```

---

## API Surface

- `IEnumerable<T> ReadCsv<T>(string filePath, CsvConfiguration? config = null)`
- `IAsyncEnumerable<T> ReadCsvAsync<T>(string filePath, CsvConfiguration? config = null, CancellationToken ct = default)`
- `void WriteCsv<T>(string filePath, IEnumerable<T> items, CsvConfiguration? config = null)`
- `Task WriteCsvAsync<T>(string filePath, IEnumerable<T> items, CsvConfiguration? config = null, CancellationToken ct = default)`

---

## License

MIT © Computer Code Blue LLC
