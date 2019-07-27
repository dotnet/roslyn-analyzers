// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers;

namespace Microsoft.NetCore.Analyzers.Performance
{
#pragma warning disable CA1200 // Avoid using cref tags with a prefix
    /// <summary>
    /// CA1828: Do not use CountAsync() when AnyAsync() can be used.
    /// <para>
    /// <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource})"/> 
    /// and <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource}, bool})"/>
    /// enumerates the entire enumerable
    /// while <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource})"/>
    /// and <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource}, bool})"/>
    /// will only enumerates, at most, up until the first item.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Use cases covered:
    /// <list type="table">
    /// <listheader><term>detected</term><term>fix</term></listheader>
    /// <item><term><c> await queryable.CountAsync() == 0               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> await queryable.CountAsync() != 0               </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> await queryable.CountAsync() &lt;= 0            </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> await queryable.CountAsync() > 0                </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> await queryable.CountAsync() &lt; 1             </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> await queryable.CountAsync() >= 1               </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> 0 == await queryable.CountAsync()               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> 0 != await queryable.CountAsync()               </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> 0 >= await queryable.CountAsync()               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> 0 &lt; await queryable.CountAsync()             </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> 1 > await queryable.CountAsync()                </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> 1 &lt;= await queryable.CountAsync()            </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    /// <item><term><c> (await queryable.CountAsync()).Equals(0)          </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> 0.Equals(await queryable.CountAsync())          </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) == 0      </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) != 0      </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) &lt;= 0   </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) > 0       </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) &lt; 1    </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true) >= 1      </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> 0 == await queryable.CountAsync(_ => true)      </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> 0 != await queryable.CountAsync(_ => true)      </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> 0 &lt; await queryable.CountAsync(_ => true)    </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> 0 >= await queryable.CountAsync(_ => true)      </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> 1 > await queryable.CountAsync(_ => true)       </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> 1 &lt;= await queryable.CountAsync(_ => true)   </c></term><description><c> await queryable.AnyAsync(_ => true)  </c></description></item>
    /// <item><term><c> await queryable.CountAsync(_ => true).Equals(0) </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// <item><term><c> 0.Equals(await queryable.CountAsync(_ => true)) </c></term><description><c> !await queryable.AnyAsync(_ => true) </c></description></item>
    /// </list>
    /// </remarks>
#pragma warning restore CA1200 // Avoid using cref tags with a prefix
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseCountAsyncWhenAnyAsyncCanBeUsedAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1828";
        private const string CountAsyncMethodName = "CountAsync";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleId,
            Title,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
            description: s_localizableDescription,
#pragma warning disable CA1308 // Normalize strings to uppercase
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/" + RuleId.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        /// <value>The supported diagnostics.</value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s_rule);

