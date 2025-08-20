# GEMINI.md

This file provides guidance for the Gemini CLI agent when working with this repository.

## Running Tests

To run all unit tests in the solution, use the following command:

```bash
dotnet test
```

## Code Formatting

The Gemini CLI agent is configured to automatically format C# code using CSharpier after making modifications.

If you need to manually format the entire codebase, use:

```bash
dotnet csharpier format .
```

To check for formatting issues without applying changes:

```bash
dotnet csharpier check .
```