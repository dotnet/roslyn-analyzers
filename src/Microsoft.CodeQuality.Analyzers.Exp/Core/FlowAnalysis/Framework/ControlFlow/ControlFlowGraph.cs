// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.ControlFlow
{
    /// <summary>
    /// IOperation based CFG for an executable code block.
    /// NOTE: This class is temporary and will be removed once we move to the CFG exposed from Microsoft.CodeAnalysis
    /// </summary>
    [DebuggerDisplay("CFG ({_blocks.Count} blocks)")]
    internal class ControlFlowGraph : IEquatable<ControlFlowGraph>
    {
        private ImmutableHashSet<BasicBlock>.Builder _blocks;

        public static ControlFlowGraph Create(IOperation body)
        {
            var generator = new ControlFlowGraphGenerator();
            var result = generator.Generate(body);
            return result;
        }

        public ControlFlowGraph(IOperation rootOperation)
        {
            RootOperation = rootOperation;

            _blocks = ImmutableHashSet.CreateBuilder<BasicBlock>();
            Entry = new BasicBlock(BasicBlockKind.Entry);
            Exit = new BasicBlock(BasicBlockKind.Exit);

            AddBlock(Entry);
            AddBlock(Exit);
        }

        public IOperation RootOperation { get; }
        public BasicBlock Entry { get; private set; }
        public BasicBlock Exit { get; private set; }
        public ImmutableHashSet<BasicBlock> Blocks => _blocks.ToImmutable();

        internal void AddBlock(BasicBlock block)
        {
            _blocks.Add(block);
        }

        internal void ConnectBlocks(BasicBlock from, BasicBlock to)
        {
            from.AddSuccessor(to);
            to.AddPredecessor(from);
            _blocks.Add(from);
            _blocks.Add(to);
        }

        public override bool Equals(object obj) => Equals(obj as ControlFlowGraph);
        public bool Equals(ControlFlowGraph other) => RootOperation == other?.RootOperation;
        public override int GetHashCode() => RootOperation.GetHashCode();
    }
}