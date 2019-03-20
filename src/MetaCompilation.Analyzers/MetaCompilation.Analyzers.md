### MetaAnalyzer001: Missing diagnostic id ###

The diagnostic id identifies a particular diagnostic so that the diagnotic can be fixed in CodeFixProvider.cs

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer002: Missing Initialize method ###

An analyzer requires the Initialize method to register code analysis actions. Actions are registered to call an analysis method when something specific changes in the syntax tree or semantic model. For example, context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.IfStatement) will call AnalyzeMethod every time an if-statement changes in the syntax tree.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer003: Missing register statement ###

The Initialize method must register for at least one action so that some analysis can be performed. Otherwise, analysis will not be performed and no diagnostics will be reported. Registering a syntax node action is useful for analyzing the syntax of a piece of code.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer004: Multiple registered actions ###

For this tutorial only, the Initialize method should only register one action. More complicated analyzers may need to register multiple actions.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer005: Incorrect method signature ###

The Initialize method should override the abstract Initialize class member from the DiagnosticAnalyzer class. It therefore needs to be public, overriden, and return void. It needs to have a single input parameter of type 'AnalysisContext.'

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer006: Incorrect statement ###

By definition, the purpose of the Initialize method is to register actions for analysis. Therefore, all other statements placed in Initialize are incorrect.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer007: Missing SupportedDiagnostics property ###

The SupportedDiagnostics property tells the analyzer which diagnostic ids the analyzer supports, in other words, which DiagnosticDescriptors might be reported by the analyzer. Generally, any DiagnosticDescriptor should be returned by SupportedDiagnostics.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer008: Incorrect SupportedDiagnostics property ###

T: The overriden SupportedDiagnostics property should return an Immutable Array of Diagnostic Descriptors

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer009: Missing get-accessor ###

The SupportedDiagnostics property needs to have a get-accessor to make the ImmutableArray of DiagnosticDescriptors accessible

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer010: Too many accessors ###

The purpose of the SupportedDiagnostics property is to return a list of all diagnostics that can be reported by a particular analyzer, so it doesn't have a need for any other accessors

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer011: Get accessor missing return value ###

The purpose of the SupportedDiagnostics property's get-accessor is to return a list of all diagnostics that can be reported by a particular analyzer

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer012: SupportedDiagnostics return value incorrect ###

The purpose of the SupportedDiagnostics property's get-accessor is to return a list of all diagnostics that can be reported by a particular analyzer

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer013: ImmutableArray incorrect ###

The purpose of the SupportedDiagnostics property is to return a list of all diagnostics that can be reported by a particular analyzer, so it should include every DiagnosticDescriptor rule that is created

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer014: Incorrect DiagnosticDescriptor id ###

The id parameter of a DiagnosticDescriptor should be a string constant previously declared. This ensures that the diagnostic id is accessible from the CodeFixProvider.cs file.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer015: Missing Diagnostic id declaration ###

The id parameter of a DiagnosticDescriptor should be a string constant previously declared. This ensures that the diagnostic id is accessible from the CodeFixProvider.cs file.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer016: Incorrect defaultSeverity ###

There are four option for the severity of the diagnostic: error, warning, hidden, and info. An error is completely not allowed and causes build errors. A warning is something that might be a problem, but is not a build error. An info diagnostic is simply information and is not actually a problem. A hidden diagnostic is raised as an issue, but it is not accessible through normal means. In simple analyzers, the most common severities are error and warning.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer017: Incorrect isEnabledByDefault ###

The 'isEnabledByDefault' field determines whether the diagnostic is enabled by default or the user of the analyzer has to manually enable the diagnostic. Generally, it will be set to true.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer018: Incorrect DiagnosticDescriptor modifiers ###

The DiagnosticDescriptor rules should all be internal because they only need to be accessed by pieces of this project and nothing outside. They should be static because their lifetime will extend throughout the entire running of this program

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer019: Missing DiagnosticDescriptor ###

The DiagnosticDescriptor rule is what is reported by the analyzer when it finds a problem, so there must be at least one. It should have an id to differentiate the diagnostic, a default severity level, a boolean describing if it's enabled by default, a title, a message, and a category.

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer020: Missing if-statement extraction ###

The context parameter has a Node member. This Node is what the register statement from Initialize triggered on, and so should be cast to the expected syntax or symbol type

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer021: If-statement extraction incorrect ###

The context parameter has a Node member. This Node is what the register statement from Initialize triggered on, so it should be cast to the expected syntax or symbol type

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer022: Missing if-keyword extraction ###

In the syntax tree, a node of type IfStatementSyntax has an IfKeyword attached to it. On the syntax tree diagram, this is represented by the green 'if' SyntaxToken

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer023: Incorrect if-keyword extraction ###

In the syntax tree, a node of type IfStatementSyntax has an IfKeyword attached to it. On the syntax tree diagram, this is represented by the green 'if' SyntaxToken

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer024: Missing trailing trivia check ###

