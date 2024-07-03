using System.Numerics;

namespace AlgebraBalancer.Notation;

public interface IAlgebraicNotation
{
    public abstract string ToString();
    public abstract string AsEquality(string lhs);
}

public interface IAlgebraicExpression : IAlgebraicNotation
{
    public abstract IAlgebraicNotation Simplified();
}

public interface IAlgebraicAtomic : IAlgebraicNotation { }

public struct Number(int value) : IAlgebraicAtomic
{
    public int value = value;

    public static implicit operator int(Number num) => num.value;
    public static implicit operator Number(int value) => new(value);
    public override readonly string ToString() => $"{value}";
    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";
}

public struct Imaginary(int coef) : IAlgebraicAtomic
{
    public int coef = coef;

    public static implicit operator Complex(Imaginary imag) => new(0, imag.coef);
    public override readonly string ToString() => (coef == 1 ? "" : coef.ToString()) + "𝑖";
    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

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
}

// todo
public struct Variable : IAlgebraicAtomic
{
    public string name;

    public override readonly string ToString() => name;
    public readonly string AsEquality(string lhs) => $"{lhs} = {name}";
}

public struct Undefined : IAlgebraicAtomic
{
    public override readonly string ToString() => "∄";
    public readonly string AsEquality(string lhs) => $"∄{lhs}";
}

public struct Huge : IAlgebraicAtomic
{
    public override readonly string ToString() => "𝑥 : |𝑥|≥2³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≥ 2³²";
}

public struct Tiny : IAlgebraicAtomic
{
    public override readonly string ToString() => "𝑥 : |𝑥|≤2⁻³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≤ 2⁻³²";
}

public struct NotEnoughInfo : IAlgebraicAtomic
{
    public override readonly string ToString() => "?";
    public readonly string AsEquality(string lhs) => $"{lhs} = ?";
}

/// <summary>
/// "No hair"
/// Not really constants, but instances are indistinguishable from eachother and so can be treated like constants.
/// </summary>
public static class Bald
{
    public static readonly Undefined UNDEFINED = new();
    public static readonly Huge HUGE = new();
    public static readonly Tiny TINY = new();
    public static readonly NotEnoughInfo NOT_ENOUGH_INFO = new();
}
