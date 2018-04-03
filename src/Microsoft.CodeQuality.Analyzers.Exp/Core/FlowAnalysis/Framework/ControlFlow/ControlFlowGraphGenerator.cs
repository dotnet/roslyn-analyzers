// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Operations.ControlFlow
{
    /// <summary>
    /// Class to generate <see cref="ControlFlowGraph"/>.
    /// NOTE: This class is temporary and will be removed once we move to the CFG exposed from Microsoft.CodeAnalysis
    /// </summary>
    internal class ControlFlowGraphGenerator
    {
        #region StatementCollector

        private sealed class StatementCollector : OperationWalker
        {
            private readonly List<IOperation> _statements;

            public StatementCollector()
            {
                _statements = new List<IOperation>();
            }

            public IEnumerable<IOperation> Statements
            {
                get => _statements;
            }

            public override void Visit(IOperation operation)
            {
                if (operation != null)
                {
                    var isStatement = IsStatement(operation);
                    var isBlockStatement = operation.Kind == OperationKind.Block;

                    if (isStatement && !isBlockStatement)
                    {
                        _statements.Add(operation);
                        return;
                    }
                }

                base.Visit(operation);
            }

            public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
            {
                // Include lamdas but don't include statements inside them,
                // because they don't belong to the lambda's containing method.
            }

            public override void VisitLocalFunction(ILocalFunctionOperation operation)
            {
                // Include local functions but don't include statements inside them,
                // because they don't belong to the local function's containing method.
            }

            private static bool IsStatement(IOperation operation)
            {
                switch (operation.Kind)
                {
                    case OperationKind.Invalid:
                    case OperationKind.Block:
                    case OperationKind.VariableDeclarationGroup:
                    case OperationKind.Switch:
                    case OperationKind.Conditional:
                    case OperationKind.Loop:
                    case OperationKind.Labeled:
                    case OperationKind.Branch:
                    case OperationKind.Empty:
                    case OperationKind.Return:
                    case OperationKind.YieldBreak:
                    case OperationKind.Lock:
                    case OperationKind.Try:
                    case OperationKind.Using:
                    case OperationKind.YieldReturn:
                    case OperationKind.ExpressionStatement:
                    case OperationKind.LocalFunction:
                    case OperationKind.Stop:
                    case OperationKind.End:
                    case OperationKind.Throw:
                        return operation.Type == null && !operation.ConstantValue.HasValue;

                    default:
                        return false;
                }
            }
        }

        #endregion

        private IList<BasicBlock> _blocks;
        private IDictionary<ILabelSymbol, BasicBlock> _labeledBlocks;
        private BasicBlock _currentBlock;
        private ControlFlowGraph _graph;

        public ControlFlowGraphGenerator()
        {
            _blocks = new List<BasicBlock>();
            _labeledBlocks = new Dictionary<ILabelSymbol, BasicBlock>();
        }

        public ControlFlowGraph Generate(IOperation body)
        {
            _graph = new ControlFlowGraph(body);

            CreateBlocks();
            ConnectBlocks();

            var result = _graph;

            _graph = null;
            _blocks.Clear();
            _labeledBlocks.Clear();

            return result;
        }

        private void CreateBlocks()
        {
            var collector = new StatementCollector();
            collector.Visit(_graph.RootOperation);

            foreach (var statement in collector.Statements)
            {
                Visit(statement);
            }
        }

        private void Visit(IOperation statement)
        {
            var isLastStatement = false;

            switch (statement.Kind)
            {
                case OperationKind.Labeled:
                    var label = (ILabeledOperation)statement;
                    _currentBlock = NewBlock();
                    _labeledBlocks.Add(label.Label, _currentBlock);
                    break;

                case OperationKind.Branch:
                    isLastStatement = true;
                    break;
            }

            if (_currentBlock == null)
            {
                _currentBlock = NewBlock();
            }

            _currentBlock.AddStatement(statement);

            if (isLastStatement)
            {
                _currentBlock = null;
            }
        }

        private BasicBlock NewBlock()
        {
            var block = new BasicBlock(BasicBlockKind.Block);

            _blocks.Add(block);
            _graph.AddBlock(block);
            return block;
        }

        private void ConnectBlocks()
        {
            var connectWithPrev = true;
            var prevBlock = _graph.Entry;

            _blocks.Add(_graph.Exit);

            foreach (var block in _blocks)
            {
                if (connectWithPrev)
                {
                    _graph.ConnectBlocks(prevBlock, block);
                }
                else
                {
                    connectWithPrev = true;
                }

                BasicBlock target = null;
                var lastStatement = block.Statements.LastOrDefault();

                switch (lastStatement)
                {
                    case IBranchOperation branch:
                        target = _labeledBlocks[branch.Target];
                        _graph.ConnectBlocks(block, target);
                        connectWithPrev = false;
                        break;

                    case IReturnOperation ret:
                        _graph.ConnectBlocks(block, _graph.Exit);
                        connectWithPrev = false;
                        break;
                }

                prevBlock = block;
            }
        }
    }
}