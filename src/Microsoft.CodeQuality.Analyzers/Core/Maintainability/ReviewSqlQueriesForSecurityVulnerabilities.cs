using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ReviewSqlQueriesForSecurityVulnerabilities : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2100";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNoNonLiterals = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesMessageNoNonLiterals), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewSQLQueriesForSecurityVulnerabilitiesDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                                          s_localizableTitle,
                                                                                          s_localizableMessageNoNonLiterals,
                                                                                          DiagnosticCategory.Usage,
                                                                                          DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                          isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                          description: s_localizableDescription,
                                                                                          helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2100-review-sql-queries-for-security-vulnerabilities",
                                                                                          customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider making this analyzer thread-safe.
            //context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationBlockStartAction(operationBlockStartContext =>
            {
                INamedTypeSymbol iDbCommandType = WellKnownTypes.IDbCommand(operationBlockStartContext.Compilation);
                INamedTypeSymbol iDataAdapterType = WellKnownTypes.IDataAdapter(operationBlockStartContext.Compilation);
                IPropertySymbol commandTextProperty = (IPropertySymbol)iDbCommandType.GetMembers("CommandText").Single();

                ISymbol symbol = operationBlockStartContext.OwningSymbol;

                var isDbCommandConstructor = false;
                var isDataAdapterConstructor = false;

                if (symbol.Kind != SymbolKind.Method)
                {
                    return;
                }

                var methodSymbol = (IMethodSymbol)symbol;

                if (methodSymbol.MethodKind == MethodKind.Constructor)
                {
                    isDbCommandConstructor = symbol.ContainingType == iDbCommandType;
                    isDataAdapterConstructor = symbol.ContainingType == iDataAdapterType;
                }

                // Only report diagnostics once per set of operation blocks
                bool diagnosticReported = false;

                // Find all potentially vulnerable parameters for later analysis
                operationBlockStartContext.RegisterOperationAction(operationContext =>
                {
                    if (diagnosticReported)
                    {
                        return;
                    }

                    var creation = (IObjectCreationOperation)operationContext.Operation;
                    if (AnalyzeMethodCall(operationContext, creation.Constructor, symbol, creation.Arguments, creation.Syntax, isDbCommandConstructor, isDataAdapterConstructor))
                    {
                        diagnosticReported = true;
                    }
                }, OperationKind.ObjectCreation);

                // If an object calls a constructor in a base class or the same class, this will get called.
                operationBlockStartContext.RegisterOperationAction(operationContext =>
                {
                    if (diagnosticReported)
                    {
                        return;
                    }

                    var invocation = (IInvocationOperation)operationContext.Operation;

                    // We only analyze constructor invocations
                    if (invocation.TargetMethod.MethodKind != MethodKind.Constructor)
                    {
                        return;
                    }

                    // If we're calling another constructor in the same class from this constructor, assume that all parameters are safe and skip analysis. Parameter usage
                    // will be analyzed there
                    if (invocation.TargetMethod.ContainingType == symbol.ContainingType)
                    {
                        return;
                    }

                    if (AnalyzeMethodCall(operationContext, invocation.TargetMethod, symbol, invocation.Arguments, invocation.Syntax, isDbCommandConstructor, isDataAdapterConstructor))
                    {
                        diagnosticReported = true;
                    }
                }, OperationKind.Invocation);

                operationBlockStartContext.RegisterOperationAction(operationContext =>
                {
                    if (diagnosticReported)
                    {
                        return;
                    }

                    var propertyReference = (IPropertyReferenceOperation)operationContext.Operation;

                    // We're only interested in implementations of IDbCommand.CommandText
                    if (!propertyReference.Property.IsImplementationOfInterfaceMember(commandTextProperty))
                    {
                        return;
                    }

                    // Make sure we're in assignment statement
                    if (!(propertyReference.Parent is IAssignmentOperation assignment))
                    {
                        return;
                    }

                    // Only if the property reference is actually the target of the assignment
                    if (assignment.Target != propertyReference)
                    {
                        return;
                    }

                    if (ReportDiagnosticIfNecessary(operationContext, assignment.Value, assignment.Syntax, propertyReference.Property, symbol))
                    {
                        diagnosticReported = true;
                    }
                }, OperationKind.PropertyReference);
            });
        }

        private bool AnalyzeMethodCall(OperationAnalysisContext operationContext,
                                       IMethodSymbol constructorSymbol,
                                       ISymbol containingSymbol,
                                       ImmutableArray<IArgumentOperation> arguments,
                                       SyntaxNode invocationSyntax,
                                       bool isDbCommandConstructor,
                                       bool isDataAdapterConstructor)
        {
            // All parameters the function takes that are explicit strings are potential vulnerabilities
            var potentials = arguments.WhereAsArray(arg => arg.Parameter.Type.SpecialType == SpecialType.System_String && !arg.Parameter.IsImplicitlyDeclared);
            if (potentials.IsEmpty)
            {
                return false;
            }

            var vulnerableArgumentsBuilder = ImmutableArray.CreateBuilder<IArgumentOperation>();

            foreach (var argument in potentials)
            {
                // For the constructor of a IDbCommand-derived class, if there is only one string parameter, then we just
                // assume that it's the command text. If it takes more than one string, then we need to figure out which
                // one is the command string. However, for the constructor of a IDataAdapter, a lot of times the
                // constructor also take in the connection string, so we can't assume it's the command if there is only one
                // string.
                if (isDataAdapterConstructor || potentials.Length > 1)
                {
                    if (!IsParameterSymbolVulnerable(argument.Parameter))
                    {
                        continue;
                    }
                }

                vulnerableArgumentsBuilder.Add(argument);
            }

            var vulnerableArguments = vulnerableArgumentsBuilder.ToImmutable();

            foreach (var argument in vulnerableArguments)
            {
                if (IsParameterSymbolVulnerable(argument.Parameter) && (isDbCommandConstructor || isDataAdapterConstructor))
                {
                    //No warnings, as Constructor parameters in derived classes are assumed to be safe since this rule will check the constructor arguments at their call sites.
                    return false;
                }
                if (ReportDiagnosticIfNecessary(operationContext, argument.Value, invocationSyntax, constructorSymbol, containingSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsParameterSymbolVulnerable(IParameterSymbol parameter)
        {
            // Parameters might be vulnerable if "cmd" or "command" is in the name
            return parameter != null &&
                   (parameter.Name.IndexOf("cmd", StringComparison.OrdinalIgnoreCase) != -1 ||
                    parameter.Name.IndexOf("command", StringComparison.OrdinalIgnoreCase) != -1);
        }

        private bool ReportDiagnosticIfNecessary(OperationAnalysisContext operationContext,
                                                 IOperation argumentValue,
                                                 SyntaxNode syntax,
                                                 ISymbol invokedSymbol,
                                                 ISymbol containingMethod)
        {
            if (argumentValue.Type.SpecialType != SpecialType.System_String || !argumentValue.ConstantValue.HasValue)
            {
                // Review if the symbol passed to {invocation} in {method/field/constructor/etc} has user input.
                operationContext.ReportDiagnostic(Diagnostic.Create(Rule,
                                                                    syntax.GetLocation(),
                                                                    invokedSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                                    containingMethod.Name));
                return true;
            }
            return false;
        }
    }
}
