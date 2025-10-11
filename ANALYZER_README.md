# Regex Analyzer

This analyzer detects and fixes a redundant pattern where `Regex.IsMatch` is used to guard a `Regex.Match` call with the same arguments.

## Problem

The following code performs redundant work, as both `Regex.IsMatch` and `Regex.Match` perform regex matching:

```csharp
if (Regex.IsMatch(input, pattern))
{
    Match m = Regex.Match(input, pattern);
    // Use m
}
```

## Solution

The analyzer suggests using pattern matching with `Regex.Match` directly:

```csharp
if (Regex.Match(input, pattern) is { Success: true } m)
{
    // Use m
}
```

This approach:
- Performs the regex match only once
- Uses modern C# pattern matching
- Provides better performance
- Makes the code more concise

## Diagnostic ID

**REGEX001**: Use Regex.Match with pattern matching instead of Regex.IsMatch guard

## Configuration

The analyzer is enabled by default and produces an Info-level diagnostic.

## Code Fix

The analyzer includes an automatic code fix that:
1. Replaces the `Regex.IsMatch` condition with `Regex.Match` and pattern matching
2. Removes the redundant `Match` declaration from the if body
3. Preserves the variable name used in the original code

## Examples

### Basic Case
**Before:**
```csharp
if (Regex.IsMatch(input, pattern))
{
    Match m = Regex.Match(input, pattern);
    Console.WriteLine(m.Value);
}
```

**After:**
```csharp
if (Regex.Match(input, pattern) is { Success: true } m)
{
    Console.WriteLine(m.Value);
}
```

### With RegexOptions
**Before:**
```csharp
if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
{
    Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
    Console.WriteLine(m.Value);
}
```

**After:**
```csharp
if (Regex.Match(input, pattern, RegexOptions.IgnoreCase) is { Success: true } m)
{
    Console.WriteLine(m.Value);
}
```

## Building and Testing

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

## References

- [dotnet/runtime#111239](https://github.com/dotnet/runtime/issues/111239) - Original issue tracking this analyzer
