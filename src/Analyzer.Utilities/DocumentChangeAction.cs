// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Analyzer.Utilities
{
    public class DocumentChangeAction : CodeAction
    {
        private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;

        public DocumentChangeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
        {
            Title = title;
            _createChangedDocument = createChangedDocument;
        }

        public override string Title { get; }

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            return _createChangedDocument(cancellationToken);
        }
    }
}