// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace PerfDiff
{
    internal sealed record OverWeightResult(string Name, float Before, float After, float Delta, float Overweight, float Percent, int Interest)
    {
        public override string ToString()
            => $"'{Name}':, Overweight: '{Overweight}%', Before: '{Before}ms', After: '{After}ms', Interest :'{Interest}'";
    }
}
