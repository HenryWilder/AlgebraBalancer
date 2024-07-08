using System;
using System.Linq;
using System.Collections.Generic;

using AlgebraBalancer.Algebra;

namespace AlgebraBalancer.Notation;

public interface IAlgebraicNotation
{
    public string AsEquality(string lhs);
    /// <summary>Does not simplify</summary>
    public IAlgebraicNotation Add(IAlgebraicNotation rhs);
    /// <summary>Does not simplify</summary>
    public IAlgebraicNotation Sub(IAlgebraicNotation rhs);
    /// <summary>Does not simplify</summary>
    public IAlgebraicNotation Mul(IAlgebraicNotation rhs);
    /// <summary>Does not simplify</summary>
    public IAlgebraicNotation Div(IAlgebraicNotation rhs);
    /// <summary>Does not simplify</summary>
    public IAlgebraicNotation Pow(int exponent);
}

public struct MultipleSolutions(params IAlgebraicNotation[] solutions) : IAlgebraicNotation
{
    public IAlgebraicNotation[] solutions = solutions;

    public readonly string AsEquality(string lhs) =>
        lhs + " = " + string.Join<IAlgebraicNotation>(", ", solutions);

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) =>
        new MultipleSolutions(solutions.Select(x => x.Add(rhs)).ToArray());
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) =>
        new MultipleSolutions(solutions.Select(x => x.Sub(rhs)).ToArray());
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) =>
        new MultipleSolutions(solutions.Select(x => x.Mul(rhs)).ToArray());
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) =>
        new MultipleSolutions(solutions.Select(x => x.Div(rhs)).ToArray());
    public readonly IAlgebraicNotation Pow(int exponent) =>
        new MultipleSolutions(solutions.Select(x => x.Pow(exponent)).ToArray());
}

public interface IAlgebraicExpression : IAlgebraicNotation
{
    public IAlgebraicNotation Simplified();
}

public interface IAlgebraicAtomic : IAlgebraicNotation { }

public struct Number(int value) : IAlgebraicAtomic
{
    public int value = value;

    public static implicit operator int(Number num) => num.value;
    public static implicit operator Number(int value) => new(value);
    public override readonly string ToString() => $"{value}";
    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => (Number)(this + num),
            _ => rhs.Add(this),
        };
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => (Number)(this - num),
            _ => rhs.Mul((Number)(-1)).Add(this),
        };
    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => (Number)(this * num),
            _ => rhs.Mul(this),
        };
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => (Number)(this / num),
            _ => rhs.Mul(new Fraction(1, this)),
        };
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
        return ExactMath.Power(value, exponent);
    }
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

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this + num,
            Imaginary imag => this + imag,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this + -num,
            Imaginary imag => this + new Imaginary(-imag.coef),
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this * num,
            Imaginary imag => this * -imag.coef,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        throw new NotImplementedException();
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
}

// todo
public struct Variable : IAlgebraicAtomic
{
    public string name;

    public override readonly string ToString() => name;
    public readonly string AsEquality(string lhs) => $"{lhs} = {name}";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Pow(int exponent) => throw new NotImplementedException();
}

public struct Undefined : IAlgebraicAtomic
{
    public override readonly string ToString() => "∄";
    public readonly string AsEquality(string lhs) => $"∄{lhs}";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Pow(int exponent) => this;
}

public struct Huge : IAlgebraicAtomic
{
    public override readonly string ToString() => "𝑥 : |𝑥|≥2³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≥ 2³²";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) => rhs is Huge ? this : Bald.NOT_ENOUGH_INFO;
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Pow(int exponent) => this;
}

public struct Tiny : IAlgebraicAtomic
{
    public override readonly string ToString() => "𝑥 : |𝑥|≤2⁻³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≤ 2⁻³²";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => Bald.NOT_ENOUGH_INFO;
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => Bald.NOT_ENOUGH_INFO;
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) => rhs is Fraction or Number or Tiny ? this : Bald.NOT_ENOUGH_INFO;
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) => rhs is Number or Huge ? this : Bald.NOT_ENOUGH_INFO;
    public readonly IAlgebraicNotation Pow(int exponent) => exponent > 0 ? this : Bald.NOT_ENOUGH_INFO;
}

public struct NotEnoughInfo : IAlgebraicAtomic
{
    public override readonly string ToString() => "?";
    public readonly string AsEquality(string lhs) => $"{lhs} = ?";

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs) => this;
    public readonly IAlgebraicNotation Pow(int exponent) => this;
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
