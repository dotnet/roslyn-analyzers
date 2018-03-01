// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Generates and stores the default <see cref="PointsToAbstractValue"/> for <see cref="AnalysisEntity"/> instances generated for member and element reference operations.
    /// </summary>
    internal sealed class DefaultPointsToValueGenerator
    {
        private readonly ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Builder _defaultPointsToValueMapBuilder
            = ImmutableDictionary.CreateBuilder<AnalysisEntity, PointsToAbstractValue>();
        private ImmutableDictionary<AnalysisEntity, PointsToAbstractValue> _lazyDefaultPointsToValueMap;
        public PointsToAbstractValue GetOrCreateDefaultValue(AnalysisEntity analysisEntity)
        {
            Debug.Assert(!analysisEntity.Type.HasValueCopySemantics());
            Debug.Assert(_lazyDefaultPointsToValueMap == null);

            if (!_defaultPointsToValueMapBuilder.TryGetValue(analysisEntity, out PointsToAbstractValue value))
            {
                value = analysisEntity.SymbolOpt?.Kind == SymbolKind.Local ?
                    PointsToAbstractValue.Undefined :
                    new PointsToAbstractValue(AbstractLocation.CreateAnalysisEntityDefaultLocation(analysisEntity));
                _defaultPointsToValueMapBuilder.Add(analysisEntity, value);
            }

            return value;
        }

        public ImmutableDictionary<AnalysisEntity, PointsToAbstractValue> GetDefaultPointsToValueMap()
        {
            _lazyDefaultPointsToValueMap = _lazyDefaultPointsToValueMap ?? _defaultPointsToValueMapBuilder.ToImmutableDictionary();
            return _lazyDefaultPointsToValueMap;
        }
    }
}
