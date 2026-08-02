# CLAUDE.md

## ComputerCodeBlue.Csv

Lightweight extension methods around [CsvHelper](https://joshclose.github.io/CsvHelper/)
that make it easy to read and write CSV files with synchronous or asynchronous
APIs.

This package is intended as a **small utility library** you can reuse across
projects, instead of rewriting boilerplate around CsvHelper.

---

## ComputerCodeBlue.Csv Principles

ComputerCodeBlue.Csv removes common boilerplate when using CsvHelper.

ComputerCodeBlue.Csv should expose reusable abstractions rather than 
application-specific behavior.

---

## Design Philosophy

When making changes, follow these priorities in order:

1. Simplicity
2. Correctness
3. Maintainability
4. Performance

Do not introduce complexity unless there is measurable benefit.

Avoid premature optimization.

Favor explicit code over magic.

---

## Scope

ComputerCodeBlue.Csv intentionally exposes a small surface area.

Do not wrap every CsvHelper feature.

Only add APIs that eliminate meaningful boilerplate or improve common usage.

If an API simply mirrors CsvHelper without simplifying it, prefer using
CsvHelper directly.

---

## Architecture

ComputerCodeBlue.Csv is a thin wrapper around CsvHelper. The goal is 
convenience, not abstraction or replacement. As such, it is only a single
assembly.

---

## Development Philosophy

Assume this repository will exist for ten years.

Write code that another engineer can understand in five minutes.

Avoid creating abstractions for future requirements.

Only build what is currently needed.

---

## AI Expectations

Before making changes:

- Understand the surrounding code.
- Identify existing patterns.
- Extend existing patterns before creating new ones.

If a proposed design differs from existing conventions, explain why.

Never rewrite large portions of the project without justification.

Prefer incremental improvements.

---

## Coding Style

Prefer small classes.

Prefer small functions.

Functions should usually fit on one screen.

Avoid boolean flag parameters.

Avoid deep inheritance.

Prefer composition.

Prefer extension methods and static helpers over service abstractions unless
state or extensibility requires otherwise.

Avoid static mutable state.

---

## C#

Target the latest supported .NET LTS unless otherwise specified.

Enable nullable reference types.

Treat warnings as errors whenever practical.

Provide both synchronous and asynchronous APIs.

New public APIs should normally include both synchronous and asynchronous
variants unless one would be inappropriate.

Use cancellation tokens for long-running work.

Avoid synchronous blocking of async code.

Prefer records for immutable models.

Avoid unnecessary DI.

---

## API

Do not hide CsvHelper concepts unless they significantly simplify the common
case.

Prefer forwarding to CsvHelper rather than reimplementing CSV parsing behavior.

Public APIs should generally consist of one or two method calls around
CsvHelper.

Avoid breaking existing public APIs unless explicitly requested.

Favor additive API changes over modifying existing signatures.

Prefer extension methods over utility classes when they improve
discoverability.

---

## Testing

Every bug fix should include a regression test when practical.

Test observable behavior.

Do not test implementation details.

Use `MemoryStream` wherever practical instead of temporary files.

Test reading and writing using in-memory streams whenever possible.

---

## Security

Treat CSV input as untrusted.

Validate user-supplied file paths before opening files.

Use least privilege.

Never hardcode:

- passwords
- API keys
- client secrets
- tokens

---

## Dependencies

Minimize third-party dependencies.

Prefer Microsoft libraries when sufficient.

Before adding a package:

1. Is it necessary?
2. Is it maintained?
3. Can we reasonably implement the functionality ourselves?

---

## Documentation

Public APIs should be documented.

Complex algorithms deserve comments.

Simple code should not.

If the architecture changes, update documentation.

---

## Git

Keep commits focused.

Do not mix formatting with functional changes.

Avoid mass file rewrites.

Preserve file history whenever possible.

---

## Refactoring

Improve code when touching it.

Avoid drive-by refactors.

Large refactors should be proposed before implementation.

---

## Performance

Measure first.

Optimize second.

Document significant optimizations.

Readable code is preferred over micro-optimizations.

---

## Error Handling

Prefer explicit failures.

Do not silently swallow exceptions.

Prefer propagating CsvHelper exceptions unless additional context significantly
improves diagnostics.

Return meaningful error information.

---

## Agent Guidance

When uncertain:

- Ask questions.
- Do not invent APIs.
- Do not assume schemas.
- Search the repository first.

---

## Project Priorities

The project values:

- Reliability over features.
- Correctness over speed.
- Maintainability over cleverness.
- Consistency over novelty.
- Small improvements over rewrites.

Every contribution should leave the project slightly better than it was found.

---

## Repository Preferences

Preferred technologies

- C#
- .NET
- CsvHelper

Avoid introducing

- Additional dependencies
- Database requirements
- UI frameworks
- Node.js build tooling