Syntax trivia are all the things that aren't actually code (i.e. comments, whitespace, end of line tokens, etc). The first step in checking for a single space between the if-keyword and '(' is to check if the if-keyword SyntaxToken has any trailing trivia

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer025: Incorrect trailing trivia check ###

Syntax trivia are all the things that aren't actually code (i.e. comments, whitespace, end of line tokens, etc). The first step in checking for a single space between the if-keyword and '(' is to check if the if-keyword SyntaxToken has any trailing trivia

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer026: Missing trailing trivia extraction ###

The first trailing trivia of the if-keyword should be a single whitespace

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer027: Incorrect trailing trivia extraction ###

The first trailing trivia of the if-keyword should be a single whitespace

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer028: Missing SyntaxKind check ###

T: Next, check if the kind of '{0}' is whitespace trivia

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer029: Incorrect SyntaxKind check ###

T: This statement should check to see if the kind of '{0}' is whitespace trivia

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer030: Missing whitespace check ###

T: Next, check if '{0}' is a single whitespace, which is the desired formatting

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer031: Incorrect whitespace check ###

T: This statement should check to see if '{0}' is a single whitespace, which is the desired formatting

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer032: Missing return ###

If the analyzer determines that there are no issues with the code it is analyzing, it can simply return from the analysis method without reporting any diagnostics

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer033: Incorrect return ###

If the analyzer determines that there are no issues with the code it is analyzing, it can simply return from the analysis method without reporting any diagnostics

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer034: Missing open parenthesis variable ###

The open parenthesis of the condition is going to be the end point of the diagnostic squiggle that is created

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer035: Open parenthesis variable incorrect ###

The open parenthesis of the condition is going to be the end point of the diagnostic squiggle that is created

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer036: Start span variable missing ###

Each node in the syntax tree has a span. This span represents the number of character spaces that the node takes up

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer037: Start span variable incorrect ###

Each node in the syntax tree has a span. This span represents the number of character spaces that the node takes up

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer038: End span variable missing ###

The open parenthesis of the condition is going to be the end point of the diagnostic squiggle that is created

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer039: End span variable incorrect ###

Each node in the syntax tree has a span. This span represents the number of character spaces that the node takes up

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer040: Diagnostic span variable missing ###

Each node in the syntax tree has a span. This span represents the number of character spaces that the node takes up

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer041: Diagnostic span variable incorrect ###

Each node in the syntax tree has a span. This span represents the number of character spaces that the node takes up. TextSpan.FromBounds(start, end) can be used to create a span to use for a diagnostic

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer042: Diagnostic location variable missing ###

A location can be created by combining a span with a syntax tree. The span is applied to the given syntax tree so that the location within the syntax tree is determined

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer043: Diagnostic location variable incorrect ###

A location can be created by combining a span with a syntax tree. The span is applied to the given syntax tree so that the location within the syntax tree is determined

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer044: Missing analysis method ###

In Initialize, the register statement denotes an analysis method to be called when an action is triggered. This method needs to be created

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer045: Too many statements ###

For the purpose of this tutorial there are too many statements here, use the code fixes to guide you through the creation of this section

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer046: Diagnostic variable missing ###

This is the diagnostic that will be reported to the user as an error squiggle

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer047: Diagnostic variable incorrect ###

The diagnostic is created with a DiagnosticDescriptor, a Location, and message arguments. The message arguments are the inputs to the DiagnosticDescriptor MessageFormat format string

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer048: Diagnostic report missing ###

A diagnostic is reported to a context so that the diagnostic will appear as a squiggle and in the eroor list

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer049: Diagnostic report incorrect ###

A diagnostic is reported to a context so that the diagnostic will appear as a squiggle and in the eroor list

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer050: Analyzer tutorial complete ###

T: Congratulations! You have written an analyzer! If you would like to explore a code fix for your diagnostic, open up CodeFixProvider.cs and take a look! To see your analyzer in action, press F5. A new instance of Visual Studio will open up, in which you can open a new C# console app and write test if-statements.

Category: Tutorial

Severity: Info

IsEnabledByDefault: True

### MetaAnalyzer051: Incorrect kind ###

For the purposes of this tutorial, the only analysis will occur on an if-statement, so it is only necessary to register for syntax of kind IfStatement

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer052: Incorrect register ###

For the purposes of this tutorial, analysis will occur on SyntaxNodes, so only actions on SyntaxNodes should be registered

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer053: Incorrect arguments ###

The RegisterSyntaxNodeAction method takes two arguments. The first argument is a method that will be called to perform the analysis. The second argument is a SyntaxKind, which is the kind of syntax that the method will be triggered on

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer054: Incorrect analysis method accessibility ###

T: The '{0}' method should be private

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer055: Incorrect analysis method return type ###

T: The '{0}' method should have a void return type

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer056: Incorrect parameter to analysis method ###

T: The '{0}' method should take one parameter of type SyntaxNodeAnalysisContext

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer057: Trailing trivia count missing ###

T: Next, check that '{0}' only has one trailing trivia element

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer058: Trailing trivia count incorrect ###

T: This statement should check that '{0}' only has one trailing trivia element

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer059: ID string literal ###

T: The ID should not be a string literal, because the ID must be accessible from the code fix provider

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer060: Change default title ###

T: Please change the title to a string of your choosing

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer061: Change default message ###

T: Please change the default message to a string of your choosing

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

### MetaAnalyzer062: Change default category ###

T: Please change the category to a string of your choosing

Category: Tutorial

Severity: Error

IsEnabledByDefault: True

