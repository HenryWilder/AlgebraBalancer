using System;
using System.Linq;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public class Algebraic : IAlgebraicExpression
{
    public bool IsInoperable => false;

    public Algebraic() =>
        numerator = new();

    public Algebraic(int num) =>
        numerator = new(new Radical(num, 1));

    public static implicit operator Algebraic(int num) => new(num);

    public Algebraic(Number num) =>
        numerator = new(new Radical(num, 1));

    public static implicit operator Algebraic(Number num) => new(num);

    public Algebraic(Fraction frac) =>
        (numerator, denominator) = (
            new(new Radical((frac.denominator < 0 ? -1 : 1) * frac.numerator, 1)),
            Math.Abs(frac.denominator)
        );

    public static implicit operator Algebraic(Fraction frac) => new(frac);

    public Algebraic(Radical rad) =>
        numerator = new(rad);

    public static implicit operator Algebraic(Radical rad) => new(rad);

    public Algebraic(RadicalFraction radFrac) =>
        (numerator, denominator) = (
            new(new Radical((radFrac.denominator < 0 ? -1 : 1) * radFrac.numerator.coefficient, radFrac.numerator.radicand)),
            Math.Abs(radFrac.denominator)
        );

    public static implicit operator Algebraic(RadicalFraction radFrac) => new(radFrac);

    public Algebraic(Imaginary imag) =>
        numerator = new(new Radical(imag.coef, -1));

    public static implicit operator Algebraic(Imaginary imag) => new(imag);

    public Algebraic(Complex cmplx) =>
        numerator = new SumOfRadicals(new Radical(cmplx.real, 1), new Radical(cmplx.imag.coef, -1));

    public static implicit operator Algebraic(Complex cmplx) => new(cmplx);

    public Algebraic(Undefined _) =>
        (numerator, denominator) = (new SumOfRadicals(new Radical(1)), 0);

    public Algebraic(SumOfRadicals numerator, int denominator) =>
        (this.numerator, this.denominator) = (
            denominator < 0 ? -numerator : numerator,
            Math.Abs(denominator)
        );

    public Algebraic(SumOfRadicals numerator) =>
        this.numerator = numerator;

    public static implicit operator Algebraic(SumOfRadicals numerator) =>
        new(numerator);

    // Something seems to be wrong with this. It's creating nulls numerators.
    public Algebraic(Radical[] numeratorTerms, int denominator) =>
        new Algebraic(new SumOfRadicals(numeratorTerms), denominator);

    public SumOfRadicals numerator;
    public int denominator = 1;

    public Algebraic SimplifiedToAlgebraic()
    {
        // Because we are only simplifying, there is no case in which
        // having a 0-denominator at the start isn't undefined.
        if (denominator == 0)
        {
            return new Algebraic(Bald.UNDEFINED);
        }
        else if (denominator < 0)
        {
            throw new Exception("That wasn't supposed to happen");
        }

        //tex:$$\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{d}$$

        //tex:
        //Simplify terms
        //$$\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}}{d}$$

        var simplifiedNumerator = numerator.TermsSimplified();

        //tex:
        //Factor out
        //$$\frac{c_0(c_1'\sqrt{r_1}+c_2'\sqrt{r_2}+\dots+c_n'\sqrt{r_n})}{d}$$

        var (numeratorGCF, factoredNumerator) = simplifiedNumerator.Factored();

        //tex:$$
        //\frac{c_0}{d} = \frac{m\frac{c_0}{m}}{m\frac{d}{m}} \mid m \in \mathbb{Z} \qquad (m \text{ may be } 1)
        //$$
        int coefGCF = ExactMath.GCF(numeratorGCF, denominator);

        //tex:$$c_0' := \frac{c_0}{m}$$
        int numeratorCoef = numeratorGCF / coefGCF;

        //tex:$$d' := \frac{d}{m}$$
        int denominatorCoef = denominator / coefGCF;

        //tex:
        //Combine like terms
        //$$\frac{c_0'}{d'}
        //\left(
        //  \left(c_1'' := \sum_{i=1}^{n} c_i' \mid r_i = r_1\right)\sqrt{!\exists r_1 \in r_{1 \dots n}}
        //  +
        //  \left(c_2'' := \sum_{i=1}^{n} c_i' \mid r_i = r_2\right)\sqrt{!\exists r_2 \in r_{1 \dots n}}
        //  +
        //  \cdots
        //  +
        //  \left(c_m'' := \sum_{i=1}^{n} c_i' \mid r_i = r_n\right)\sqrt{!\exists r_n \in r_{1 \dots n}}
        //\right)$$
        //$$\frac{c_0'}{d'}(c_1''\sqrt{r_1} + c_2''\sqrt{r_2} + \dots + c_m''\sqrt{r_n})$$

        var combinedNumerator = factoredNumerator.LikeTermsCombined();

        //tex:
        //Every term in the numerator was equal to 0 and got excluded;
        //this is also known as "being equal to zero".
        //$$C = \{\} \implies \frac{\displaystyle \sum_{x \in C} = 0}{d} = 0 \iff d \ne 0$$
        if (combinedNumerator.terms.Length == 0) return (Number)0;

        //tex:$$
        //\begin{cases}
        //0 & \text{if } C \equiv \varnothing\\
        //\dfrac{c_0'c_1''\sqrt{r_1} + c_0'c_2''\sqrt{r_2} + \dots + c_0'c_n''\sqrt{r_n}}{d'}  & \text{if } C \not\equiv \varnothing\\
        //\end{cases}$$
        return new Algebraic(combinedNumerator * numeratorCoef, denominatorCoef);
    }

    public IAlgebraicNotation Simplified()
    {
        // Because we are only simplifying, there is no case in which
        // having a 0-denominator at the start isn't undefined.
        if (denominator == 0)
        {
            return Bald.UNDEFINED;
        }

        var alg = SimplifiedToAlgebraic();

        //tex:$$\frac{c\sqrt{r}}{d}$$
        if (alg.numerator.TryMonomial(out var numeratorTerm))
        {
            //tex:$$\frac{c\sqrt{r}}{1}$$
            if (alg.denominator == 1)
            {
                //tex:$$\frac{c\sqrt{0}}{1} = 0$$
                if (numeratorTerm.radicand == 0)
                {
                    return new Number(0);
                }
                //tex:$$\frac{c\sqrt{1}}{1} = c$$
                else if (numeratorTerm.radicand == 1)
                {
                    return new Number(numeratorTerm.coefficient);
                }
                //tex:$$\frac{c\sqrt{-1}}{1} = ci$$
                else if (numeratorTerm.radicand == -1)
                {
                    return new Imaginary(numeratorTerm.coefficient);
                }
                //tex:$$\frac{c\sqrt{r}}{1} = c\sqrt{r}$$
                else
                {
                    return numeratorTerm.Simplified();
                }
            }
            //tex:$$\frac{c\sqrt{1}}{d} = \frac{c}{d}$$
            else if (numeratorTerm.radicand == 1)
            {
                return new Fraction(numeratorTerm.coefficient, alg.denominator).Simplified();
            }
            //tex:$$\frac{c\sqrt{r}}{d}$$
            else
            {
                return new RadicalFraction(numeratorTerm, alg.denominator).Simplified();
            }
        }
        //tex:$$\frac{c_1\sqrt{\pm 1}+c_2\sqrt{\mp 1}}{1} = \begin{gathered}c_1+c_2i\\\text{or}\\c_2+c_1i\end{gathered} = a+bi$$
        else if (
            alg.denominator == 1 &&
            alg.numerator.TryBinomial(out var term1, out var term2) &&
            (
                term1.radicand == 1 && term2.radicand == -1 ||
                term2.radicand == 1 && term1.radicand == -1
            )
        )
        {
            var (real, imag) = term1.radicand == -1
                ? (term2.coefficient, term1.coefficient)
                : (term1.coefficient, term2.coefficient);

            return new Complex(real, imag);
        }

        return alg;
    }

    public override string ToString()
    {
        string numeratorStr = string.Join<Radical>("+", numerator.terms).Replace("+-", "-");
        return denominator == 1
            ? numeratorStr
            : $"({numeratorStr})/{denominator}";
    }

    public string AsEquality(string lhs) => $"{lhs} = {ToString()}";

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
        var lhsNumerator = lhs.numerator * rhs.denominator;
        var rhsNumerator = rhs.numerator * lhs.denominator;
        return new(lhsNumerator + rhsNumerator, newDenominator);
    }

    //tex:$$\begin{gathered}
    //-\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}\\
    // =\\
    //\frac{-\sqrt{a_1}-\sqrt{a_2}-\dots-\sqrt{a_n}}{b}
    //\end{gathered}$$
    public static Algebraic operator -(Algebraic rhs)
    {
        return new(-rhs.numerator, rhs.denominator);
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
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_m}}{d}\\
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
        return new(lhs.numerator * rhs.numerator, lhs.denominator * rhs.denominator);
    }

    public static Algebraic operator ^(Algebraic lhs, Algebraic rhs)
    {
        var newRhs = rhs.Simplified();
        if (newRhs is Number rhsInt)
        {
            if (rhsInt == 0) return new(1);
            var result = lhs;
            for (int i = 1; i < Math.Abs(rhsInt); ++i)
            {
                result *= lhs;
            }
            return (rhsInt < 0) ? 1 / result : result;
        }
        //else if (newRhs is Fraction rhsFraction)
        //{
        //    // Not sure how to generalize
        //}
        else
        {
            throw new NotImplementedException($"\"{lhs}^{{{rhs}}}\" not supported");
        }
    }

    public static Algebraic operator /(Algebraic lhs, Algebraic rhs)
    {
        //tex:$$\begin{gathered}
        //\frac{\sqrt{a_1} + \sqrt{a_2} + \dots + \sqrt{a_n}}{b}
        //\div
        //\frac{\sqrt{c_1} + \sqrt{c_2} + \dots + \sqrt{c_m}}{d}\\
        // =\\
        //\frac{d\sqrt{a_1} + d\sqrt{a_2} + \dots + d\sqrt{a_n}}{b\sqrt{c_1} + b\sqrt{c_2} + \dots + b\sqrt{c_m}}
        //\end{gathered}$$
        var numerator   = (lhs.numerator * rhs.denominator).TermsSimplified().LikeTermsCombined();
        var denominator = (rhs.numerator * lhs.denominator).TermsSimplified().LikeTermsCombined();

        // Rationalize denominator
        while (denominator.terms.Any(term => !term.IsInteger()))
        {
            //tex:$$\overline{\sqrt{c_1} + \sqrt{c_2} + \dots + \sqrt{c_m}} = \sqrt{c_1} + \sqrt{c_2} + \dots - \sqrt{c_m}$$
            var conj = denominator.Conjugate();

            //tex:$$\begin{gathered}
            //\frac{
            //  \left(d\sqrt{a_1} + d\sqrt{a_2} + \dots + d\sqrt{a_n}\right)\!\left(b\sqrt{c_1} + b\sqrt{c_2} + \dots - b\sqrt{c_m}\right)
            //}{
            //  \left(b\sqrt{c_1} + b\sqrt{c_2} + \dots + b\sqrt{c_m}\right)\!\left(b\sqrt{c_1} + b\sqrt{c_2} + \dots - b\sqrt{c_m}\right)
            //}\\
            // =\\
            //\frac{
            //  bd\sqrt{a_1c_1} + bd\sqrt{a_1c_2} + \dots - bd\sqrt{a_1c_m} +
            //  bd\sqrt{a_2c_1} + bd\sqrt{a_2c_2} + \dots - bd\sqrt{a_2c_m} +
            //  \dots +
            //  bd\sqrt{a_nc_1} + bd\sqrt{a_nc_2} + \dots - bd\sqrt{a_nc_m}
            //}{
            //  b^2c_1 + b^2\sqrt{c_1c_2} + \dots - b^2\sqrt{c_1c_m} +
            //  b^2\sqrt{c_2c_1} + b^2c_2 + \dots - b^2\sqrt{c_2c_m} +
            //  \dots +
            //  b^2\sqrt{c_mc_1} + b^2\sqrt{c_mc_2} + \dots - b^2c_m
            //  \quad = \quad
            //  b^2c_1 + b^2c_2 + \dots - b^2c_m
            //}\\
            //\vdots
            //\end{gathered}$$
            numerator *= conj;
            denominator *= conj;
            denominator = denominator.TermsSimplified().LikeTermsCombined();
        }

        // If all terms are rationalized and like terms combined, there will only be one term.
        if (!(denominator.TryMonomial(out var term) && term.IsInteger()))
        {
            throw new Exception("Expected a rationalized denominator to be a monomial");
        }

        return new Algebraic(numerator, term.coefficient);
    }

    public Algebraic Squared()
    {
        return new Algebraic(numerator.Squared(), denominator * denominator);
    }
}
