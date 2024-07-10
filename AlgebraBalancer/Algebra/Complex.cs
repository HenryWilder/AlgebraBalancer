using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public class Complex : IAlgebraicExpression
{
    public bool IsInoperable => false;

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

    public override string ToString()
    {
        if (IsReal()) return real.ToString();
        if (IsImaginary()) return imag.ToString();
        else return $"{real}{imag:+#;-#;+0}";
    }

    public string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public bool IsReal() => imag.coef == 0;
    public bool IsImaginary() => real == 0;
    public IAlgebraicNotation Simplified()
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

    public static Complex operator -(Complex rhs) => new(-rhs.real, -rhs.imag);
    public Complex Conjugate() => new(real, -imag);
}
