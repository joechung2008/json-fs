# json-fs

## License

MIT

## Reference

[json.org](http://json.org)

## Building

To build the entire solution (including CLI, Shared library, and tests):

```bash
dotnet build
```

To build only the CLI application:

```bash
dotnet build CLI/CLI.fsproj
```

To build the Shared library:

```bash
dotnet build Shared/Shared.fsproj
```

To build and run tests:

```bash
dotnet build Shared.Tests/Shared.Tests.fsproj
dotnet test Shared.Tests/Shared.Tests.fsproj
```

## Code Formatting

This project uses [Fantomas](https://github.com/fsprojects/fantomas) for F# code formatting.

### Installation

Install Fantomas globally using the .NET CLI:

```bash
dotnet tool install -g fantomas
```

Alternatively, if Fantomas is already installed in your environment, you can use it directly.

### Checking Formatting

To check if all F# files in the project are properly formatted without making changes:

```bash
fantomas --check .
```

This command will exit with a non-zero code if any files need formatting.

### Fixing Formatting

To automatically format all F# files in the project:

```bash
fantomas .
```

This will reformat all `.fs` and `.fsx` files in the current directory and subdirectories according to Fantomas' style rules.

## Running the CLI

The CLI is a JSON pretty-printer that reads JSON from standard input and outputs formatted JSON to standard output.

### Prerequisites

- .NET 8.0 SDK (or later)

### Interactive Mode

Run the CLI and enter JSON directly, then press Ctrl+D (Linux/macOS) or Ctrl+Z followed by Enter (Windows) to end input:

```bash
dotnet run --project CLI/CLI.fsproj
```

### Piping from a File

Pipe JSON content from a file:

**Linux/macOS:**

```bash
cat example.json | dotnet run --project CLI/CLI.fsproj
```

**Windows:**

```bash
type example.json | dotnet run --project CLI/CLI.fsproj
```

### Piping from a Command

Pipe output from other commands:

```bash
echo '{"name": "example", "value": 123}' | dotnet run --project CLI/CLI.fsproj
```

### Example

```bash
$ echo '{"key":"value","array":[1,2,3]}' | dotnet run --project CLI/CLI.fsproj
{
  "key": "value",
  "array": [
    1,
    2,
    3
  ]
}
```
