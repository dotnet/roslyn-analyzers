namespace ClrHeapAllocationAnalyzer
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExplicitAllocationAnalyzer : AllocationAnalyzer
    {
        public static DiagnosticDescriptor NewArrayRule = new DiagnosticDescriptor("HAA0501", "Explicit new array type allocation", "Explicit new array type allocation", "Performance", DiagnosticSeverity.Info, true);

        public static DiagnosticDescriptor NewObjectRule = new DiagnosticDescriptor("HAA0502", "Explicit new reference type allocation", "Explicit new reference type allocation", "Performance", DiagnosticSeverity.Info, true);

        public static DiagnosticDescriptor AnonymousNewObjectRule = new DiagnosticDescriptor("HAA0503", "Explicit new anonymous object allocation", "Explicit new anonymous object allocation", "Performance", DiagnosticSeverity.Info, true, string.Empty, "http://msdn.microsoft.com/en-us/library/bb397696.aspx");

        public static DiagnosticDescriptor ImplicitArrayCreationRule = new DiagnosticDescriptor("HAA0504", "Implicit new array creation allocation", "Implicit new array creation allocation", "Performance", DiagnosticSeverity.Info, true);

        public static DiagnosticDescriptor InitializerCreationRule = new DiagnosticDescriptor("HAA0505", "Initializer reference type allocation", "Initializer reference type allocation", "Performance", DiagnosticSeverity.Info, true);

        public static DiagnosticDescriptor LetCauseRule = new DiagnosticDescriptor("HAA0506", "Let clause induced allocation", "Let clause induced allocation", "Performance", DiagnosticSeverity.Info, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(LetCauseRule, InitializerCreationRule, ImplicitArrayCreationRule, AnonymousNewObjectRule, NewObjectRule, NewArrayRule);

        protected override SyntaxKind[] Expressions => new[]
        {
            SyntaxKind.ObjectCreationExpression,            // Used
            SyntaxKind.AnonymousObjectCreationExpression,   // Used
            SyntaxKind.ArrayInitializerExpression,          // Used (this is inside an ImplicitArrayCreationExpression)
            SyntaxKind.CollectionInitializerExpression,     // Is this used anywhere?
            SyntaxKind.ComplexElementInitializerExpression, // Is this used anywhere? For what this is see http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis.CSharp/Compilation/CSharpSemanticModel.cs,80
            SyntaxKind.ObjectInitializerExpression,         // Used linked to InitializerExpressionSyntax
            SyntaxKind.ArrayCreationExpression,             // Used
            SyntaxKind.ImplicitArrayCreationExpression,     // Used (this then contains an ArrayInitializerExpression)
            SyntaxKind.LetClause                            // Used
        };

        private static readonly object[] EmptyMessageArgs = { };

        protected override void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var semanticModel = context.SemanticModel;
            Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;
            var cancellationToken = context.CancellationToken;
            string filePath = node.SyntaxTree.FilePath;

            // An InitializerExpressionSyntax has an ObjectCreationExpressionSyntax as it's parent, i.e
            // var testing = new TestClass { Name = "Bob" };
            //               |             |--------------| <- InitializerExpressionSyntax or SyntaxKind.ObjectInitializerExpression
            //               |----------------------------| <- ObjectCreationExpressionSyntax or SyntaxKind.ObjectCreationExpression
            var initializerExpression = node as InitializerExpressionSyntax;
            if (initializerExpression?.Parent is ObjectCreationExpressionSyntax)
            {
                var objectCreation = node.Parent as ObjectCreationExpressionSyntax;
                var typeInfo = semanticModel.GetTypeInfo(objectCreation, cancellationToken);
                if (typeInfo.ConvertedType?.TypeKind != TypeKind.Error &&
                    typeInfo.ConvertedType?.IsReferenceType == true &&
                    objectCreation.Parent?.IsKind(SyntaxKind.EqualsValueClause) == true &&
                    objectCreation.Parent?.Parent?.IsKind(SyntaxKind.VariableDeclarator) == true)
                {
                    reportDiagnostic(Diagnostic.Create(InitializerCreationRule, ((VariableDeclaratorSyntax)objectCreation.Parent.Parent).Identifier.GetLocation(), EmptyMessageArgs));
                    HeapAllocationAnalyzerEventSource.Logger.NewInitializerExpression(filePath);
                    return;
                }
            }

            var implicitArrayExpression = node as ImplicitArrayCreationExpressionSyntax;
            if (implicitArrayExpression != null)
            {
                reportDiagnostic(Diagnostic.Create(ImplicitArrayCreationRule, implicitArrayExpression.NewKeyword.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.NewImplicitArrayCreationExpression(filePath);
                return;
            }

            var newAnon = node as AnonymousObjectCreationExpressionSyntax;
            if (newAnon != null)
            {
                reportDiagnostic(Diagnostic.Create(AnonymousNewObjectRule, newAnon.NewKeyword.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.NewAnonymousObjectCreationExpression(filePath);
                return;
            }

            var newArr = node as ArrayCreationExpressionSyntax;
            if (newArr != null)
            {
                reportDiagnostic(Diagnostic.Create(NewArrayRule, newArr.NewKeyword.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.NewArrayExpression(filePath);
                return;
            }

            var newObj = node as ObjectCreationExpressionSyntax;
            if (newObj != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(newObj, cancellationToken);
                if (typeInfo.ConvertedType != null && typeInfo.ConvertedType.TypeKind != TypeKind.Error && typeInfo.ConvertedType.IsReferenceType)
                {
                    reportDiagnostic(Diagnostic.Create(NewObjectRule, newObj.NewKeyword.GetLocation(), EmptyMessageArgs));
                    HeapAllocationAnalyzerEventSource.Logger.NewObjectCreationExpression(filePath);
                }
                return;
            }

            var letKind = node as LetClauseSyntax;
            if (letKind != null)
            {
                reportDiagnostic(Diagnostic.Create(LetCauseRule, letKind.LetKeyword.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.LetClauseExpression(filePath);
                return;
            }
        }
    }
}