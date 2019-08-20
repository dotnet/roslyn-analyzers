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
    public abstract class DoNotUseCountWhenAnyCanBeUsedAnalyzer : DiagnosticAnalyzer
    {
        internal const string AsyncRuleId = "CA1828";
        internal const string SyncRuleId = "CA1827";
        internal const string OperationKey = nameof(OperationKey);
        internal const string IsAsyncKey = nameof(IsAsyncKey);
        internal const string ShouldNegateKey = nameof(ShouldNegateKey);
        internal const string OperationEqualsInstance = nameof(OperationEqualsInstance);
        internal const string OperationEqualsArgument = nameof(OperationEqualsArgument);
        internal const string OperationBinaryLeft = nameof(OperationBinaryLeft);
        internal const string OperationBinaryRight = nameof(OperationBinaryRight);
        internal const string CountMethodName = "Count";
        internal const string LongCountMethodName = "LongCount";
        internal const string CountAsyncMethodName = "CountAsync";
        internal const string LongCountAsyncMethodName = "LongCountAsync";
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

        private static readonly ImmutableHashSet<string> s_syncMethodNames = ImmutableHashSet.Create(StringComparer.Ordinal, CountMethodName, LongCountMethodName);
        private static readonly ImmutableHashSet<string> s_asyncMethodNames = ImmutableHashSet.Create(StringComparer.Ordinal, CountAsyncMethodName, LongCountAsyncMethodName);

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
        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (WellKnownTypes.Enumerable(context.Compilation) is INamedTypeSymbol enumerableType)
            {
                var operationActionsHandler = CreateOperationActionsHandlerForEnuumerables(
                    targetType: enumerableType,
                    targetMethodNames: s_syncMethodNames,
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
                var operationActionsHandler = new AsyncOperationActionsHandler(
                    targetType: entityFrameworkQueryableExtensionsType,
                    targetMethodNames: s_asyncMethodNames,
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
                var operationActionsHandler = new AsyncOperationActionsHandler(
                    targetType: queryableExtensionsType,
                    targetMethodNames: s_asyncMethodNames,
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
        /// Creates the operation actions handler for enuumerables.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetMethodNames">The target method names.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>The operation actions handler for enuumerables.</returns>
        protected abstract OperationActionsHandler CreateOperationActionsHandlerForEnuumerables(INamedTypeSymbol targetType, ImmutableHashSet<string> targetMethodNames, DiagnosticDescriptor rule);

        /// <summary>
        /// Handler for operaction actions.
        /// </summary>
        protected class OperationActionsHandler
        {
            private readonly DiagnosticDescriptor _rule;

            /// <summary>
            /// Initializes a new instance of the <see cref="OperationActionsHandler"/> class.
            /// </summary>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="targetMethodNames">The target method names.</param>
            /// <param name="rule">The rule.</param>
            public OperationActionsHandler(INamedTypeSymbol targetType, ImmutableHashSet<string> targetMethodNames, DiagnosticDescriptor rule)
            {
                this.TargetType = targetType;
                this.TargetMethodNames = targetMethodNames;
                this._rule = rule;
            }

            /// <summary>
            /// Gets the type of the target.
            /// </summary>
            /// <value>The type of the target.</value>
            protected INamedTypeSymbol TargetType { get; }

            /// <summary>
            /// Gets the target method names.
            /// </summary>
            /// <value>The target method names.</value>
            protected ImmutableHashSet<string> TargetMethodNames { get; }

            /// <summary>
            /// Analyzes the invocation operation.
            /// </summary>
            /// <param name="context">The context.</param>
            public void AnalyzeInvocationOperation(OperationAnalysisContext context)
            {
                var invocationOperation = (IInvocationOperation)context.Operation;
                var method = invocationOperation.TargetMethod;

                if (invocationOperation.Arguments.Length == 1 &&
                    IsEqualsMethod(method))
                {
                    string operationKey;

                    var methodName = IsCountEqualsZero(invocationOperation);

                    if (methodName is object)
                    {
                        operationKey = OperationEqualsInstance;
                    }
                    else
                    {
                        methodName = IsZeroEqualsCount(invocationOperation);

                        if (methodName is object)
                        {
                            operationKey = OperationEqualsArgument;
                        }
                        else
                        {
                            return;
                        }
                    }

                    var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                    propertiesBuilder.Add(OperationKey, operationKey);
                    propertiesBuilder.Add(ShouldNegateKey, null);
                    this.AddDiagnosticsProperties(propertiesBuilder);
                    var properties = propertiesBuilder.ToImmutable();

                    context.ReportDiagnostic(
                        invocationOperation.Syntax.CreateDiagnostic(
                            rule: this._rule,
                            properties: properties,
                            args: methodName));
                }
            }

            /// <summary>
            /// Analyzes the binary operation.
            /// </summary>
            /// <param name="context">The context.</param>
            public void AnalyzeBinaryOperation(OperationAnalysisContext context)
            {
                var binaryOperation = (IBinaryOperation)context.Operation;

                if (binaryOperation.IsComparisonOperator())
                {
                    string operationKey;

                    var methodName = IsLeftCountComparison(binaryOperation, out var shouldNegate);

                    if (methodName is object)
                    {
                        operationKey = OperationBinaryLeft;
                    }
                    else
                    {
                        methodName = IsRightCountComparison(binaryOperation, out shouldNegate);

                        if (methodName is object)
                        {
                            operationKey = OperationBinaryRight;
                        }
                        else
                        {
                            return;
                        }
                    }

                    var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                    propertiesBuilder.Add(OperationKey, operationKey);
                    if (shouldNegate) propertiesBuilder.Add(ShouldNegateKey, null);
                    this.AddDiagnosticsProperties(propertiesBuilder);
                    var properties = propertiesBuilder.ToImmutable();

                    context.ReportDiagnostic(
                        binaryOperation.Syntax.CreateDiagnostic(
                            rule: this._rule,
                            properties: properties,
                            args: methodName));
                }
            }

            /// <summary>
            /// Adds handler specifc diagnostics properties.
            /// </summary>
            /// <param name="propertiesBuilder">The properties builder.</param>
            protected virtual void AddDiagnosticsProperties(ImmutableDictionary<string, string>.Builder propertiesBuilder)
            {
            }

            /// <summary>
            /// Checks if the given method is the <see cref="int.Equals(int)"/> method.
            /// </summary>
            /// <param name="methodSymbol">The method symbol.</param>
            /// <returns><see langword="true"/> if the given method is the <see cref="int.Equals(int)"/> method; otherwise, <see langword="false"/>.</returns>
            private static bool IsEqualsMethod(IMethodSymbol methodSymbol)
            {
                return string.Equals(methodSymbol.Name, WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal) &&
                       (methodSymbol.ContainingType.SpecialType == SpecialType.System_Int32 ||
                            methodSymbol.ContainingType.SpecialType == SpecialType.System_UInt32 ||
                            methodSymbol.ContainingType.SpecialType == SpecialType.System_Int64 ||
                            methodSymbol.ContainingType.SpecialType == SpecialType.System_UInt64);
            }

            /// <summary>
            /// Checks whether the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 using <see cref="int.Equals(int)"/>.
            /// </summary>
            /// <param name="invocationOperation">The invocation operation.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 using <see cref="int.Equals(int)"/>; otherwise, <see langword="null" />.</returns>
            private string IsCountEqualsZero(IInvocationOperation invocationOperation)
            {
                if (!TryGetZeroOrOneConstant(invocationOperation.Arguments[0].Value, out var constant) || constant != 0)
                {
                    return null;
                }

                return IsCountMethodInvocation(invocationOperation.Instance);
            }

            /// <summary>
            /// Checks whether 0 is being compared with the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// using <see cref="int.Equals(int)"/>.
            /// </summary>
            /// <param name="invocationOperation">The invocation operation.</param>
            /// <returns>The name of the invoked method if 0 is being compared with the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// using <see cref="int.Equals(int)"/>; otherwise, <see langword="null" />.</returns>
            private string IsZeroEqualsCount(IInvocationOperation invocationOperation)
            {
                if (!TryGetZeroOrOneConstant(invocationOperation.Instance, out var constant) || constant != 0)
                {
                    return null;
                }

                return IsCountMethodInvocation(invocationOperation.Arguments[0].Value);
            }

            /// <summary>
            /// Checks whether the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators.
            /// </summary>
            /// <param name="binaryOperation">The binary operation.</param>
            /// <param name="shouldNegate">If set to <see langword="true" /> the result of the invocation should be negated.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="null" />.</returns>
            private string IsLeftCountComparison(IBinaryOperation binaryOperation, out bool shouldNegate)
            {
                shouldNegate = false;

                if (!TryGetZeroOrOneConstant(binaryOperation.RightOperand, out var constant))
                {
                    return null;
                }

                switch (constant)
                {
                    case 0:
                        switch (binaryOperation.OperatorKind)
                        {
                            case BinaryOperatorKind.Equals:
                            case BinaryOperatorKind.LessThanOrEqual:
                                shouldNegate = true;
                                break;
                            case BinaryOperatorKind.NotEquals:
                            case BinaryOperatorKind.GreaterThan:
                                shouldNegate = false;
                                break;
                            default:
                                return null;
                        }

                        break;
                    case 1:
                        switch (binaryOperation.OperatorKind)
                        {
                            case BinaryOperatorKind.LessThan:
                                shouldNegate = true;
                                break;
                            case BinaryOperatorKind.GreaterThanOrEqual:
                                shouldNegate = false;
                                break;
                            default:
                                return null;
                        }

                        break;
                    default:
                        return null;
                }

                return IsCountMethodInvocation(binaryOperation.LeftOperand);
            }

            /// <summary>
            /// Checks whether 0 or 1 is being compared with the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// using <see cref="int" /> comparison operators.
            /// </summary>
            /// <param name="binaryOperation">The binary operation.</param>
            /// <param name="shouldNegate">If set to <see langword="true" /> the result of the invocation should be negated.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="null" />.</returns>
            private string IsRightCountComparison(IBinaryOperation binaryOperation, out bool shouldNegate)
            {
                shouldNegate = false;

                if (!TryGetZeroOrOneConstant(binaryOperation.LeftOperand, out var constant))
                {
                    return null;
                }

                switch (constant)
                {
                    case 0:
                        switch (binaryOperation.OperatorKind)
                        {
                            case BinaryOperatorKind.Equals:
                            case BinaryOperatorKind.LessThan:
                                shouldNegate = true;
                                break;

                            case BinaryOperatorKind.NotEquals:
                            case BinaryOperatorKind.GreaterThanOrEqual:
                                shouldNegate = false;
                                break;

                            default:
                                return null;
                        }

                        break;
                    case 1:
                        switch (binaryOperation.OperatorKind)
                        {
                            case BinaryOperatorKind.LessThanOrEqual:
                                shouldNegate = false;
                                break;

                            case BinaryOperatorKind.GreaterThan:
                                shouldNegate = true;
                                break;

                            default:
                                return null;
                        }

                        break;
                    default:
                        return null;
                }

                return IsCountMethodInvocation(binaryOperation.RightOperand);
            }

            /// <summary>
            /// Tries the get an <see cref="int"/> constant from the <paramref name="operation"/>.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <param name="constant">If this method returns <see langword="true"/>, this parameter is guaranteed to be 0 or 1; otherwise, it's meaningless.</param>
            /// <returns><see langword="true" /> <paramref name="operation"/> is a 0 or 1 constant, <see langword="false" /> otherwise.</returns>
            private static bool TryGetZeroOrOneConstant(IOperation operation, out int constant)
            {
                constant = default;

                if (operation?.Type?.SpecialType != SpecialType.System_Int32 &&
                    operation?.Type?.SpecialType != SpecialType.System_Int64 &&
                    operation?.Type?.SpecialType != SpecialType.System_UInt32 &&
                    operation?.Type?.SpecialType != SpecialType.System_UInt64 &&
                    operation?.Type?.SpecialType != SpecialType.System_Object)
                {
                    return false;
                }

                operation = operation.WalkDownConversion();

                var comparandValueOpt = operation.ConstantValue;

                if (!comparandValueOpt.HasValue)
                {
                    return false;
                }

                switch (comparandValueOpt.Value)
                {
                    case int intValue:

                        if (intValue >= 0 && intValue <= 1)
                        {
                            constant = intValue;
                            return true;
                        }

                        break;

                    case uint uintValue:

                        if (uintValue >= 0 && uintValue <= 1)
                        {
                            constant = (int)uintValue;
                            return true;
                        }

                        break;

                    case long longValue:

                        if (longValue >= 0 && longValue <= 1)
                        {
                            constant = (int)longValue;
                            return true;
                        }

                        break;

                    case ulong ulongValue:

                        if (ulongValue >= 0 && ulongValue <= 1)
                        {
                            constant = (int)ulongValue;
                            return true;
                        }

                        break;
                }

                return false;
            }

            /// <summary>
            /// Checks the <paramref name="operation" /> is an invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="TargetMethodNames" /> in the <see cref="TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="null" />.</returns>
            protected virtual string IsCountMethodInvocation(IOperation operation)
            {
                operation = operation
                    .WalkDownParenthesis()
                    .WalkDownConversion()
                    .WalkDownParenthesis();

                if (operation is IInvocationOperation invocationOperation &&
                    this.TargetMethodNames.Contains(invocationOperation.TargetMethod.Name) &&
                    invocationOperation.TargetMethod.ContainingSymbol.Equals(this.TargetType))
                {
                    return invocationOperation.TargetMethod.Name;
                }

                return null;
            }
        }

        /// <summary>
        /// Handler for operaction actions specific to asynchronous methods. This class cannot be inherited.
        /// </summary>
        private sealed class AsyncOperationActionsHandler
            : OperationActionsHandler
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncOperationActionsHandler"/> class.
            /// </summary>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="targetMethodNames">The target method names.</param>
            /// <param name="rule">The rule.</param>
            public AsyncOperationActionsHandler(INamedTypeSymbol targetType, ImmutableHashSet<string> targetMethodNames, DiagnosticDescriptor rule)
                : base(targetType, targetMethodNames, rule)
            {
            }

            /// <summary>
            /// Checks the <paramref name="operation" /> is an invocation of one of the <see cref="OperationActionsHandler.TargetMethodNames" /> in the <see cref="OperationActionsHandler.TargetType" />.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="OperationActionsHandler.TargetMethodNames" /> in the <see cref="OperationActionsHandler.TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="null" />.</returns>
            protected override string IsCountMethodInvocation(IOperation operation)
            {
                operation = operation
                    .WalkDownParenthesis()
                    .WalkDownConversion();

                if (operation is IAwaitOperation awaitOperation)
                {
                    operation = awaitOperation.Operation;
                }
                else
                {
                    return null;
                }

                operation = operation
                    .WalkDownConversion()
                    .WalkDownParenthesis();

                if (operation is IInvocationOperation invocationOperation &&
                    this.TargetMethodNames.Contains(invocationOperation.TargetMethod.Name) &&
                    invocationOperation.TargetMethod.ContainingSymbol.Equals(this.TargetType))
                {
                    return invocationOperation.TargetMethod.Name;
                }

                return null;
            }

            /// <summary>
            /// Adds handler specifc diagnostics properties.
            /// </summary>
            /// <param name="propertiesBuilder">The properties builder.</param>
            protected override void AddDiagnosticsProperties(ImmutableDictionary<string, string>.Builder propertiesBuilder)
            {
                base.AddDiagnosticsProperties(propertiesBuilder);

                propertiesBuilder.Add(IsAsyncKey, null);
            }
        }
    }
}
