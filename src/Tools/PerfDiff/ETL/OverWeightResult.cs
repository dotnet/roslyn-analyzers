// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace PerfDiff
{
    internal record OverWeightResult(string Name, float Before, float After, float Delta, float Overweight, float Percent, int Interest)
    {
        public override string ToString()
            => $"'{Name}':, Overweight: '{Overweight}%', Before: '{Before}ms', After: '{After}ms', Interest :'{Interest}'";
    }
}
