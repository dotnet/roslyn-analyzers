// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Analyzer.Utilities
{
    public class SolutionChangeAction : CodeAction
    {
        private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

        public SolutionChangeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution)
        {
            Title = title;
            _createChangedSolution = createChangedSolution;
        }

        public override string Title { get; }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            return _createChangedSolution(cancellationToken);
        }
    }
}