' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports Analyzer.Utilities.Extensions
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

#Disable Warning CA1200 ' Avoid using cref tags with a prefix
    ''' <summary>
    ''' CA1827: Do Not use Count()/LongCount() when Any() can be used.
    ''' CA1828: Do Not use CountAsync()/LongCountAsync() when AnyAsync() can be used.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' For non-empty sequences, <c>Count</c>/<c>CountAsync</c> enumerates the entire sequence,
    ''' while <c>Any</c>/<c>AnyAsync</c> stops at the first item Or the first item that satisfies a condition.
    ''' </para>
    ''' <para>
    ''' <b>CA1827</b> applies to <see cref="M:System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> And 
    ''' <see cref="M:System.Linq.Enumerable.Any{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
    ''' And covers the following use cases:
    ''' </para>
    ''' <list type="table">
    ''' <listheader><term>detected</term><term>fix</term></listheader>
    ''' <item><term><c> enumerable.Count(Function(x) True) = 0       </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True) &lt;> 0   </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True) &lt;= 0   </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True) > 0       </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True) &lt; 1    </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True) >= 1      </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> 0 = enumerable.Count(Function(x) True)       </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> 0 &lt;> enumerable.Count(Function(x) True)   </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> 0 &lt; enumerable.Count(Function(x) True)    </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> 0 >= enumerable.Count(Function(x) True)      </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> 1 > enumerable.Count(Function(x) True)       </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> 1 &lt;= enumerable.Count(Function(x) True)   </c></term><description><c> enumerable.Any(Function(x) True)  </c></description></item>
    ''' <item><term><c> enumerable.Count(Function(x) True).Equals(0) </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' <item><term><c> 0.Equals(enumerable.Count(Function(x) True)) </c></term><description><c> !enumerable.Any(Function(x) True) </c></description></item>
    ''' </list>
    ''' <para>
    ''' <b>CA1827</b> applies to <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource})"/> And
    ''' <see cref="T:Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync{TSource}(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource}, bool})"/>
    ''' And covers the following use cases:
    ''' </para>
    ''' <list type="table">
    ''' <listheader><term>detected</term><term>fix</term></listheader>
    ''' <item><term><c> await queryable.CountAsync() = 0               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> await queryable.CountAsync() &lt;> 0           </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> await queryable.CountAsync() &lt;= 0           </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> await queryable.CountAsync() > 0               </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> await queryable.CountAsync() &lt; 1            </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> await queryable.CountAsync() >= 1              </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> 0 = await queryable.CountAsync()               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> 0 &lt;> await queryable.CountAsync()           </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> 0 >= await queryable.CountAsync()              </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> 0 &lt; await queryable.CountAsync()            </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> 1 > await queryable.CountAsync()               </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> 1 &lt;= await queryable.CountAsync()           </c></term><description><c> await queryable.AnyAsync()  </c></description></item>
    ''' <item><term><c> (await queryable.CountAsync()).Equals(0)       </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> 0.Equals(await queryable.CountAsync())         </c></term><description><c> !await queryable.AnyAsync() </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) = 0       </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) &lt;> 0   </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) &lt;= 0   </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) > 0       </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) &lt; 1    </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True) >= 1      </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> 0 = await queryable.CountAsync(Function(x) True)       </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> 0 &lt;> await queryable.CountAsync(Function(x) True)   </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> 0 &lt; await queryable.CountAsync(Function(x) True)    </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> 0 >= await queryable.CountAsync(Function(x) True)      </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> 1 > await queryable.CountAsync(Function(x) True)       </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> 1 &lt;= await queryable.CountAsync(Function(x) True)   </c></term><description><c> await queryable.AnyAsync(Function(x) True)  </c></description></item>
    ''' <item><term><c> await queryable.CountAsync(Function(x) True).Equals(0) </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' <item><term><c> 0.Equals(await queryable.CountAsync(Function(x) True)) </c></term><description><c> !await queryable.AnyAsync(Function(x) True) </c></description></item>
    ''' </list>
    ''' </remarks>
#Enable Warning CA1200 ' Avoid using cref tags with a prefix
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotUseCountWhenAnyCanBeUsedAnalyzer
        Inherits DoNotUseCountWhenAnyCanBeUsedAnalyzer

        ''' <summary>
        ''' Creates the operation actions handler for enuumerables.
        ''' </summary>
        ''' <param name="targetType">Type of the target.</param>
        ''' <param name="targetMethodNames">The target method names.</param>
        ''' <param name="rule">The rule.</param>
        ''' <returns>OperationActionsHandler.</returns>
        Protected Overrides Function CreateOperationActionsHandlerForEnuumerables(targetType As INamedTypeSymbol, targetMethodNames As ImmutableHashSet(Of String), rule As DiagnosticDescriptor) As OperationActionsHandler

            Return New BasicOperationActionsHandlerForEnuumerables(targetType, targetMethodNames, rule)

        End Function

        Private NotInheritable Class BasicOperationActionsHandlerForEnuumerables
            Inherits OperationActionsHandler

            ''' <summary>
            ''' Initializes a new instance of the <see cref="BasicOperationActionsHandlerForEnuumerables"/> class.
            ''' </summary>
            ''' <param name="targetType">Type of the target.</param>
            ''' <param name="targetMethodNames">The target method names.</param>
            ''' <param name="rule">The rule.</param>
            Public Sub New(targetType As INamedTypeSymbol, targetMethodNames As ImmutableHashSet(Of String), rule As DiagnosticDescriptor)
                MyBase.New(targetType, targetMethodNames, rule)
            End Sub

            ''' <summary>
            ''' Checks the <paramref name="operation" /> is an invocation of one of the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetMethodNames" /> in the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetType" />.
            ''' </summary>
            ''' <param name="operation">The operation.</param>
            ''' <returns>The name of the invoked method if the value of the invocation of one of the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetMethodNames" /> in the <see cref="DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationActionsHandler.TargetType" />
            ''' is being compared with 0 or 1 using <see cref="Integer" /> comparison operators; otherwise, <see langword="null" />.</returns>
            Protected Overrides Function IsCountMethodInvocation(operation As IOperation) As String

                operation = operation.
                    WalkDownParenthesis().
                    WalkDownConversion().
                    WalkDownParenthesis()

                Dim invocationOperation = TryCast(operation, IInvocationOperation)

                If Not invocationOperation Is Nothing AndAlso
                    (invocationOperation.TargetMethod.Parameters.Length = 1 AndAlso
                        invocationOperation.TargetMethod.Name.Equals(DoNotUseCountWhenAnyCanBeUsedAnalyzer.CountMethodName, StringComparison.Ordinal) OrElse
                        invocationOperation.TargetMethod.Name.Equals(DoNotUseCountWhenAnyCanBeUsedAnalyzer.LongCountMethodName, StringComparison.Ordinal)) AndAlso
                    Me.TargetMethodNames.Contains(invocationOperation.TargetMethod.Name) AndAlso
                    invocationOperation.TargetMethod.ContainingSymbol.Equals(Me.TargetType) Then

                    Return invocationOperation.TargetMethod.Name

                End If

                Return Nothing

            End Function

        End Class

    End Class

End Namespace
