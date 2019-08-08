// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
#pragma warning disable CA1200 // Avoid using cref tags with a prefix
    /// <summary>
    /// CA1827: Do not use Count()/LongCount() when Any() can be used.
    /// CA1828: Do not use CountAsync()/LongCountAsync() when AnyAsync() can be used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For non-empty sequences, <c>Count</c>/<c>CountAsync</c> enumerates the entire sequence,
    /// while <c>Any</c>/<c>AnyAsync</c> stops at the first item or the first item that satisfies a condition.
    /// </para>
    /// <para>
    /// <b>CA1827</b> applies to <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> and 
    /// <see cref="System.Linq.Enumerable.Any{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
    /// and covers the following use cases:
    /// </para>
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
    /// <remarks>
    /// <para>
    /// <b>CA1827</b> applies to <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource})"/> and
    /// <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource}, bool})"/>
    /// and covers the following use cases:
    /// </para>
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
    public sealed class DoNotUseCountWhenAnyCanBeUsedAnalyzer : DiagnosticAnalyzer
    {
        internal const string AsyncRuleId = "CA1828";
        internal const string SyncRuleId = "CA1827";
        private static readonly LocalizableString s_asyncLocalizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_asyncLocalizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_syncLocalizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_syncLocalizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_syncLocalizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly DiagnosticDescriptor s_asyncRule = new DiagnosticDescriptor(
            AsyncRuleId,
            s_asyncLocalizableTitle,
            s_asyncLocalizableMessage,
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
            description: s_localizableDescription,
#pragma warning disable CA1308 // Normalize strings to uppercase
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/" + AsyncRuleId.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase
        private static readonly DiagnosticDescriptor s_syncRule = new DiagnosticDescriptor(
            SyncRuleId,
            s_syncLocalizableTitle,
            s_syncLocalizableMessage,
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
            description: s_syncLocalizableDescription,
#pragma warning disable CA1308 // Normalize strings to uppercase
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/" + SyncRuleId.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase

        private static readonly ImmutableHashSet<string> s_syncMethodNames = ImmutableHashSet.Create(StringComparer.Ordinal, "Count", "LongCount");
        private static readonly ImmutableHashSet<string> s_asyncMethodNames = ImmutableHashSet.Create(StringComparer.Ordinal, "CountAsync", "LongCountAsync");

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        /// <value>The supported diagnostics.</value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s_syncRule, s_asyncRule);

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
            if (WellKnownTypes.Enumerable(context.Compilation) is INamedTypeSymbol enumerableType)
            {
                var operationActionsHandler = new OperationActionsHandler(
                    targetType: enumerableType,
                    targetMethodNames: s_syncMethodNames,
                    isAsync: false,
                    rule: s_syncRule);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeBinaryOperation,
                    OperationKind.BinaryOperator);
            }

            if (WellKnownTypes.Queryable(context.Compilation) is INamedTypeSymbol queryableType)
            {
                var operationActionsHandler = new OperationActionsHandler(
                    targetType: queryableType,
                    targetMethodNames: s_syncMethodNames,
                    isAsync: false,
                    rule: s_syncRule);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeBinaryOperation,
                    OperationKind.BinaryOperator);
            }

            if (context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions") is INamedTypeSymbol entityFrameworkQueryableExtensionsType)
            {
                var operationActionsHandler = new OperationActionsHandler(
                    targetType: entityFrameworkQueryableExtensionsType,
                    targetMethodNames: s_asyncMethodNames,
                    isAsync: true,
                    rule: s_asyncRule);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeBinaryOperation,
                    OperationKind.BinaryOperator);
            }

            if (context.Compilation.GetTypeByMetadataName("System.Data.Entity.QueryableExtensions") is INamedTypeSymbol queryableExtensionsType)
            {
                var operationActionsHandler = new OperationActionsHandler(
                    targetType: queryableExtensionsType,
                    targetMethodNames: s_asyncMethodNames,
                    isAsync: true,
                    rule: s_asyncRule);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeInvocationOperation,
                    OperationKind.Invocation);

                context.RegisterOperationAction(
                    operationActionsHandler.AnalyzeBinaryOperation,
                    OperationKind.BinaryOperator);
            }
        }

        /// <summary>
        /// Handler for operaction actions. This class cannot be inherited.
        /// </summary>
        private sealed class OperationActionsHandler
        {
            private readonly INamedTypeSymbol _targetType;
            private readonly ImmutableHashSet<string> _targetMethodNames;
            private readonly bool _isAsync;
            private readonly DiagnosticDescriptor _rule;

            public OperationActionsHandler(INamedTypeSymbol targetType, ImmutableHashSet<string> targetMethodNames, bool isAsync, DiagnosticDescriptor rule)
            {
                this._targetType = targetType;
                this._targetMethodNames = targetMethodNames;
                this._isAsync = isAsync;
                this._rule = rule;
            }

            /// <summary>
            /// Analyzes the invocation operation.
            /// </summary>
            /// <param name="context">The context.</param>
            public void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocationOperation = (IInvocationOperation)context.Operation;

                if (invocationOperation.Arguments.Length == 1)
                {
                    var method = invocationOperation.TargetMethod;
                    if (IsInt32EqualsMethod(method) &&
                        (IsCountEqualsZero(invocationOperation, out var methodName) || IsZeroEqualsCount(invocationOperation, out methodName)))
                    {
                        context.ReportDiagnostic(
                            invocationOperation.Syntax.CreateDiagnostic(
                                this._rule,
                                methodName));
                    }
                }
            }

            /// <summary>
            /// Analyzes the binary operation.
            /// </summary>
            /// <param name="context">The context.</param>
            public void AnalyzeBinaryOperation(OperationAnalysisContext context)
            {
                var binaryOperation = (IBinaryOperation)context.Operation;

                if (binaryOperation.IsComparisonOperator() &&
                    (IsLeftCountComparison(binaryOperation, out var methodName) || IsRightCountComparison(binaryOperation, out methodName)))
                {
                    context.ReportDiagnostic(
                        binaryOperation.Syntax.CreateDiagnostic(
                            this._rule,
                            methodName));
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
            /// Checks whether the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// is being compared with 0 using <see cref="int.Equals(int)"/>.
            /// </summary>
            /// <param name="invocationOperation">The invocation operation.</param>
            /// 
            /// 
            /// <returns><see langword="true" /> if the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// is being compared with 0 using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
            private bool IsCountEqualsZero(IInvocationOperation invocationOperation, out string methodName)
            {
                if (!TryGetInt32Constant(invocationOperation.Arguments[0].Value, out var constant) || constant != 0)
                {
                    methodName = null;
                    return false;
                }

                return IsCountMethodInvocationAwaited(invocationOperation.Instance, out methodName);
            }

            /// <summary>
            /// Checks whether 0 is being compared with the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// using <see cref="int.Equals(int)"/>.
            /// </summary>
            /// <param name="invocationOperation">The invocation operation.</param>
            /// <returns><see langword="true" /> if 0 is being compared with the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// using <see cref="int.Equals(int)"/>; otherwise, <see langword="false" />.</returns>
            private bool IsZeroEqualsCount(IInvocationOperation invocationOperation, out string methodName)
            {
                if (!TryGetInt32Constant(invocationOperation.Instance, out var constant) || constant != 0)
                {
                    methodName = null;
                    return false;
                }

                return IsCountMethodInvocationAwaited(invocationOperation.Arguments[0].Value, out methodName);
            }

            /// <summary>
            /// Checks whether the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators.
            /// </summary>
            /// <param name="binaryOperation">The binary operation.</param>
            /// <param name="methodName">If the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />, contains the method name; <see langword="null"/> otherwise.</param>
            /// <returns><see langword="true" /> if the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
            private bool IsLeftCountComparison(IBinaryOperation binaryOperation, out string methodName)
            {
                methodName = null;

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

                return IsCountMethodInvocationAwaited(binaryOperation.LeftOperand, out methodName);
            }

            /// <summary>
            /// Checks whether 0 or 1 is being compared with the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// using <see cref="int" /> comparison operators.
            /// </summary>
            /// <param name="binaryOperation">The binary operation.</param>
            /// <param name="methodName">If the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />, contains the method name; <see langword="null"/> otherwise.</param>
            /// <returns><see langword="true" /> if 0 or 1 is being compared with the value of the invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />
            /// using <see cref="int" /> comparison operators; otherwise, <see langword="false" />.</returns>
            private bool IsRightCountComparison(IBinaryOperation binaryOperation, out string methodName)
            {
                methodName = null;

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

                return IsCountMethodInvocationAwaited(binaryOperation.RightOperand, out methodName);
            }

            /// <summary>
            /// Tries the get an <see cref="int"/> constant from the <paramref name="operation"/>.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <param name="constant">The constant the <paramref name="operation"/> represents, if succeeded, or zero if <paramref name="operation"/> is not a constant.</param>
            /// <returns><see langword="true" /> <paramref name="operation"/> is a constant, <see langword="false" /> otherwise.</returns>
            private static bool TryGetInt32Constant(IOperation operation, out int constant)
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
            /// Checks the <paramref name="operation" /> is an invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <param name="methodName">If the <paramref name="operation" /> is an invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />, contains the method name; <see langword="null"/> otherwise.</param>
            /// <returns><see langword="true" /> if the <paramref name="operation" /> is an invocation of one of the <see cref="_targetMethodNames" /> in the <see cref="_targetType" />;
            /// <see langword="false" /> otherwise.</returns>
            private bool IsCountMethodInvocationAwaited(IOperation operation, out string methodName)
            {
                methodName = null;

                operation = operation.WalkDownParenthesis();

                if (this._isAsync)
                {
                    if (operation is IAwaitOperation awaitOperation)
                    {
                        operation = awaitOperation.Operation;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (operation is IInvocationOperation invocationOperation &&
                    this._targetMethodNames.Contains(invocationOperation.TargetMethod.Name) &&
                    invocationOperation.TargetMethod.ContainingSymbol.Equals(this._targetType))
                {
                    methodName = invocationOperation.TargetMethod.Name;
                    return true;
                }

                return false;
            }
        }
    }
}
