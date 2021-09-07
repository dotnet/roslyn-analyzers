// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

using System;
using System.Collections.Immutable;

namespace CA2002
{
    public abstract class DoNotLockOnObjectsWithWeakIdentityAnalyzerBase : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CA2002";

        private static readonly LocalizableString Title = "Do not lock on objects with weak identity";
        private static readonly LocalizableString MessageFormat = "Do not lock on objects with weak identity";
        private static readonly LocalizableString Description = "An object is said to have a weak identity when it can be directly accessed across application domain boundaries. A thread that tries to acquire a lock on an object that has a weak identity can be blocked by a second thread in a different application domain ...";
        private const string Category = "Reliability";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var monitorTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Monitor");
                var marshalByRefObjectTypeSymbol = compilation.GetTypeByMetadataName("System.MarshalByRefObject");
                var executionEngineExceptionTypeSymbol = compilation.GetTypeByMetadataName("System.ExecutionEngineException");
                var outOfMemoryExceptionTypeSymbol = compilation.GetTypeByMetadataName("System.OutOfMemoryException");
                var stackOverflowExceptionTypeSymbol = compilation.GetTypeByMetadataName("System.StackOverflowException");
                var memberInfoTypeSymbol = compilation.GetTypeByMetadataName("System.Reflection.MemberInfo");
                var parameterInfoTypeSymbol = compilation.GetTypeByMetadataName("System.Reflection.ParameterInfo");
                var threadTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Thread");
                new Analyzer(
                    monitorTypeSymbol,
                    marshalByRefObjectTypeSymbol,
                    executionEngineExceptionTypeSymbol,
                    outOfMemoryExceptionTypeSymbol,
                    stackOverflowExceptionTypeSymbol,
                    memberInfoTypeSymbol,
                    parameterInfoTypeSymbol,
                    threadTypeSymbol,
                    IsThisExpression).Run(compilationStartContext);
            });
        }

        protected abstract bool IsThisExpression(SyntaxNode node);

        private class Analyzer
        {
            private readonly INamedTypeSymbol? monitorTypeSymbol;
            private readonly INamedTypeSymbol? marshalByRefObjectTypeSymbol;
            private readonly INamedTypeSymbol? executionEngineExceptionTypeSymbol;
            private readonly INamedTypeSymbol? outOfMemoryExceptionTypeSymbol;
            private readonly INamedTypeSymbol? stackOverflowExceptionTypeSymbol;
            private readonly INamedTypeSymbol? memberInfoTypeSymbol;
            private readonly INamedTypeSymbol? parameterInfoTypeSymbol;
            private readonly INamedTypeSymbol? threadTypeSymbol;
            private readonly Func<SyntaxNode, bool> isThisExpression;

            public Analyzer(
                INamedTypeSymbol? monitorTypeSymbol,
                INamedTypeSymbol? marshalByRefObjectTypeSymbol,
                INamedTypeSymbol? executionEngineExceptionTypeSymbol,
                INamedTypeSymbol? outOfMemoryExceptionTypeSymbol,
                INamedTypeSymbol? stackOverflowExceptionTypeSymbol,
                INamedTypeSymbol? memberInfoTypeSymbol,
                INamedTypeSymbol? parameterInfoTypeSymbol,
                INamedTypeSymbol? threadTypeSymbol,
                Func<SyntaxNode, bool> isThisExpression)
            {
                this.monitorTypeSymbol = monitorTypeSymbol;
                this.marshalByRefObjectTypeSymbol = marshalByRefObjectTypeSymbol;
                this.executionEngineExceptionTypeSymbol = executionEngineExceptionTypeSymbol;
                this.outOfMemoryExceptionTypeSymbol = outOfMemoryExceptionTypeSymbol;
                this.stackOverflowExceptionTypeSymbol = stackOverflowExceptionTypeSymbol;
                this.memberInfoTypeSymbol = memberInfoTypeSymbol;
                this.parameterInfoTypeSymbol = parameterInfoTypeSymbol;
                this.threadTypeSymbol = threadTypeSymbol;
                this.isThisExpression = isThisExpression;
            }

            internal void Run(CompilationStartAnalysisContext compilationStartContext)
            {
                var compilation = compilationStartContext.Compilation;
                compilationStartContext.RegisterOperationAction(
                    context => ReportOnWeakIdentityObject(((ILockStatement)context.Operation).LockedObject, context),
                    OperationKind.LockStatement);

                if (monitorTypeSymbol is INamedTypeSymbol monitorType)
                {
                    compilationStartContext.RegisterOperationAction(context =>
                    {
                        var invocationOperation = (IInvocationExpression)context.Operation;
                        var method = invocationOperation.TargetMethod;

                        if ((method.Name != "Enter" && method.Name != "TryEnter") ||
                            invocationOperation.ArgumentsInSourceOrder.IsEmpty)
                        {
                            return;
                        }

                        if (method.ContainingType.Equals(monitorType) &&
                            invocationOperation.ArgumentsInSourceOrder[0].Value is IConversionExpression conversionOperation)
                        {
                            ReportOnWeakIdentityObject(conversionOperation.Operand, context);
                        }
                    },
                    OperationKind.InvocationExpression);
                }
            }

            private void ReportOnWeakIdentityObject(IOperation operation, OperationAnalysisContext context)
            {
                if (operation is IInstanceReferenceExpression instanceReference && isThisExpression(instanceReference.Syntax))
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(Rule, operation.Syntax.ToString()));
                }
                else if (operation.Type is ITypeSymbol type && TypeHasWeakIdentity(type))
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(Rule, operation.Syntax.ToString()));
                }
            }

            private bool TypeHasWeakIdentity(ITypeSymbol type)
            {
                switch (type.TypeKind)
                {
                    case TypeKind.Array:
                        return type is IArrayTypeSymbol arrayType && IsPrimitiveType(arrayType.ElementType);
                    case TypeKind.Class:
                    case TypeKind.TypeParameter:
                        return
                            type.SpecialType == SpecialType.System_String ||
                            type.Equals(executionEngineExceptionTypeSymbol) ||
                            type.Equals(outOfMemoryExceptionTypeSymbol) ||
                            type.Equals(stackOverflowExceptionTypeSymbol) ||
                            type.Inherits(marshalByRefObjectTypeSymbol) ||
                            type.Inherits(memberInfoTypeSymbol) ||
                            type.Inherits(parameterInfoTypeSymbol) ||
                            type.Inherits(threadTypeSymbol);

                    // What about struct types?
                    default:
                        return false;
                }
            }

            public static bool IsPrimitiveType(ITypeSymbol type)
            {
                return type.SpecialType switch
                {
                    SpecialType.System_Boolean
                    or SpecialType.System_Byte
                    or SpecialType.System_Char
                    or SpecialType.System_Double
                    or SpecialType.System_Int16
                    or SpecialType.System_Int32
                    or SpecialType.System_Int64
                    or SpecialType.System_UInt16
                    or SpecialType.System_UInt32
                    or SpecialType.System_UInt64
                    or SpecialType.System_IntPtr
                    or SpecialType.System_UIntPtr
                    or SpecialType.System_SByte
                    or SpecialType.System_Single => true,
                    _ => false,
                };
            }
        }
    }
}
