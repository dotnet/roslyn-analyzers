// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.ControlFlow
{
    /// <summary>
    /// Basic block of a <see cref="ControlFlowGraph"/>.
    /// NOTE: This class is temporary and will be removed once we move to the CFG exposed from Microsoft.CodeAnalysis
    /// </summary>
    [DebuggerDisplay("{Kind} ({Statements.Length} statements)")]
    internal class BasicBlock
    {
        private readonly ImmutableArray<IOperation>.Builder _statements;
        private readonly ImmutableHashSet<BasicBlock>.Builder _successors;
        private readonly ImmutableHashSet<BasicBlock>.Builder _predecessors;

        public BasicBlock(BasicBlockKind kind)
        {
            Kind = kind;
            _statements = ImmutableArray.CreateBuilder<IOperation>();
            _successors = ImmutableHashSet.CreateBuilder<BasicBlock>();
            _predecessors = ImmutableHashSet.CreateBuilder<BasicBlock>();
        }

        public BasicBlockKind Kind { get; private set; }
        public ImmutableArray<IOperation> Statements => _statements.ToImmutable();
        public ImmutableHashSet<BasicBlock> Successors => _successors.ToImmutable();
        public ImmutableHashSet<BasicBlock> Predecessors => _predecessors.ToImmutable();
        internal void AddStatement(IOperation statement) => _statements.Add(statement);
        internal void AddSuccessor(BasicBlock block) => _successors.Add(block);
        internal void AddPredecessor(BasicBlock block) => _predecessors.Add(block);
    }
}
