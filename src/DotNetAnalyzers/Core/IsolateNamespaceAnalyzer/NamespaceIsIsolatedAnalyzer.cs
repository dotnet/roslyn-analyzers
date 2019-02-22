// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using DiagnosticIds = Roslyn.Diagnostics.Analyzers.RoslynDiagnosticIds;

namespace DotNetAnalyzers.IsolateNamespaceAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamespaceIsIsolatedAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor TypeIsInIsolatedNamespaceRule = new DiagnosticDescriptor(
            id: DiagnosticIds.TypeIsInIsolatedNamespaceRuleId,
            title: DotNetAnalyzersResources.TypeIsInIsolatedNamespaceTitle,
            messageFormat: DotNetAnalyzersResources.TypeIsInIsolatedNamespaceMessage,
            category: "ApiDesign",
            defaultSeverity: DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: DotNetAnalyzersResources.TypeIsInIsolatedNamespaceDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(TypeIsInIsolatedNamespaceRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();

            // Analyzer needs to get callbacks for generated code, and might report diagnostics in generated code.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            analysisContext.RegisterCompilationStartAction(
                compilationStartAnalysisContext =>
                {
                    var predicate = BuildIsolationPredicate(compilationStartAnalysisContext.Compilation);

                    // If no isolation attribute exist on the assembly, there's nothing to do
                    if (predicate == null)
                        return;

                    analysisContext.RegisterSyntaxNodeAction(
                        context =>
                        {
                            switch (context.Node)
                            {
                                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                                    foreach (var syntax in localDeclarationStatementSyntax.Declaration.Variables)
                                    {
                                        var localDeclarationTypeSymbol = (ILocalSymbol)context.SemanticModel.GetDeclaredSymbol(syntax);
                                        Verify(localDeclarationTypeSymbol.Type, localDeclarationStatementSyntax.Declaration.Type);
                                    }
                                    break;

                                case FieldDeclarationSyntax fieldDeclarationSyntax:
                                    foreach (var syntax in fieldDeclarationSyntax.Declaration.Variables)
                                    {
                                        var fieldSymbol = (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(syntax);
                                        Verify(fieldSymbol.Type, fieldDeclarationSyntax.Declaration.Type);
                                    }
                                    break;

                                case EventFieldDeclarationSyntax eventFieldDeclarationSyntax:
                                    foreach (var syntax in eventFieldDeclarationSyntax.Declaration.Variables)
                                    {
                                        var eventFieldSymbol = (IEventSymbol)context.SemanticModel.GetDeclaredSymbol(syntax);
                                        Verify(eventFieldSymbol.Type, eventFieldDeclarationSyntax.Declaration.Type);
                                    }
                                    break;

                                case PropertyDeclarationSyntax propertyDeclarationSyntax:
                                    var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);
                                    Verify(propertySymbol.Type, propertyDeclarationSyntax.Type);
                                    break;

                                case EventDeclarationSyntax eventDeclarationSyntax:
                                    var eventSymbol = context.SemanticModel.GetDeclaredSymbol(eventDeclarationSyntax);
                                    Verify(eventSymbol.Type, eventDeclarationSyntax.Type);
                                    break;

                                case ParameterSyntax parameterSyntax:
                                    var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameterSyntax);
                                    Verify(parameterSymbol.Type, parameterSyntax.Type);
                                    break;

                                case StructDeclarationSyntax structDeclarationSyntax:
                                    VerifyConstraintClauses(structDeclarationSyntax.ConstraintClauses);
                                    break;

                                case ClassDeclarationSyntax classDeclarationSyntax:
                                    if (classDeclarationSyntax.BaseList != null)
                                    {
                                        foreach (var syntax in classDeclarationSyntax.BaseList.Types)
                                        {
                                            var baseTypeSymbol = (ITypeSymbol)context.SemanticModel.GetSymbolInfo(syntax.Type).Symbol;
                                            if (baseTypeSymbol != null)
                                                Verify(baseTypeSymbol, syntax.Type);
                                        }
                                    }

                                    VerifyConstraintClauses(classDeclarationSyntax.ConstraintClauses);
                                    break;

                                case TypeOfExpressionSyntax typeOfExpressionSyntax:
                                    var typeSymbol = (ITypeSymbol)context.SemanticModel.GetSymbolInfo(typeOfExpressionSyntax.Type).Symbol;
                                    Verify(typeSymbol, typeOfExpressionSyntax.Type);
                                    break;
                            }

                            void Verify(ITypeSymbol targetSymbol, SyntaxNode syntax) => VerifyIsolation(context.ReportDiagnostic, targetSymbol, context.ContainingSymbol, syntax);

                            void VerifyConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
                            {
                                if (constraintClauses.Count != 0)
                                {
                                    foreach (var constraintClauseSyntax in constraintClauses)
                                    {
                                        foreach (var typeParameterConstraintSyntax in constraintClauseSyntax.Constraints)
                                        {
                                            if (typeParameterConstraintSyntax is TypeConstraintSyntax typeConstraintSyntax)
                                            {
                                                var typeSymbol = (ITypeSymbol)context.SemanticModel.GetSymbolInfo(typeConstraintSyntax.Type).Symbol;
                                                if (typeSymbol != null)
                                                {
                                                    Verify(typeSymbol, typeConstraintSyntax.Type);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        SyntaxKind.LocalDeclarationStatement,
                        SyntaxKind.PropertyDeclaration,
                        SyntaxKind.FieldDeclaration,
                        SyntaxKind.EventFieldDeclaration,
                        SyntaxKind.EventDeclaration,
                        SyntaxKind.Parameter,
                        SyntaxKind.TypeOfExpression,
                        SyntaxKind.StructDeclaration,
                        SyntaxKind.ClassDeclaration);

                    compilationStartAnalysisContext.RegisterOperationAction(
                        context =>
                        {
                            switch (context.Operation)
                            {
                                case IArrayCreationOperation arrayCreation:
                                    if (arrayCreation.Syntax is ArrayCreationExpressionSyntax arrayCreationExpression)
                                        VerifyOperation(arrayCreation.Type, arrayCreationExpression.Type);
                                    break;

                                case IObjectCreationOperation objectCreation:
                                    if (objectCreation.Syntax is ObjectCreationExpressionSyntax objectCreationExpression)
                                        VerifyOperation(objectCreation.Type, objectCreationExpression.Type);
                                    break;

                                case IMethodReferenceOperation methodReference:
                                    if (methodReference.Method.IsStatic)
                                        VerifyMethod(methodReference.Syntax, methodReference.Method, UnwrapMethodReference(methodReference.Syntax));
                                    break;

                                case IMemberReferenceOperation memberReference:
                                    if (memberReference.Member.IsStatic)
                                        VerifyOperation(memberReference.Member.ContainingType, GetOwningMemberSyntax(memberReference.Syntax));
                                    break;

                                case IInvocationOperation invocation:
                                    if (invocation.TargetMethod.IsStatic)
                                    {
                                        var syntax = invocation.Syntax is InvocationExpressionSyntax ies ? ies.Expression : invocation.Syntax;
                                        VerifyMethod(invocation.Syntax, invocation.TargetMethod, GetOwningMemberSyntax(syntax));
                                    }
                                    break;
                            }

                            void VerifyOperation(ITypeSymbol targetSymbol, SyntaxNode syntax) => VerifyIsolation(context.ReportDiagnostic, targetSymbol, context.ContainingSymbol, syntax);

                            void VerifyMethod(SyntaxNode syntax, IMethodSymbol method, SyntaxNode diagnosticSyntax)
                            {
                                UnwrapMethodNameSyntax();

                                if (syntax is GenericNameSyntax genericNameSyntax &&
                                    genericNameSyntax.TypeArgumentList.Arguments.Count == method.TypeArguments.Length)
                                {
                                    for (var i = 0; i < method.TypeArguments.Length; i++)
                                    {
                                        var typeArgument = method.TypeArguments[i];
                                        var typeArgumentSyntax = genericNameSyntax.TypeArgumentList.Arguments[i];
                                        VerifyOperation(typeArgument, typeArgumentSyntax);
                                    }
                                }

                                VerifyOperation(method.ContainingType, diagnosticSyntax);

                                void UnwrapMethodNameSyntax()
                                {
                                    // Loop to avoid recursion
                                    while (true)
                                    {
                                        switch (syntax)
                                        {
                                            case InvocationExpressionSyntax invocationExpressionSyntax:
                                                syntax = invocationExpressionSyntax.Expression;
                                                continue;
                                            case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                                                syntax = memberAccessExpressionSyntax.Name;
                                                continue;
                                            case MemberBindingExpressionSyntax memberAccessExpressionSyntax:
                                                syntax = memberAccessExpressionSyntax.Name;
                                                continue;
                                            default:
                                                return;
                                        }
                                    }
                                }
                            }
                        },
                        OperationKind.ArrayCreation,
                        OperationKind.ObjectCreation,
                        OperationKind.FieldReference,
                        OperationKind.PropertyReference,
                        OperationKind.EventReference,
                        OperationKind.MethodReference,
                        OperationKind.Invocation);

                    SyntaxNode UnwrapMethodReference(SyntaxNode syntax)
                    {
                        // Loop to avoid recursion
                        while (true)
                        {
                            switch (syntax)
                            {
                                case MemberAccessExpressionSyntax memberAccessExpressionSyntax when memberAccessExpressionSyntax.Expression is MemberAccessExpressionSyntax:
                                    syntax = memberAccessExpressionSyntax.Expression;
                                    continue;
                                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                                    return memberAccessExpressionSyntax.Name;
                                default:
                                    return syntax;
                            }
                        }
                    }

                    SyntaxNode GetOwningMemberSyntax(SyntaxNode syntax)
                    {
                        if (syntax is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                        {
                            switch (memberAccessExpressionSyntax.Expression)
                            {
                                case IdentifierNameSyntax identifierNameSyntax:
                                    return identifierNameSyntax;
                                case MemberAccessExpressionSyntax expressionMemberAccessExpressionSyntax:
                                    return expressionMemberAccessExpressionSyntax.Name;
                            }
                        }

                        return syntax;
                    }

                    void VerifyIsolation(Action<Diagnostic> reportDiagnostic, ITypeSymbol targetSymbol, ISymbol containingSymbol, SyntaxNode syntax)
                    {
                        syntax = UnwrapQualifiedName(syntax);

                        if (targetSymbol is IArrayTypeSymbol arrayTypeSymbol)
                        {
                            syntax = syntax is ArrayTypeSyntax arrayTypeSyntax ? arrayTypeSyntax.ElementType : syntax;
                            VerifyIsolation(reportDiagnostic, arrayTypeSymbol.ElementType, containingSymbol, syntax);
                        }
                        else
                        {
                            VerifyTypeIsolation(targetSymbol);

                            if (targetSymbol is INamedTypeSymbol namedTypeSymbol &&
                                syntax is GenericNameSyntax genericNameSyntax &&
                                namedTypeSymbol.TypeArguments.Length != 0 &&
                                namedTypeSymbol.TypeArguments.Length == genericNameSyntax.Arity)
                            {
                                for (var i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
                                {
                                    var typeArgument = namedTypeSymbol.TypeArguments[i];
                                    var typeArgumentSyntax = genericNameSyntax.TypeArgumentList.Arguments[i];
                                    VerifyIsolation(reportDiagnostic, typeArgument, containingSymbol, UnwrapQualifiedName(typeArgumentSyntax));
                                }
                            }
                        }

                        void VerifyTypeIsolation(ITypeSymbol typeSymbol)
                        {
                            var isIsolationViolation = !predicate(typeSymbol.ContainingNamespace, containingSymbol.ContainingNamespace);

                            if (isIsolationViolation)
                            {
                                // Type '{0}' is isolated and may not be used from namespace '{1}'.
                                reportDiagnostic(
                                    Diagnostic.Create(
                                        TypeIsInIsolatedNamespaceRule,
                                        syntax.GetLocation(),
                                        typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                        containingSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                            }
                        }

                        SyntaxNode UnwrapQualifiedName(SyntaxNode syntaxNode)
                        {
                            return syntaxNode is QualifiedNameSyntax qualifiedName ? qualifiedName.Right : syntaxNode;
                        }
                    }

                    IsolationPredicate BuildIsolationPredicate(Compilation compilation)
                    {
                        var assemblyAttributes = compilation.Assembly.GetAttributes();

                        var isolateNamespaceAttributes = assemblyAttributes
                            .Where(attr => attr.AttributeClass.Name == nameof(IsolateNamespaceAttribute) &&
                                           attr.AttributeConstructor.Parameters.Length == 1 &&
                                           attr.AttributeConstructor.Parameters[0].Type.SpecialType == SpecialType.System_String);

                        var isolateNamespaceGroupAttributes = assemblyAttributes
                            .Where(attr => attr.AttributeClass.Name == nameof(IsolateNamespaceGroupAttribute) &&
                                           attr.AttributeConstructor.Parameters.Length == 1 &&
                                           attr.AttributeConstructor.Parameters[0].Type is IArrayTypeSymbol arrayTypeSymbol &&
                                           arrayTypeSymbol.ElementType.SpecialType == SpecialType.System_String)
                            .Select(attr => attr.ConstructorArguments[0].Values.Select(v => (string)v.Value).ToArray());

                        // The set of namespaces participating in isolation.
                        var isolatedNamespaces = new HashSet<string>(StringComparer.Ordinal);

                        // Isolated namespaces that are visible to other namespaces are modeled here,
                        // whether due to group membership or IsolateNamespaceAttribute.AllowFrom.
                        var friendPairs = new HashSet<(string target, string from)>();

                        foreach (var attribute in isolateNamespaceAttributes)
                        {
                            var ns = (string)attribute.ConstructorArguments[0].Value;
                            isolatedNamespaces.Add(ns);
                            var allowFrom = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == nameof(IsolateNamespaceAttribute.AllowFrom)).Value;
                            if (allowFrom.Kind == TypedConstantKind.Array &&
                                allowFrom.Type is IArrayTypeSymbol arrayTypeSymbol &&
                                arrayTypeSymbol.ElementType.SpecialType == SpecialType.System_String)
                            {
                                foreach (var value in allowFrom.Values)
                                {
                                    var ns2 = (string)value.Value;
                                    friendPairs.Add((ns, ns2));
                                }
                            }
                        }

                        foreach (var groupValues in isolateNamespaceGroupAttributes)
                        {
                            for (int i = 0; i < groupValues.Length; i++)
                            {
                                var ns1 = groupValues[i];
                                isolatedNamespaces.Add(ns1);

                                for (int j = 0; j < groupValues.Length; j++)
                                {
                                    if (i == j)
                                        continue;

                                    var ns2 = groupValues[j];
                                    friendPairs.Add((ns1, ns2));
                                }
                            }
                        }

                        // If no matching attributes were declared on the assembly, return null which will
                        // short circuit further analysis.
                        if (isolatedNamespaces.Count == 0 && friendPairs.Count == 0)
                            return null;

                        // A cache of namespace strings by symbol.
                        var dic = ImmutableDictionary<INamespaceSymbol, string>.Empty;

                        // Return a predicate to use when evaluating references between types.
                        return (targetNamespace, fromNamespace) =>
                        {
                            // Null namespaces can happen. Just bail.
                            if (targetNamespace == null || fromNamespace == null)
                                return true;

                            // If the namespaces match, it's fine.
                            if (targetNamespace.Equals(fromNamespace))
                                return true;

                            var target = ImmutableInterlocked.GetOrAdd(ref dic, targetNamespace, ns => ns.ToString());

                            // If the target is subject to isolation...
                            if (isolatedNamespaces.Contains(target))
                            {
                                // ...ensure the relationship is allowed
                                var from = ImmutableInterlocked.GetOrAdd(ref dic, fromNamespace, ns => ns.ToString());

                                return friendPairs.Contains((target, from));
                            }

                            // The target is not explicitly modeled, so it's fine.
                            return true;
                        };
                    }
                });
        }

        /// <summary>
        /// Predicate for testing isolation violations between namespaces.
        /// </summary>
        /// <param name="targetNamespace">The namespace being referenced.</param>
        /// <param name="fromNamespace">The namespace the reference is within.</param>
        /// <returns><c>true</c> if the reference is allowed, otherwise <c>false</c> for an isolation violation.</returns>
        private delegate bool IsolationPredicate(INamespaceSymbol targetNamespace, INamespaceSymbol fromNamespace);
    }
}
