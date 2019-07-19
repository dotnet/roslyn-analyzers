// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    /// <summary>
    /// CA1827: Do not use Count() when Any() can be used.
    /// <para>
    /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> enumerates the entire enumerable
    /// while <see cref="System.Linq.Enumerable.Any{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> will only enumerates, at most, up until the first item.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Use cases covered:
    /// <list type="table">
    /// <listheader><term>detected</term><term>fix</term></listheader>
    /// <item><term><c> enumerable.Count() == 0               </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> enumerable.Count() != 0               </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> enumerable.Count() &lt;= 0            </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> enumerable.Count() > 0                </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> enumerable.Count() &lt; 1             </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> enumerable.Count() >= 1               </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> 0 == enumerable.Count()               </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> 0 != enumerable.Count()               </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> 0 >= enumerable.Count()               </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> 0 &lt; enumerable.Count()             </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> 1 > enumerable.Count()                </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> 1 &lt;= enumerable.Count()            </c></term><description><c> enumerable.Any()  </c></description></item>
    /// <item><term><c> enumerable.Count().Equals(0)          </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> 0.Equals(enumerable.Count())          </c></term><description><c> !enumerable.Any() </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) == 0      </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) != 0      </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) &lt;= 0   </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) > 0       </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) &lt; 1    </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true) >= 1      </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> 0 == enumerable.Count(_ => true)      </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> 0 != enumerable.Count(_ => true)      </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> 0 &lt; enumerable.Count(_ => true)    </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> 0 >= enumerable.Count(_ => true)      </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> 1 > enumerable.Count(_ => true)       </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> 1 &lt;= enumerable.Count(_ => true)   </c></term><description><c> enumerable.Any(_ => true)  </c></description></item>
    /// <item><term><c> enumerable.Count(_ => true).Equals(0) </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// <item><term><c> 0.Equals(enumerable.Count(_ => true)) </c></term><description><c> !enumerable.Any(_ => true) </c></description></item>
    /// </list>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseCountWhenAnyCanBeUsedAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1827";
        private const string CountMethodName = "Count";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
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
            var enumerableType = WellKnownTypes.Enumerable(context.Compilation);

            if (enumerableType == null)
            {
                return;
            }

            context.RegisterOperationAction(
                operationAnalysisContext => AnalyzeInvocationExpression((IInvocationOperation)operationAnalysisContext.Operation, enumerableType, operationAnalysisContext.ReportDiagnostic),
                OperationKind.Invocation);

            context.RegisterOperationAction(
                operationAnalysisContext => AnalyzeBinaryExpression((IBinaryOperation)operationAnalysisContext.Operation, enumerableType, operationAnalysisContext.ReportDiagnostic),
                OperationKind.BinaryOperator);
        }

        /// <summary>
        /// Check to see if we have an expression comparing the result of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> or
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})"/>
        /// using <see cref="int.Equals(int)"/>.
        /// </summary>
        private static void AnalyzeInvocationExpression(IInvocationOperation invocationOperation, INamedTypeSymbol enumerableType, Action<Diagnostic> reportDiagnostic)
        {
            if (invocationOperation.Arguments.Length == 1)
            {
                var methodSymbol = invocationOperation.TargetMethod;
                if (IsInt32EqualsMethod(methodSymbol) &&
                    (IsCountEqualsZero(invocationOperation, enumerableType) || IsZeroEqualsCount(invocationOperation, enumerableType)))
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
        /// Checks whether the value of the invocation of <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
        /// is being compared with 0 using <see cref="int.Equals(int)"/>.
        /// </summary>
        /// <param name="invocationOperation">The invocation operation.</param>
        /// <param name="enumerableType">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if the value of the invocation of <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
        /// is being compared with 0 using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
        private static bool IsCountEqualsZero(IInvocationOperation invocationOperation, INamedTypeSymbol enumerableType)
        {
            if (!TryGetInt32Constant(invocationOperation.Arguments[0].Value, out var constant) || constant != 0)
            {
                return false;
            }

            return IsCountMethodInvocation(invocationOperation.Instance, enumerableType);
        }

        /// <summary>
        /// Checks whether 0 is being compared with the value of the invocation of 
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
        /// using <see cref="int.Equals(int)"/>.
        /// </summary>
        /// <param name="invocationOperation">The invocation operation.</param>
        /// <param name="enumerableType">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if 0 is being compared with the value of the invocation of 
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
        /// using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
        private static bool IsZeroEqualsCount(IInvocationOperation invocationOperation, INamedTypeSymbol enumerableType)
        {
            if (!TryGetInt32Constant(invocationOperation.Instance, out var constant) || constant != 0)
            {
                return false;
            }

            return IsCountMethodInvocation(invocationOperation.Arguments[0].Value, enumerableType);
        }

        /// <summary>
        /// Check to see if we have an expression comparing the result of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> or
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})"/>
        /// using operators.
        /// </summary>
        private static void AnalyzeBinaryExpression(IBinaryOperation binaryOperation, INamedTypeSymbol enumerableType, Action<Diagnostic> reportDiagnostic)
        {
            if (binaryOperation.IsComparisonOperator() &&
                (IsLeftCountComparison(binaryOperation, enumerableType) || IsRightCountComparison(binaryOperation, enumerableType)))
            {
                reportDiagnostic(binaryOperation.Syntax.CreateDiagnostic(s_rule));
            }
        }

        /// <summary>
        /// Checks whether the value of the invocation of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})" />
        /// is being compared with 0 or 1 using <see cref="int" /> comparison operators.
        /// </summary>
        /// <param name="binaryOperation">The binary operation.</param>
        /// <param name="enumerableType">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if the value of the invocation of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})" />
        /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
        private static bool IsLeftCountComparison(IBinaryOperation binaryOperation, INamedTypeSymbol enumerableType)
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

            return IsCountMethodInvocation(binaryOperation.LeftOperand, enumerableType);
        }

        /// <summary>
        /// Checks whether 0 or 1 is being compared with the value of the invocation of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})" />
        /// using <see cref="int" /> comparison operators.
        /// </summary>
        /// <param name="binaryOperation">The binary operation.</param>
        /// <param name="enumerableType">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if 0 or 1 is being compared with the value of the invocation of
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})" />
        /// using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
        private static bool IsRightCountComparison(IBinaryOperation binaryOperation, INamedTypeSymbol enumerableType)
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

            return IsCountMethodInvocation(binaryOperation.RightOperand, enumerableType);
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
        /// Checks the is a <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})" />
        /// or <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})" /> invocation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="enumerableType">Type of the enumerable.</param>
        /// <returns><see langword="true" /> if XXXX, <see langword="false" /> otherwise.</returns>
        private static bool IsCountMethodInvocation(IOperation operation, INamedTypeSymbol enumerableType)
        {
            return operation is IInvocationOperation invocationExpression &&
                invocationExpression.TargetMethod.Name.Equals(DoNotUseCountWhenAnyCanBeUsedAnalyzer.CountMethodName, StringComparison.Ordinal) &&
                invocationExpression.TargetMethod.ContainingSymbol.Equals(enumerableType);
        }
    }
}
