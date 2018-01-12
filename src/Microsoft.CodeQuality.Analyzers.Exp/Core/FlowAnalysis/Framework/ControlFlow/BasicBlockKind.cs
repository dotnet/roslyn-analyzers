// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.ControlFlow
{
    /// <summary>
    /// Kind of <see cref="BasicBlock"/>.
    /// NOTE: This class is temporary and will be removed once we move to the CFG exposed from Microsoft.CodeAnalysis
    /// </summary>
    internal enum BasicBlockKind
    {
        Entry,
        Exit,
        Block
    }
}
