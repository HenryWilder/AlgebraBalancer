using AlgebraBalancer.Notation;

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
