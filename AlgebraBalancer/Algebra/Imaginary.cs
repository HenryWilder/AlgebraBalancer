using System;
using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public struct Imaginary(int coef) : IAlgebraicAtomic
{
    public int coef = coef;

    public readonly bool IsInoperable => false;

    public static implicit operator Complex(Imaginary imag) => new(0, imag.coef);
    public override readonly string ToString() => $"{coef}*i";
    public readonly string AsEquality(string lhs) => $"{lhs}={ToString()}";

    public static Imaginary operator +(Imaginary a, Imaginary b)
    {
        return new Imaginary(a.coef + b.coef);
    }

    public static Complex operator +(Number a, Imaginary b)
    {
        return new Complex(a, b.coef);
    }

    public static Complex operator +(Imaginary a, Number b)
    {
        return b + a;
    }

    public static Number operator *(Imaginary a, Imaginary b)
    {
        return a.coef * -b.coef;
    }

    public static Imaginary operator *(Imaginary a, Number b)
    {
        return new Imaginary(a.coef * b);
    }

    public static Imaginary operator *(Number a, Imaginary b)
    {
        return b * a;
    }

    public readonly IAlgebraicNotation Pow(int exponent)
    {
        if (coef == 0) return (Number)0;
        else if (exponent == 0) return (Number)1;

        return (ExactMath.Power(coef, exponent) is Number pow)
            ? ((exponent - 1) % 4) switch
            {
                0 => new Imaginary(pow),
                1 => new Number(pow),
                2 => new Imaginary(-pow),
                3 => new Number(-pow),
                _ => throw new Exception(),
            }
            : Bald.NOT_ENOUGH_INFO;
    }

    public static Imaginary operator -(Imaginary rhs) => new(-rhs.coef);

    public readonly Number Squared() => new(coef * -coef);
}
