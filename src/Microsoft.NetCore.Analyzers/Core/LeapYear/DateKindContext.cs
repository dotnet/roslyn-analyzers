// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.Analyzers.LeapYear
{
    public sealed class DateKindContext
    {
        public DateKindContext(ObjectCreationExpressionSyntax node, IMethodSymbol semanticMethodInfo)
        {
            this.ObjectCreationExpression = node;
            this.SemanticMethodInfo = semanticMethodInfo;
        }

        public string CurrentArgumentIdentifier { get; set; }

        public ObjectCreationExpressionSyntax ObjectCreationExpression { get; private set; }

        public IMethodSymbol SemanticMethodInfo { get; private set; }

        public int? MonthIntValue { get; set; }

        public int? DayIntValue { get; set; }

        public BinaryExpressionSyntax YearArgumentBinaryExpression { get; set; }

        public IdentifierNameSyntax YearArgumentIdentifier { get; set; }

        public BinaryExpressionSyntax YearArgumentIdentifierBinaryExpression { get; set; }

        public IList<ExpressionSyntax> YearArgumentExpressions { get; } = new List<ExpressionSyntax>();

        public IList<ExpressionSyntax> MonthArgumentExpressions { get; } = new List<ExpressionSyntax>();

        public bool IgnoreDiagnostic { get; private set; } = false;

        public string IgnoreDiagnosticReason { get; private set; }

        public void ShouldNotRaiseDiagnostic(string reason)
        {
            IgnoreDiagnostic = true;
            IgnoreDiagnosticReason = reason;
        }

        /// <summary>
        /// Stores the given expression in the property matching the current argument identifier.
        /// </summary>
        /// <param name="expression">The expression to store.</param>
        public void StoreArgumentExpression(ExpressionSyntax expression)
        {
            if (this.CurrentArgumentIdentifier == DateKindConstants.YearParameterIdentifer)
            {
                this.YearArgumentExpressions.Add(expression);
            }
            else if (this.CurrentArgumentIdentifier == DateKindConstants.MonthParameterIdentifier)
            {
                this.MonthArgumentExpressions.Add(expression);
            }
        }

        /// <summary>
        /// Examines stored code analysis to see if the date is safe from being a leap year day.
        /// </summary>
        /// <returns>True if able to determine that the date is safe from possible leap year issues.</returns>
        public bool AreMonthOrDayValuesSafe()
        {
            // Determine if this possible LeapYear issue can be ignored based
            // on month or day values
            if (this.MonthIntValue.HasValue)
            {
                if (this.MonthIntValue.Value == 2)
                {
                    if (this.DayIntValue.HasValue)
                    {
                        if (this.DayIntValue.Value == 29)
                        {
                            // Determined this date is refering to February 29, can not ignore.
                            return false;
                        }
                    }
                    else
                    {
                        // Month value is unsafe but unable to determine a day value,
                        // can not ignore.
                        return false;
                    }
                }
            }
            else
            {
                // Unable to determine a month value, we can still check for a day value.
                if (this.DayIntValue.HasValue)
                {
                    if (this.DayIntValue.Value == 29)
                    {
                        // Unable to determine a month value, but determined this date is
                        // refering to a day of 29, can not ignore.
                        return false;
                    }
                }
                else
                {
                    // Unable to determine a month or day value, can not ignore.
                    return false;
                }
            }

            return true;
        }
    }
}
