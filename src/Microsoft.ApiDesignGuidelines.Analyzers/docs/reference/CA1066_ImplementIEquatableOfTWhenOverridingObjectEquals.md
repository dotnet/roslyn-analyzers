# CA1066: Implement IEquatable<T\> when overriding Object.Equals

## Cause
A type overrides `Object.Equals` but does not implement the interface `IEquatable<T>`.

## Rule description
When a type `T` overrides `Object.Equals(object)`, the implementation must cast the `object` argument to the correct type `T` before performing the comparison. If the type implements `IEquatable<T>`, and therefore offers the method `T.Equals(T)`, and if the argument is known at compile time to be of type `T`, then the compiler can call `IEquatable<T>.Equals(T)` instead of `Object.Equals(object)`, and no cast is necessary, improving performance. 

## How to fix violations
Implement `IEquatable<T>`. Reimplement `Object.Equals(object)` to safely cast its argument to `T` and then call `IEquatable<T>.Equals(T)`.

## When to suppress warnings
There is no technical reason to suppress this warning.

## Example of a violation

### Description
The type `C` overrides `Object.Equals` but does not implement the interface `IEquatable<T>`.

### Code

    public class C
    {
        private readonly int _c;

        public C(int c)
        {
            _c = c;
        }

        public override bool Equals(object other)
        {
            C otherC = other as C;
            if (otherC == null)
            {
                return false;
            }

            return _c == otherC._c;
        }

        public override int GetHashCode()
        {
            return _c.GetHashCode();
        }
    }

## Example of how to fix

### Description
The type `C` now implements `IEquatable<T>`, and its override of `Object.Equals(object)` delegates to `IEquatable<T>.Equals(T)`.

### Code

    using System;

    public class C: IEquatable<C>
    {
        private readonly int _c;

        public C(int c)
        {
            _c = c;
        }

        public override bool Equals(object other)
        {
            C otherC = other as C;
            return Equals(otherC);
        }

        public override int GetHashCode()
        {
            return _c.GetHashCode();
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            return _c == other._c;
        }
    }

## Related rules

[CA1067: Override Object.Equals when implementing IEquatable<T\>](https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.ApiDesignGuidelines.Analyzers/docs/reference/CA1067_OverrideObjectEqualsWhenImplementingIEquatableOfT.md)