        /// <summary>
        /// Called once at session start to register actions in the analysis context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        /// <summary>
        /// Called on compilation start.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions") is INamedTypeSymbol entityFrameworkQueryableExtensionsType)
            {
                context.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeInvocationExpression((IInvocationOperation)operationAnalysisContext.Operation, entityFrameworkQueryableExtensionsType, CountAsyncMethodName, operationAnalysisContext.ReportDiagnostic),
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeBinaryExpression((IBinaryOperation)operationAnalysisContext.Operation, entityFrameworkQueryableExtensionsType, CountAsyncMethodName, operationAnalysisContext.ReportDiagnostic),
                    OperationKind.BinaryOperator);
            }

            if (context.Compilation.GetTypeByMetadataName("System.Data.Entity.QueryableExtensions") is INamedTypeSymbol queryableExtensionsType)
            {
                context.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeInvocationExpression((IInvocationOperation)operationAnalysisContext.Operation, queryableExtensionsType, CountAsyncMethodName, operationAnalysisContext.ReportDiagnostic),
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeBinaryExpression((IBinaryOperation)operationAnalysisContext.Operation, queryableExtensionsType, CountAsyncMethodName, operationAnalysisContext.ReportDiagnostic),
                    OperationKind.BinaryOperator);
            }
        }

        /// <summary>
        /// Check to see if we have an expression comparing the result of the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// using <see cref="int.Equals(int)" />.
        /// </summary>
        /// <param name="invocationOperation">The invocation operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        private static void AnalyzeInvocationExpression(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol, string methodName, Action<Diagnostic> reportDiagnostic)
        {
            if (invocationOperation.Arguments.Length == 1)
            {
                var methodSymbol = invocationOperation.TargetMethod;
                if (IsInt32EqualsMethod(methodSymbol) &&
                    (IsCountEqualsZero(invocationOperation, containingSymbol, methodName) || IsZeroEqualsCount(invocationOperation, containingSymbol, methodName)))
                {
                    reportDiagnostic(invocationOperation.Syntax.CreateDiagnostic(s_rule));
                }
            }
        }

        /// <summary>
        /// Checks if the given method is the <see cref="int.Equals(int)"/> method.
        /// </summary>
        /// <param name="methodSymbol">The method symbol.</param>
        /// <returns><see langword="true"/> if the given method is the <see cref="int.Equals(int)"/> method; otherwise, <see langword="false"/>.</returns>
        private static bool IsInt32EqualsMethod(IMethodSymbol methodSymbol)
        {
            return string.Equals(methodSymbol.Name, WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal) &&
                   methodSymbol.ContainingType.SpecialType == SpecialType.System_Int32;
        }

        /// <summary>
        /// Checks whether the value of the invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>
        /// is being compared with 0 using <see cref="int.Equals(int)"/>.
        /// </summary>
        /// <param name="invocationOperation">The invocation operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns><see langword="true" /> if the value of the invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>
        /// is being compared with 0 using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
        private static bool IsCountEqualsZero(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol, string methodName)
        {
            if (!TryGetInt32Constant(invocationOperation.Arguments[0].Value, out var constant) || constant != 0)
            {
                return false;
            }

            return IsCountAsyncMethodInvocationAwaited(invocationOperation.Instance, containingSymbol, methodName);
        }

        /// <summary>
        /// Checks whether 0 is being compared with the value of the invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>
        /// using <see cref="int.Equals(int)"/>.
        /// </summary>
        /// <param name="invocationOperation">The invocation operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns><see langword="true" /> if 0 is being compared with the value of the invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>
        /// using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
        private static bool IsZeroEqualsCount(IInvocationOperation invocationOperation, INamedTypeSymbol containingSymbol, string methodName)
        {
            if (!TryGetInt32Constant(invocationOperation.Instance, out var constant) || constant != 0)
            {
                return false;
            }

            return IsCountAsyncMethodInvocationAwaited(invocationOperation.Arguments[0].Value, containingSymbol, methodName);
        }

        /// <summary>
        /// Check to see if we have an expression comparing the result of
        /// the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// using operators.
        /// </summary>
        /// <param name="binaryOperation">The binary operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="reportDiagnostic">The report diagnostic action.</param>
        private static void AnalyzeBinaryExpression(IBinaryOperation binaryOperation, INamedTypeSymbol containingSymbol, string methodName, Action<Diagnostic> reportDiagnostic)
        {
            if (binaryOperation.IsComparisonOperator() &&
                (IsLeftCountComparison(binaryOperation, containingSymbol, methodName) || IsRightCountComparison(binaryOperation, containingSymbol, methodName)))
            {
                reportDiagnostic(binaryOperation.Syntax.CreateDiagnostic(s_rule));
            }
        }

        /// <summary>
        /// Checks whether the value of the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// is being compared with 0 or 1 using <see cref="int" /> comparison operators.
        /// </summary>
        /// <param name="binaryOperation">The binary operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns><see langword="true" /> if the value of the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
        private static bool IsLeftCountComparison(IBinaryOperation binaryOperation, INamedTypeSymbol containingSymbol, string methodName)
        {
            if (!TryGetInt32Constant(binaryOperation.RightOperand, out var constant))
            {
                return false;
            }

            if (constant == 0 &&
                binaryOperation.OperatorKind != BinaryOperatorKind.Equals &&
                binaryOperation.OperatorKind != BinaryOperatorKind.NotEquals &&
                binaryOperation.OperatorKind != BinaryOperatorKind.LessThanOrEqual &&
                binaryOperation.OperatorKind != BinaryOperatorKind.GreaterThan)
            {
                return false;
            }
            else if (constant == 1 &&
                binaryOperation.OperatorKind != BinaryOperatorKind.LessThan &&
                binaryOperation.OperatorKind != BinaryOperatorKind.GreaterThanOrEqual)
            {
                return false;
            }
            else if (constant > 1)
            {
                return false;
            }

            return IsCountAsyncMethodInvocationAwaited(binaryOperation.LeftOperand, containingSymbol, methodName);
        }

        /// <summary>
        /// Checks whether 0 or 1 is being compared with the value of the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// using <see cref="int" /> comparison operators.
        /// </summary>
        /// <param name="binaryOperation">The binary operation.</param>
        /// <param name="containingSymbol">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if 0 or 1 is being compared with the value of the invocation of the method <paramref name="methodName" /> in the <paramref name="containingSymbol" />
        /// using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
        private static bool IsRightCountComparison(IBinaryOperation binaryOperation, INamedTypeSymbol containingSymbol, string methodName)
        {
            if (!TryGetInt32Constant(binaryOperation.LeftOperand, out var constant))
            {
                return false;
            }

            if (constant == 0 &&
                binaryOperation.OperatorKind != BinaryOperatorKind.Equals &&
                binaryOperation.OperatorKind != BinaryOperatorKind.NotEquals &&
                binaryOperation.OperatorKind != BinaryOperatorKind.LessThan &&
                binaryOperation.OperatorKind != BinaryOperatorKind.GreaterThanOrEqual)
            {
                return false;
            }
            else if (constant == 1 &&
                binaryOperation.OperatorKind != BinaryOperatorKind.LessThanOrEqual &&
                binaryOperation.OperatorKind != BinaryOperatorKind.GreaterThan)
            {
                return false;
            }
            else if (constant > 1)
            {
                return false;
            }

            return IsCountAsyncMethodInvocationAwaited(binaryOperation.RightOperand, containingSymbol, methodName);
        }

        /// <summary>
        /// Tries the get an <see cref="int"/> constant from the <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="constant">The constant the <paramref name="operation"/> represents, if succeeded, or zero if <paramref name="operation"/> is not a constant.</param>
        /// <returns><see langword="true" /> <paramref name="operation"/> is a constant, <see langword="false" /> otherwise.</returns>
        public static bool TryGetInt32Constant(IOperation operation, out int constant)
        {
            constant = default;

            if (operation?.Type?.SpecialType != SpecialType.System_Int32)
            {
                return false;
            }

            var comparandValueOpt = operation.ConstantValue;

            if (!comparandValueOpt.HasValue)
            {
                return false;
            }

            constant = (int)comparandValueOpt.Value;

            return true;
        }

        /// <summary>
        /// Checks the <paramref name="operation"/> is an invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="containingSymbol">The containing symbol.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns><see langword="true" /> if the <paramref name="operation"/> is an invocation of the method <paramref name="methodName"/> in the <paramref name="containingSymbol"/>; 
        /// <see langword="false" /> otherwise.</returns>
        private static bool IsCountAsyncMethodInvocationAwaited(IOperation operation, INamedTypeSymbol containingSymbol, string methodName)
        {
            if (operation is IParenthesizedOperation parenthesizedOperation)
            {
                return IsCountAsyncMethodInvocationAwaited(parenthesizedOperation.Operand, containingSymbol, methodName);
            }

            return operation is IAwaitOperation awaitOperation &&
                awaitOperation.Operation is IInvocationOperation invocationOperation &&
                invocationOperation.TargetMethod.Name.Equals(methodName, StringComparison.Ordinal) &&
                invocationOperation.TargetMethod.ContainingSymbol.Equals(containingSymbol);
        }
    }
}
