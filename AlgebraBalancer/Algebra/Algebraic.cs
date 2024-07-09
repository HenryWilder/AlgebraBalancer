using System;
using System.Collections.Generic;
using System.Linq;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.IAlgebraicNotation;

namespace AlgebraBalancer.Algebra;
internal struct Algebraic : IAlgebraicExpression
{
    public readonly NotationKind Kind => NotationKind.Algebraic;

    public Algebraic() =>
        numeratorTerms = [];

    public Algebraic(int num) =>
        numeratorTerms = [new Radical(num, 1)];

    public Algebraic(Number num) =>
        numeratorTerms = [new Radical(num, 1)];

    public Algebraic(Fraction frac) =>
        (numeratorTerms, denominator) = ([new Radical(frac.numerator, 1)], frac.denominator);

    public Algebraic(Radical rad) =>
        numeratorTerms = [rad];

    public Algebraic(RadicalFraction radFrac) =>
        (numeratorTerms, denominator) = ([radFrac.numerator], radFrac.denominator);

    public Algebraic(Imaginary imag) =>
        numeratorTerms = [new Radical(imag.coef, -1)];

    public Algebraic(Complex cmplx) =>
        numeratorTerms = [new Radical(cmplx.real, 1), new Radical(cmplx.imag.coef, -1)];

    public Algebraic(Undefined _) =>
        (numeratorTerms, denominator) = ([new Radical(0, 1)], 0);

    public Algebraic(Radical[] numeratorTerms, int denominator) =>
        (this.numeratorTerms, this.denominator) = (numeratorTerms, denominator);

    public Radical[] numeratorTerms;
    public int denominator = 1;

    public readonly IAlgebraicNotation Simplified()
    {

    }

    public override readonly string ToString() => $"({string.Join("+", numeratorTerms)})/{denominator}";

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    //tex:$$\begin{gathered}
    //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
    // +
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_m}}{d}\\
    // =\\
    //\frac{
    //  d\sqrt{a_1} + d\sqrt{a_2} + \dots + d\sqrt{a_n}
    //  +
    //  b\sqrt{c_1} + b\sqrt{c_2} + \dots + b\sqrt{c_m}
    //}{bd}
    //\end{gathered}$$
    public static Algebraic operator +(Algebraic lhs, Algebraic rhs)
    {
        int newDenominator = lhs.denominator * rhs.denominator;
        var lhsNumerator = lhs.numeratorTerms.Select(x => new Radical(x.coefficient * rhs.denominator, x.radicand));
        var rhsNumerator = rhs.numeratorTerms.Select(x => new Radical(x.coefficient * lhs.denominator, x.radicand));
        return new(lhsNumerator.Concat(rhsNumerator).ToArray(), newDenominator);
    }

    //tex:$$\begin{gathered}
    //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
    // -
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_m}}{d}\\
    // =\\
    //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
    // +
    //\frac{-\sqrt{c_1}-\sqrt{c_2}-\dots-\sqrt{c_m}}{d}
    //\end{gathered}$$
    public static Algebraic operator -(Algebraic rhs)
    {
        var negatedNumerator = rhs.numeratorTerms.Select(x => new Radical(-x.coefficient, x.radicand)).ToArray();
        return new(negatedNumerator, rhs.denominator);
    }

    //tex:$$\begin{gathered}
    //-\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}\\
    // =\\
    //\frac{-\sqrt{a_1}-\sqrt{a_2}-\dots-\sqrt{a_n}}{b}
    //\end{gathered}$$
    public static Algebraic operator -(Algebraic lhs, Algebraic rhs)
    {
        return lhs + -rhs;
    }

    //tex:$$\begin{gathered}
    //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
    //\times
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_n}}{d}\\
    // =\\
    //\frac{
    //  \sqrt{a_1c_1}+\sqrt{a_1c_2}+\dots+\sqrt{a_1c_m}
    //  +
    //  \sqrt{a_2c_1}+\sqrt{a_2c_2}+\dots+\sqrt{a_2c_m}
    //  +
    //  \dots
    //  +
    //  \sqrt{a_nc_1}+\sqrt{a_nc_2}+\dots+\sqrt{a_nc_m}
    //}{bd}
    //\end{gathered}$$
    public static Algebraic operator *(Algebraic lhs, Algebraic rhs)
    {
        int newDenominator = lhs.denominator * rhs.denominator;
        var newNumerator = lhs.numeratorTerms.SelectMany(
            a => rhs.numeratorTerms.Select(
                b => new Radical(a.coefficient * b.coefficient, a.radicand * b.radicand)
            )
        );
        return new(newNumerator.ToArray(), newDenominator);
    }

    //tex:$$\begin{gathered}
    //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
    //\div
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_n}}{d}\\
    // =\\
    //\frac{
    //  d\sqrt{a_1}+d\sqrt{a_2}+\dots+d\sqrt{a_n}
    //}{
    //  b\sqrt{c_1}+b\sqrt{c_2}+\dots+b\sqrt{c_n}
    //}\\
    // =\\
    // ?
    //\end{gathered}$$
    public static Algebraic operator /(Algebraic lhs, Algebraic rhs)
    {
        var newNumerator = lhs.numeratorTerms.SelectMany(
            x => new Radical(x.coefficient * rhs.denominator, x.radicand)
        );
        return new(newNumerator.ToArray(), lhs.denominator);
    }


    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {

    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
    
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
    
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
    
    }
    public readonly IAlgebraicNotation Neg()
    {
    
    }
    public readonly IAlgebraicNotation Reciprocal()
    {
    
    }
}
