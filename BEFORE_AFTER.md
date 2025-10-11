# Before and After Examples

## The Problem: Redundant Regex Matching

When you use `Regex.IsMatch` to check if a pattern matches, and then immediately call `Regex.Match` with the same arguments, you're performing the regex matching **twice**.

### ❌ Before (Inefficient - Matches Twice)

```csharp
using System.Text.RegularExpressions;

string input = "The year is 2024";
string pattern = @"\d+";

// First regex match happens here
if (Regex.IsMatch(input, pattern))
{
    // Second regex match happens here - REDUNDANT!
    Match m = Regex.Match(input, pattern);
    Console.WriteLine($"Found: {m.Value}");
}
```

**Problems:**
- Regex engine runs **twice** on the same input
- Wastes CPU cycles
- Allocates more memory
- Takes more time

---

## The Solution: Pattern Matching

Use `Regex.Match` once with C# pattern matching to check success and capture the match simultaneously.

### ✅ After (Efficient - Matches Once)

```csharp
using System.Text.RegularExpressions;

string input = "The year is 2024";
string pattern = @"\d+";

// Only ONE regex match happens here
if (Regex.Match(input, pattern) is { Success: true } m)
{
    // Match object 'm' is already available
    Console.WriteLine($"Found: {m.Value}");
}
```

**Benefits:**
- Regex engine runs **once**
- ~50% reduction in regex matching overhead
- Modern C# pattern matching syntax
- More concise code
- Match object immediately available

---

## More Examples

### Example 2: With RegexOptions

#### ❌ Before
```csharp
if (Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
{
    Match m = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
    string domain = m.Groups["domain"].Value;
}
```

#### ✅ After
```csharp
if (Regex.Match(email, pattern, RegexOptions.IgnoreCase) is { Success: true } m)
{
    string domain = m.Groups["domain"].Value;
}
```

---

### Example 3: Multiple Uses of Match

#### ❌ Before
```csharp
if (Regex.IsMatch(text, pattern))
{
    Match m = Regex.Match(text, pattern);
    Console.WriteLine($"Value: {m.Value}");
    Console.WriteLine($"Index: {m.Index}");
    Console.WriteLine($"Length: {m.Length}");
}
```

#### ✅ After
```csharp
if (Regex.Match(text, pattern) is { Success: true } m)
{
    Console.WriteLine($"Value: {m.Value}");
    Console.WriteLine($"Index: {m.Index}");
    Console.WriteLine($"Length: {m.Length}");
}
```

---

## How the Analyzer Helps

### 1. **Automatic Detection**
The analyzer scans your code and identifies the pattern:
- `if (Regex.IsMatch(...))`
- followed by `Match m = Regex.Match(...)`
- with the same arguments

### 2. **Visual Indication**
In your IDE, you'll see:
- 💡 Lightbulb or screwdriver icon
- Info-level diagnostic: **REGEX001**
- Message: "Use Regex.Match with pattern matching instead of Regex.IsMatch guard"

### 3. **Automatic Fix**
Press `Ctrl+.` (or `Cmd+.` on Mac) and select:
- "Replace with Regex.Match pattern matching"

The code is automatically refactored!

---

## Performance Impact

For a typical regex operation:

**Before:**
```
Regex.IsMatch: ~X ms
Regex.Match:   ~X ms
Total:         ~2X ms
```

**After:**
```
Regex.Match:   ~X ms
Total:         ~X ms
```

**Savings: ~50% of regex matching time**

For frequently executed code paths or complex regex patterns, this can result in significant performance improvements.

---

## Pattern Matching Explained

The pattern `is { Success: true } m` combines:
- **Type checking**: `is` checks if the result is a Match
- **Property pattern**: `{ Success: true }` checks if Success property is true
- **Variable declaration**: `m` captures the Match object for use in the block

This is equivalent to:
```csharp
Match m = Regex.Match(input, pattern);
if (m.Success)
{
    // Use m
}
```

But more concise and performant when combined with the if statement!
