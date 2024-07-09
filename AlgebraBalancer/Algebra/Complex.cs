using System;
using AlgebraBalancer.Notation;

using static AlgebraBalancer.Notation.IAlgebraicNotation;

namespace AlgebraBalancer.Algebra;
public struct Complex : IAlgebraicExpression
{
    public Complex(int real, int imag)
    {
        this.real = real;
        this.imag = new(imag);
    }
    public Complex(Number real)
    {
        this.real = real;
        imag = new(0);
    }
    public Complex(Imaginary imag)
    {
        real = 0;
        this.imag = imag;
    }
    public Complex(Number real, Imaginary imag)
    {
        this.real = real;
        this.imag = imag;
    }

    public Number real;
    public Imaginary imag;

    public readonly NotationKind Kind => NotationKind.Complex;

    public override readonly string ToString()
    {
        if (IsReal()) return real.ToString();
        if (IsImaginary()) return imag.ToString();
        else return $"{real}{imag:+#;-#;+0}";
    }

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly bool IsReal() => imag.coef == 0;
    public readonly bool IsImaginary() => real == 0;
    public readonly IAlgebraicNotation Simplified()
    {
        if (IsReal()) return real;
        if (IsImaginary()) return imag;
        else return this;
    }

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this + num,
            Complex cmplx => this + cmplx,
            Imaginary imag => this + imag,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {
        return Add(rhs.Neg());
    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this * num,
            Complex cmplx => this * cmplx,
            Imaginary imag => this * imag,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => this * num,
            Complex cmplx => this * cmplx,
            Imaginary imag => this * imag,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Neg()
    {
        return this * -1;
    }
    public readonly IAlgebraicNotation Reciprocal()
    {
        throw new NotImplementedException();
    }

    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.real + b.real, a.imag + b.imag);
    }

    public static Complex operator +(Complex a, Number b)
    {
        return new Complex(a.real + b, a.imag);
    }

    public static Complex operator +(Number a, Complex b)
    {
        return b + a;
    }

    public static IAlgebraicNotation operator *(Complex a, Complex b)
    {
        // FOIL
        Complex x = new(
            a.real * b.real + a.imag * b.imag,
            a.real * b.imag + a.imag * b.real
        );
        return x.Simplified();
    }

    public static IAlgebraicNotation operator *(Complex a, Number b)
    {
        return a * new Complex(b);
    }

    public static IAlgebraicNotation operator *(Number a, Complex b)
    {
        return b * a;
    }
}
