# Dynamic-shaped CSV support

Status: **Implemented.**

## Motivation

This work is driven by Karl (a mail library / command-line tool), which needs
to read CSV files as loosely-typed row data for template substitution
(`{{FirstName}}`, `{{Balance}}`, ...) and, potentially, write CSV reports back
out. Karl doesn't have a fixed record type to hand `Read<T>`/`Write<T>` — it
needs to work against whatever columns happen to be in a given file.

## Background: what already works today

`Read<T>`/`Write<T>` already support `dynamic` and `object` with **zero
code changes**, because `dynamic` is erased to `System.Object` in compiled
IL — `Read<dynamic>(...)` and `Read<object>(...)` are the same generic
instantiation. CsvHelper has its own built-in handling for this case:

- **Read**: `GetRecords<T>()` with `T == object` returns
  `CsvHelper.FastDynamicObject` instances. Dynamic member access
  (`row.Name`) works, and every field value comes back as a raw
  `System.String` — CsvHelper does **not** do type inference on dynamic
  reads, so there's no fidelity risk (a zip code `"07030"` stays
  `"07030"`, not `7030`).
- **Write**: `WriteRecords(IEnumerable<object>)` also has a per-record
  dynamic path, but it is narrower and less safe than it first appears
  (see below).

## Problems identified (verified empirically against CsvHelper 33.1.0)

### 1. Empty dynamic collections write no header

`Write<Person>(stream, new List<Person>())` (concrete `T`) correctly writes
just the header row, because CsvHelper knows the columns from `typeof(T)`
independent of row count.

`Write<object>`/`Write<dynamic>` (or `CsvStream.Write` with a `dynamic`
item type) on an **empty** collection writes nothing at all — not even a
header. CsvHelper's dynamic path determines columns by reflecting on the
*first record*, so with zero records there is nothing to reflect on.

### 2. CsvHelper's dynamic write path requires `IDynamicMetaObjectProvider`, not `IDictionary<string, object>`

Only genuine dynamic-dispatch objects work — `ExpandoObject`, or
CsvHelper's own `FastDynamicObject`. A plain `Dictionary<string, object>`
implements `IDictionary<string, object>` but **not**
`IDynamicMetaObjectProvider`, and throws even when boxed as `object`:

```
ConfigurationException: Types that inherit IEnumerable cannot be auto mapped.
```

CsvHelper's AutoMap step rejects any concrete record type that implements
`IEnumerable`, which `Dictionary<TKey,TValue>` does via
`IEnumerable<KeyValuePair<...>>`. This was confirmed with a custom class
that implements only `IDictionary<string, object>` (no
`IDynamicMetaObjectProvider`) — same exception.

### 3. Even where it works, it's positional, not name-matched

Two `ExpandoObject` records with the **same keys**, added in a different
order:

```csharp
row1.Name = "Alice"; row1.Age = 30;
row2.Age = 42;       row2.Name = "Bob";   // reversed insertion order
```

produced:

```
Name,Age
Alice,30
42,Bob
```

`Age`'s value landed in the `Name` column. CsvHelper writes each record's
members in whatever order it enumerates them, and never reconciles that
against the header it already committed to from record 1. This is a
silent-corruption risk, not just a "mixed shape" edge case — it can bite
same-shaped rows too, e.g. anything built conditionally or deserialized
from JSON into an `ExpandoObject`, where member order isn't guaranteed.

**Conclusion:** this is a real gap in CsvHelper, not something it already
handles under a different name. Confirms the suspicion raised in
conversation — CsvHelper leaves this to the caller.

## Proposed API

### `WriteDynamic` / `WriteDynamicAsync`

Added to both `CsvFile` and `CsvStream`:

