# Using the Regex Analyzer

## Installation

To use the analyzer in your project, add a project reference to the analyzer:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/RegexAnalyzer.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Or, when packaged as a NuGet package:

```xml
<ItemGroup>
  <PackageReference Include="RegexAnalyzer" Version="1.0.0" />
</ItemGroup>
```

## Viewing Diagnostics

The analyzer produces **Info**-level diagnostics (REGEX001), which means:

- **In IDEs** (Visual Studio, VS Code, etc.): The diagnostic will appear as an information message with a lightbulb offering a code fix
- **In CLI builds**: Info diagnostics are not shown by default to avoid cluttering build output

To see all diagnostics during a CLI build, you can:

```bash
# Option 1: Use detailed build verbosity
dotnet build --verbosity detailed

# Option 2: Use an .editorconfig file to promote the diagnostic to Warning
```

## Using in an IDE

When you open a project with the analyzer in Visual Studio or VS Code:

1. The analyzer will run automatically on your code
2. When it detects the pattern, you'll see a lightbulb (💡) or screwdriver icon
3. Click the icon or press `Ctrl+.` (Windows/Linux) or `Cmd+.` (Mac)
4. Select "Replace with Regex.Match pattern matching"
5. The code will be automatically refactored

## Example .editorconfig

To make the diagnostic more visible, add this to your `.editorconfig`:

```ini
[*.cs]
# Promote REGEX001 from Info to Warning
dotnet_diagnostic.REGEX001.severity = warning
```

## Demo Project

The `demo` folder contains a sample console application that demonstrates the pattern the analyzer detects. Open this project in your IDE to see the analyzer in action.

## Testing

All analyzer functionality is verified by comprehensive unit tests:

```bash
cd /path/to/roslyn-analyzers
dotnet test
```

All 10 tests should pass, covering:
- Detection of the Regex.IsMatch + Match pattern
- Code fix transformation
- Edge cases (different arguments, no Match call, etc.)
