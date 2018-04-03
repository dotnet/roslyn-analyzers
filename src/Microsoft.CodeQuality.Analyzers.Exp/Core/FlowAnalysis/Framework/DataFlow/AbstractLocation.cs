// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// <para>
    /// Represents an abstract analysis location.
    /// This is may be used to represent a location where an <see cref="AnalysisEntity"/> resides, i.e. <see cref="AnalysisEntity.InstanceLocation"/> or
    /// a location that is pointed to by a reference type variable, and tracked with <see cref="PointsToAnalysis.PointsToAnalysis"/>.
    /// </para>
    /// <para>
    /// An analysis location can be created for one of the following cases:
    ///     1. An allocation or an object creation operation (<see cref="CreateAllocationLocation(IOperation, ITypeSymbol)"/>).
    ///     2. Location for the implicit 'this' or 'Me' instance being analyzed (<see cref="CreateThisOrMeLocation(INamedTypeSymbol)"/>).
    ///     3. Location created for certain symbols which do not have a declaration in executable code, i.e. no <see cref="IOperation"/> for declaration (such as parameter symbols, member symbols, etc. - <see cref="CreateSymbolLocation(ISymbol)"/>).
    /// </para>
    /// </summary>
    internal sealed class AbstractLocation : CacheBasedEquatable<AbstractLocation>
    {
        public static readonly AbstractLocation Null = new AbstractLocation(creationOpt: null, analysisEntityOpt: null, symbolOpt: null, locationTypeOpt: null);

        private AbstractLocation(IOperation creationOpt, AnalysisEntity analysisEntityOpt, ISymbol symbolOpt, ITypeSymbol locationTypeOpt)
        {
            CreationOpt = creationOpt;
            AnalysisEntityOpt = analysisEntityOpt;
            SymbolOpt = symbolOpt;
            LocationTypeOpt = locationTypeOpt;
        }

        private static AbstractLocation Create(IOperation creationOpt, AnalysisEntity analysisEntityOpt, ISymbol symbolOpt, ITypeSymbol locationType)
        {
            Debug.Assert(creationOpt != null ^ symbolOpt != null ^ analysisEntityOpt != null);
            Debug.Assert(locationType != null);

            return new AbstractLocation(creationOpt, analysisEntityOpt, symbolOpt, locationType);
        }

        public static AbstractLocation CreateAllocationLocation(IOperation creation, ITypeSymbol locationType) => Create(creation, analysisEntityOpt: null, symbolOpt: null, locationType: locationType);
        public static AbstractLocation CreateAnalysisEntityDefaultLocation(AnalysisEntity analysisEntity) => Create(creationOpt: null, analysisEntityOpt: analysisEntity, symbolOpt: null, locationType: analysisEntity.Type);
        public static AbstractLocation CreateThisOrMeLocation(INamedTypeSymbol namedTypeSymbol) => Create(creationOpt: null, analysisEntityOpt: null, symbolOpt: namedTypeSymbol, locationType: namedTypeSymbol);
        public static AbstractLocation CreateSymbolLocation(ISymbol symbol) => Create(creationOpt: null, analysisEntityOpt: null, symbolOpt: symbol, locationType: symbol.GetMemerOrLocalOrParameterType());

        public IOperation CreationOpt { get; }
        public AnalysisEntity AnalysisEntityOpt { get; }
        public ISymbol SymbolOpt { get; }
        public ITypeSymbol LocationTypeOpt { get; }
        public bool IsNull
        {
            get
            {
                var isNull = ReferenceEquals(this, Null);
                Debug.Assert(isNull || LocationTypeOpt != null);
                return isNull;
            }
        }

        protected override int ComputeHashCode()
        {
            return HashUtilities.Combine(CreationOpt?.GetHashCode() ?? 0,
                HashUtilities.Combine(SymbolOpt?.GetHashCode() ?? 0,
                HashUtilities.Combine(AnalysisEntityOpt?.GetHashCode() ?? 0, LocationTypeOpt?.GetHashCode() ?? 0)));
        }
    }
}
