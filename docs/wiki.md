## Overview
We’ve rebuilt the most popular 100+ FxCop rules as live analyzers to help you detect problems in your code and make it super easy for you to fix them on the spot with quick fixes. Installing the NuGet or Extension gives you the same great code analysis as before with FXCop but now the analysis is live as-you-type and has quick-fixes.

## Install Instructions
You can download the FXCop live code analyzers as a [NuGet package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/) or as a Visual Studio extension ([VS2017](https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2017) or [VS2019](https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2019)).

## Apply a Code Fix for a Rule
Place your cursor in the span of the squiggle/dotted line and press `Ctrl+.` to trigger the lightbulb menu. From the lightbulb, you will see all available quick actions.
> Note: not all 100 code analysis rules in this extension have fixes associated with them.

<img src="https://github.com/dotnet/roslyn-analyzers/blob/main/docs/MakeStatic.PNG">

## Configure a Rule (Enable, Disable, Suppress, Baseline)
* **Configure a rule on/off and change severity** – in the Solution Explorer, navigate to the "References" node under your project. Right-click on the Analyzers sub-node and select "Open Active Rule Set". Using the GUI, you can set the severity of any rule to "None", "Info", "Warning", or "Error". Note: If you have a .NET Core project, you must manually add a file name “ProjectName.ruleset”. 
* **Suppress a rule** – place your cursor inside the span of the squiggle and press “Ctrl+.” to trigger the lightbulb menu. From there you should see the option to Suppress in file (via #pragma) or via a global Suppression file. Alternatively, you can go to the Error List and right-click to suppress. 
* **Bulk-suppression / Baselining** – To suppress all active issues, you can select all issues in the Error List, right-click, and navigate to “Suppress” or you can right-click on your project in the Solution Explorer and navigate to Analyze > Run Code Analysis and Suppress Active Issues. 

<img src = "https://github.com/dotnet/roslyn-analyzers/blob/main/docs/SuppressErrorList.PNG">

## Severity Levels for Analyzers
* **None** – rule violations at this severity level will not have a visual adornment in the editor nor appear in the Error List/scroll bar.
* **Suggestion** – rule violations at this severity level will be adorned with a gray, dotted line and will appear as "Messages" in the Error List and gray boxes in the scroll bar.
* **Warning** – rule violations at this severity level will be adorned with a green, squiggly line and will appear as "Warnings" in the Error List and green boxes in the scroll bar. 
* **Error** – rule violations at this severity level will be adorned with a red, squiggly line and will appear as "Errors" in the Error List and red boxes in the scroll bar. 
 
## FAQ
* **Why am I only seeing rule violations in open files rather than my entire solution?**

This is because "Full Solution Analysis" is off by default. To turn on live code analysis for your entire solution, go to Tools>Options>Text Editor> [C# | Basic] > Advanced > Enable full solution analysis.

* **Do I need a .ruleset file in every project?**

Yes. Ruleset files work on a per-project basis. When we move to EditorConfig, the configuration file will apply at the directory-level.

* **How do I report issues/bugs I find? How do I report a false-positive?**

Please file any issues/bugs on our GitHub repository [dotnet/roslyn-analyzers](https://github.com/dotnet/roslyn-analyzers/issues).

* **Does this work with EditorConfig?**

Not yet. The plan is to use EditorConfig going forward rather than .ruleset files.

* **What is the difference between this extension and the NuGet package? How do I get the NuGet package?**

There is no difference in the rules, just in how they function. An extension applies at the solution-level while a NuGet works at the project-level. Also, if you have a NuGet package installed, rule violations at the Error severity level will break your build. 

You can get the NuGet package for these rules on [NuGet.org](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/).

* **Can I contribute analyzers and fixers to this repo?**

YES! Create an issue with the rule/fix you want to contribute and we’ll help you get started.

* **Why am I not seeing errors when I build on the command-line?**

Extensions cannot affect your build. To have these rules enforced on your command-line or CI builds, please install our equivalent NuGet package on [NuGet.org](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/).

* **I'm seeing too many errors at different severities than I had configured? What's happening?**

You may have both the NuGet package and the Extension for Microsoft Code Analysis installed. Right now, this is a known issue and we have a design to fix this scenario.
