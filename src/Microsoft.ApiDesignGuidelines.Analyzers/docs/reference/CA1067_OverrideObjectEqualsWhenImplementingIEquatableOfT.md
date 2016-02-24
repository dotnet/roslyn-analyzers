# CA1067: Override Object.Equals when implementing IEquatable<T\>

## Cause
A type implements the interface `IEquatable<T>` but does not override `Object.Equals`.

## Rule description
When a type `T` implements the interface `IEquatable<T>`, it suggests to a user who sees a call to the `Equals` method in source code that an instance of the type can be equated with an instance of any other type. The user might be confused if their attempt to equate the type with an instance of another type fails to compile. This violates the "principle of least surprise".

## How to fix violations
Implement `Object.Equals(object)` to safely cast its argument to `T` and the call `IEquatable<T>.Equals(T)`.

## When to suppress warnings
There is no technical reason to suppress this warning.

## Example of a violation

### Description
The type `C` implements the interface `IEquatable<T>` but does not override `Object.Equals`.

### Code

    using System;

    public class C: IEquatable<C>
    {
        private readonly int _c;

        public C(int c)
        {
            _c = c;
        }

        public bool Equals(C other)
        {
            if (other == null)
            {
                return false;
            }

            return _c == otherC._c;
        }
    }

## Example of how to fix

### Description
The type `C` now implements `Object.Equals(object)`, which delegates to `IEquatable<T>.Equals(T)`.

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

        public bool Equals(C other)
        {
            if (otherC == null)
            {
                return false;
            }

            return _c == otherC._c;
        }
    }

## Related rules

[CA1066: Implement IEquatable<T\> when overriding Object.Equals>](https://github.com/dotnet/roslyn-analyzers/blob/master/docs/reference/CA1066_ImplementIEquatableOfTWhenOverridingObjectEquals.md)