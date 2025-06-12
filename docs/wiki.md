# Wiki

## Overview

We've rebuilt the most popular FxCop rules and additional more rules as live analyzers to help you detect problems in your code and make it super easy for you to fix them on the spot with quick fixes. For more information, see [Overview of .NET source code analysis](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview).

## Apply a code fix for a rule

For information on applying code fixes, see [Quick Actions](https://learn.microsoft.com/visualstudio/ide/quick-actions).

> Note: not all code analysis rules have fixes associated with them.

## Configure a rule

- **Change a rule severity**: In a [configuration file](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files), add `dotnet_diagnostic.<rule_id>.severity = <severity>`. For more information, see [How to suppress code analysis warnings](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/suppress-warnings) and [Configuration options for code analysis](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-options).
- **Suppress a rule** – place your cursor inside the span of the squiggle and press `Ctrl+.` to trigger the lightbulb menu. From there you should see the option to Suppress in file (via #pragma) or via a global Suppression file. Alternatively, you can go to the Error List and right-click to suppress.
- **Bulk-suppression / Baselining** – To suppress all active issues, you can select all issues in the Error List, right-click, and navigate to “Suppress” or you can right-click on your project in the Solution Explorer and navigate to Analyze > Run Code Analysis and Suppress Active Issues.

![Suppress via error list](SuppressErrorList.PNG)

## Severity levels for analyzers

- `none` – rule violations at this severity level will not have a visual adornment in the editor nor appear in the Error List/scroll bar.
- `suggestion` – rule violations at this severity level will be adorned with a gray, dotted line and will appear as "Messages" in the Error List and gray boxes in the scroll bar.
- `warning` – rule violations at this severity level will be adorned with a green, squiggly line and will appear as "Warnings" in the Error List and green boxes in the scroll bar.
- `error` – rule violations at this severity level will be adorned with a red, squiggly line and will appear as "Errors" in the Error List and red boxes in the scroll bar.

## FAQ

- **Why am I only seeing rule violations in open files rather than my entire solution?**

    This is because "Full Solution Analysis" is off by default. To turn on live code analysis for your entire solution, go to Tools>Options>Text Editor> [C# | Visual Basic] > Advanced > Enable full solution analysis.

- **How do I report issues/bugs I find? How do I report a false-positive?**

    Please file any issues/bugs on our GitHub repository [dotnet/roslyn-analyzers](https://github.com/dotnet/roslyn-analyzers/issues).

- **Does this work with EditorConfig?**

    Yes. `.editorconfig` files are supported along with `.globalconfig` files. For more information, see [Configuration files for code analysis rules](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files).

- **Can I contribute analyzers and fixers to this repo?**

    YES! Create an issue with the rule/fix you want to contribute and we'll help you get started.