```csharp
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

(`CsvStream` variants take `Stream stream` in place of `string filePath`.)

**Behavior:**
- Writes `headers` as the header row, always — independent of whether
  `items` is empty. Fixes problem #1.
- For each item, looks up each declared header **by key**
  (`item.TryGetValue(header, out var value)`) rather than by writing
  whatever the record happens to enumerate. Fixes problem #3.
- A record missing a declared key writes a blank cell by default. This
  reuses the existing `CsvOptions.MissingField` setting
  (`Ignore` → blank cell, `Throw` → throws) instead of introducing a new
  boolean parameter, keeping one consistent knob for "missing field"
  semantics across read and write.
- Accepts `IDictionary<string, object?>` rather than `dynamic`/`object`,
  which is strictly *more* capable than CsvHelper's own dynamic write
  path: plain `Dictionary<string, object>` works (problem #2 doesn't
  apply, since we never route through CsvHelper's `IEnumerable<object>`
  AutoMap path — we write field-by-field ourselves).
- Both `ExpandoObject` and `CsvHelper.FastDynamicObject` (what
  `Read<dynamic>` returns) already implement `IDictionary<string, object>`
  (verified), so a `Read<dynamic>` → transform → `WriteDynamic` round trip
  needs only `.Cast<IDictionary<string, object?>>()`, no conversion step.

**Implementation note:** unlike every other method in this library, this
can't be a one- or two-line forward to `csv.WriteRecords(...)`. It drops to
CsvHelper's low-level `WriteField`/`NextRecord()` API: write each header,
call `NextRecord()`, then for each item look up and write each header's
value, call `NextRecord()`. This was prototyped and confirmed to work,
including the empty and mixed-shape cases.

### `ReadDynamic` / `ReadDynamicAsync`

Added to both `CsvFile` and `CsvStream`, mirroring `Read`/`ReadAsync`:

```csharp
IEnumerable<IDictionary<string, string>> ReadDynamic(
    string filePath,
    CsvOptions? options = null);

IAsyncEnumerable<IDictionary<string, string>> ReadDynamicAsync(
    string filePath,
    CsvOptions? options = null,
    CancellationToken ct = default);
```

**Behavior:**
- No `headers` parameter needed — unlike write, read has no ambiguity to
  resolve. A CSV file (with `HasHeaderRecord: true`) already has one real
  header row; that's the schema.
- Each row becomes a `Dictionary<string, string>` keyed by the actual CSV
  header names — the shape a template engine wants directly, no
  per-caller boilerplate turning rows into a lookup.
- Built directly off CsvHelper's `HeaderRecord`/`GetField(header)`, not
  via `Read<dynamic>`/`FastDynamicObject` — avoids constructing a dynamic
  wrapper only to immediately flatten it into a plain dictionary.
  Verified `GetField` and `FastDynamicObject` return identical raw string
  values, so this is not a fidelity trade-off, just a simpler path.
- A missing/null field becomes `""`, not `null` — the project builds with
  `<Nullable>enable</Nullable>`, and `IDictionary<string, string>`'s
  non-nullable value type shouldn't hold nulls.

## Non-goals

- No changes to `Read<T>`/`Read<dynamic>` — reading into `dynamic`/`object`
  already works correctly today (problem #3 is write-only: a single CSV
  file can only have one header row, so there's no "mixed shape" ambiguity
  when reading).
- No attempt to make CsvHelper's own `WriteRecords(IEnumerable<object>)`
  path safer or to detect/warn on its positional behavior — `WriteDynamic`
  is a separate, additive method, not a patch over the existing one.

## Testing plan

- `WriteDynamic`: empty collection writes header only; mixed-shape items
  write blanks for missing keys, not misaligned values; same-shape items
  with different key insertion order still align correctly by name;
  `CsvOptions.MissingField = Throw` throws on a missing key instead of
  writing blank.
- `ReadDynamic`: basic round trip; a field value with a leading zero
  (e.g. a zip code) round-trips unchanged.
- Round trip: `ReadDynamic` → mutate a couple of values → `WriteDynamic`
  using the same header list.
