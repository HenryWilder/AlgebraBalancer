using System;
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
        //tex:$$\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{d}$$

        // Simplify terms
        //tex:$$\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}}{d}$$

        var numeratorTerms = this.numeratorTerms
            .Select(term =>
            {
                var primeFactors = ExactMath.PrimeFactors(term.radicand);

                (int prime, int exponent)[] simplifyableFactors = primeFactors
                    .Where(x => x.exponent >= 2)
                    .Select(x => (x.prime, x.exponent - x.exponent % 2))
                    .ToArray();

                int newRadicand = term.radicand / simplifyableFactors
                    .Select(x => ExactMath.UncheckedUIntPower(x.prime, (uint)x.exponent))
                    .Aggregate((a, b) => a * b);

                int newCoefficient = term.coefficient * simplifyableFactors
                    .Select(x => ExactMath.UncheckedUIntPower(x.prime, (uint)x.exponent / 2))
                    .Aggregate((a, b) => a * b);

                return new Radical(newCoefficient, newRadicand);
            });

        // Factor out
        //tex:$$\frac{c_0(c_1'\sqrt{r_1}+c_2'\sqrt{r_2}+\dots+c_n'\sqrt{r_n})}{d}$$

        (int gcf, int[] associatedFactors) =
            ExactMath.CommonFactors(
                numeratorTerms
                    .Select(x => x.coefficient)
                    .ToArray()
            )
            .Last();

        numeratorTerms = numeratorTerms
            .Zip(
                associatedFactors,
                (term, factor) =>
                    new Radical(
                        factor,
                        term.radicand
                    )
            );

        //tex:$$\frac{c_0'}{d'}(c_1'\sqrt{r_1}+c_2'\sqrt{r_2}+\dots+c_n'\sqrt{r_n})$$
        var coef = new Fraction(gcf, denominator).Simplified();
        int numeratorCoef;
        int denominatorCoef;
        if (coef is Fraction frac)
        {
            numeratorCoef = frac.numerator;
            denominatorCoef = frac.denominator;
        }
        else if (coef is Number num)
        {
            numeratorCoef = num;
            denominatorCoef = 1;
        }
        else if (coef is Huge or Tiny or Undefined)
        {
            return coef;
        }
        else
        {
            throw new NotImplementedException("I'm not expecting Fraction to return these");
        }

        // Combine like terms
        //tex:$$\frac{c_0'}{d'}
        //\left(
        //  \left(c_1'' \gets \sum_{i=1}^{n} c_i' \mid r_i = r_1\right)\sqrt{!\exists r_1 \in r_{1 \dots n}}
        //  +
        //  \left(c_2'' \gets \sum_{i=1}^{n} c_i' \mid r_i = r_2\right)\sqrt{!\exists r_2 \in r_{1 \dots n}}
        //  +
        //  \cdots
        //  +
        //  \left(c_n'' \gets \sum_{i=1}^{n} c_i' \mid r_i = r_n\right)\sqrt{!\exists r_n \in r_{1 \dots n}}
        //\right)$$

        //tex:$$\frac{c_0'}{d'}(c_1''\sqrt{r_1} + c_2''\sqrt{r_2} + \dots + c_n''\sqrt{r_n})$$

        var uniqueRadicands = numeratorTerms
            .Select(x => x.radicand)
            .Distinct();

        var combined = uniqueRadicands
            .Select(r =>
                new Radical(
                    numeratorTerms
                        .Where(x => x.radicand == r)
                        .Sum(x => x.coefficient),
                    r
                )
            )
            .Where(x =>
                x.coefficient != 0 &&
                x.radicand != 0
            );

        var alg = new Algebraic(
            combined.Select(x => new Radical(x.coefficient * numeratorCoef, x.radicand)).ToArray(),
            denominatorCoef
        );

        //tex:$$\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}}{0} = \frac{1}{0}$$
        if (alg.denominator == 0)
        {
            return Bald.UNDEFINED;
        }
        //tex:$$\frac{c\sqrt{r}}{d}$$
        else if (alg.numeratorTerms.Length == 1)
        {
            var numerator = alg.numeratorTerms[0];
            //tex:$$\frac{c\sqrt{r}}{1}$$
            if (alg.denominator is 1 or -1)
            {
                //tex:$$\frac{c\sqrt{0}}{1} = 0$$
                if (numerator.radicand == 0)
                {
                    return new Number(0);
                }
                //tex:$$\frac{c\sqrt{1}}{1} = c$$
                else if (numerator.radicand == 1)
                {
                    return new Number(numerator.coefficient);
                }
                //tex:$$\frac{c\sqrt{-1}}{1} = ci$$
                else if (numerator.radicand == -1)
                {
                    return new Imaginary(numerator.coefficient);
                }
                //tex:$$\frac{c\sqrt{r}}{1} = c\sqrt{r}$$
                else
                {
                    return numerator;
                }
            }
            //tex:$$\frac{c\sqrt{1}}{d} = \frac{c}{d}$$
            else if (numerator.radicand == 1)
            {
                return new Fraction(numerator.coefficient, denominator);
            }
            //tex:$$\frac{c\sqrt{r}}{d}$$
            else
            {
                return new RadicalFraction(numerator, denominator);
            }
        }
        //tex:$$\frac{c_1\sqrt{\pm 1}+c_2\sqrt{\mp 1}}{1} = \begin{gathered}c_1+c_2i\\\text{or}\\c_2+c_1i\end{gathered} = a+bi$$
        else if (
            alg.denominator == 1 &&
            alg.numeratorTerms.Length == 2 &&
            alg.numeratorTerms[0].radicand is 1 or -1 &&
            alg.numeratorTerms[1].radicand is 1 or -1 &&
            alg.numeratorTerms[0].radicand < 0 != alg.numeratorTerms[1].radicand < 0
        )
        {
            var (real, imag) = alg.numeratorTerms[0].radicand < 0
                ? (alg.numeratorTerms[1].coefficient, alg.numeratorTerms[0].coefficient)
                : (alg.numeratorTerms[0].coefficient, alg.numeratorTerms[1].coefficient);

            return new Complex(real, imag);
        }

        return alg;
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
    //-\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}\\
    // =\\
    //\frac{-\sqrt{a_1}-\sqrt{a_2}-\dots-\sqrt{a_n}}{b}
    //\end{gathered}$$
    public static Algebraic operator -(Algebraic rhs)
    {
        var negatedNumerator = rhs.numeratorTerms.Select(x => new Radical(-x.coefficient, x.radicand)).ToArray();
        return new(negatedNumerator, rhs.denominator);
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
        //tex:$$\begin{gathered}
        //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
        //\div
        //\frac{\sqrt{c}}{d}\\
        // =\\
        //\frac{d\sqrt{a_1}+d\sqrt{a_2}+\dots+d\sqrt{a_n}}{b\sqrt{c}}\\
        // =\\
        //\frac{d\sqrt{a_1}\sqrt{c}+d\sqrt{a_2}\sqrt{c}+\dots+d\sqrt{a_n}\sqrt{c}}{b\sqrt{c}\sqrt{c}}\\
        // =\\
        //\frac{d\sqrt{ca_1}+d\sqrt{ca_2}+\dots+d\sqrt{ca_n}}{bc}
        //\end{gathered}$$
        if (rhs.numeratorTerms.Length == 1)
        {
            var rhsNumerator = rhs.numeratorTerms[0];
            var newNumeratorTerms = lhs.numeratorTerms.Select(
                x => new Radical(x.coefficient * rhs.denominator, x.radicand * rhsNumerator.radicand)
            );
            return new(newNumeratorTerms.ToArray(), lhs.denominator * rhsNumerator.radicand);
        }
        else
        {
            throw new NotImplementedException();
        }
    }


    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Neg()
    {
        throw new NotImplementedException();
    }
    public readonly IAlgebraicNotation Reciprocal()
    {
        throw new NotImplementedException();
    }
}
