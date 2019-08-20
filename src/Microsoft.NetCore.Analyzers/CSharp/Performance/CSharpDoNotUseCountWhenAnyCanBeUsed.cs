// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotUseCountWhenAnyCanBeUsedAnalyzer
        : DoNotUseCountWhenAnyCanBeUsedAnalyzer
    {
        /// <summary>
        /// Creates the operation actions handler for enuumerables.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetMethodNames">The target method names.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>The operation actions handler for enuumerables.</returns>
        protected override OperationActionsHandler CreateOperationActionsHandlerForEnuumerables(
            INamedTypeSymbol targetType,
            ImmutableHashSet<string> targetMethodNames,
            DiagnosticDescriptor rule)
            => new CSharpOperationActionsHandlerForEnuumerables(targetType, targetMethodNames, rule);

        /// <summary>
        /// Handler for operaction actions specific to <see cref="System.Linq.Enumerable"/> methods. This class cannot be inherited.
        /// </summary>
        private sealed class CSharpOperationActionsHandlerForEnuumerables
            : OperationActionsHandler
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CSharpOperationActionsHandlerForEnuumerables"/> class.
            /// </summary>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="targetMethodNames">The target method names.</param>
            /// <param name="rule">The rule.</param>
            public CSharpOperationActionsHandlerForEnuumerables(INamedTypeSymbol targetType, ImmutableHashSet<string> targetMethodNames, DiagnosticDescriptor rule)
                : base(targetType, targetMethodNames, rule)
            {
            }

            /// <summary>
            /// Checks the <paramref name="operation" /> is an invocation of one of the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetMethodNames" /> in the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetType" />.
            /// </summary>
            /// <param name="operation">The operation.</param>
            /// <returns>The name of the invoked method if the value of the invocation of one of the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetMethodNames" /> in the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetType" />
            /// is being compared with 0 or 1 using <see cref="int" /> comparison operators; otherwise, <see langword="null" />.</returns>
            protected sealed override string IsCountMethodInvocation(IOperation operation)
            {
                operation = operation
                    .WalkDownParenthesis()
                    .WalkDownConversion()
                    .WalkDownParenthesis();

                if (operation is IInvocationOperation invocationOperation &&
                    (invocationOperation.TargetMethod.Parameters.Length == 2 &&
                        invocationOperation.TargetMethod.Name.Equals(DoNotUseCountWhenAnyCanBeUsedAnalyzer.CountMethodName, StringComparison.Ordinal) ||
                        invocationOperation.TargetMethod.Name.Equals(DoNotUseCountWhenAnyCanBeUsedAnalyzer.LongCountMethodName, StringComparison.Ordinal)) &&
                    this.TargetMethodNames.Contains(invocationOperation.TargetMethod.Name) &&
                    invocationOperation.TargetMethod.ContainingSymbol.Equals(this.TargetType))
                {
                    return invocationOperation.TargetMethod.Name;
                }

                return null;
            }
        }
    }
}
