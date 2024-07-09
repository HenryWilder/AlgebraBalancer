using System;
using System.Linq;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.IAlgebraicNotation;

namespace AlgebraBalancer.Algebra;
public struct Algebraic : IAlgebraicExpression
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

        //tex:
        //Simplify terms
        //$$\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}}{d}$$

        var numeratorTerms = this.numeratorTerms
            .Select(term =>
            {
                //tex:
                //PrimeFactors($x$) returns the prime factors of $x$ in the form of
                //$$
                //x = p_1^{e_1} \times p_2^{e_2} \times \dots \times p_n^{e_n}
                //\implies
                //[
                //  (\mathtt{prime}_1, \mathtt{exponent}_1),
                //  (\mathtt{prime}_2, \mathtt{exponent}_2),
                //  \dots,
                //  (\mathtt{prime}_n, \mathtt{exponent}_n)
                //]
                //$$
                //In ascending order of $\mathtt{prime}$.
                //If $x$ is negative, the first element will always be $(-1, 1)$.
                //If $x$ is prime, there will be exactly one element: $(x, 1)$.
                //There is no case in which PrimeFactors($x$) should ever return $\{\}$.
                var primeFactors = ExactMath.PrimeFactors(term.radicand);

                //tex: Let $F_p = \mathtt{primeFactors}$

                //tex:
                //Let $F_s = \mathtt{simplifyableFactors}$ be $$
                //\left\{
                //  \left(p,2\left\lfloor{\frac{e}{2}}\right\rfloor\right)
                //  \mid
                //  (p,e) \in F_p,
                //  e \ge 2
                //\right\}
                //$$
                (int prime, int exponent)[] simplifyableFactors = primeFactors
                    .Where(x => x.exponent >= 2)
                    .Select(x => (x.prime, x.exponent - x.exponent % 2))
                    .ToArray();

                //tex:$$
                //\nexists e \in F_p \mid e \ge 2
                //\implies
                //\text{radical term is in simplest form}
                //\iff
                //\text{radical index = 2}
                //$$
                if (simplifyableFactors.Length == 0) return term;

                //tex:$$c' := c\prod_{p^e \in F_s}\sqrt{p^e}$$
                int newCoefficient = term.coefficient * simplifyableFactors
                    .Select(x => ExactMath.UncheckedUIntPower(x.prime, (uint)x.exponent / 2))
                    .Aggregate((a, b) => a * b);

                //tex:$$r' := \frac{r}{\displaystyle\prod_{p^e \in F_s}p^e}$$
                int newRadicand = term.radicand / simplifyableFactors
                    .Select(x => ExactMath.UncheckedUIntPower(x.prime, (uint)x.exponent))
                    .Aggregate((a, b) => a * b);

                //tex:$$c'\sqrt{r'}$$
                return new Radical(newCoefficient, newRadicand);
            });

        //tex:
        //Factor out
        //$$\frac{c_0(c_1'\sqrt{r_1}+c_2'\sqrt{r_2}+\dots+c_n'\sqrt{r_n})}{d}$$

        //tex:
        //CommonFactors($x_1, x_2, \dots, x_n$) returns the common factors in the form
        //$$
        //\begin{gathered}
        //  x_1, x_2, \dots, x_n\\
        //  = c_1(a_{1,1}, a_{1,2}, \dots, a_{1,n})\\
        //  = c_2(a_{2,1}, a_{2,2}, \dots, a_{2,n})\\
        //  \vdots\\
        //  = c_m(a_{m,1}, a_{m,2}, \dots, a_{m,n})
        //\end{gathered}
        //\implies
        //\begin{bmatrix}
        //  (\mathtt{common}_1, [
        //      \mathtt{associated}_{1,1},
        //      \mathtt{associated}_{1,2},
        //      \dots,
        //      \mathtt{associated}_{1,n}
        //  ]),\\
        //  (\mathtt{common}_2, [
        //      \mathtt{associated}_{2,1},
        //      \mathtt{associated}_{2,2},
        //      \dots,
        //      \mathtt{associated}_{2,n}
        //  ]),\\
        //  \vdots\\
        //  (\mathtt{common}_m, [
        //      \mathtt{associated}_{m,1},
        //      \mathtt{associated}_{m,2},
        //      \dots,
        //      \mathtt{associated}_{m,n}
        //  ])
        //\end{bmatrix}
        //$$
        //In ascending order of $\mathtt{common}$.
        //The first element is always
        //$$(1, [x_1, x_2, \dots, x_n])$$
        //The last element is always
        //$$\left(\text{GCF}, \left[\frac{x_1}{\text{GCF}}, \frac{x_2}{\text{GCF}}, \dots, \frac{x_n}{\text{GCF}}\right]\right)$$
        //If there is only one number (i.e. CommonFactors($x$)), the entire output is always
        //$$[(1, [x]), (x, [1])]$$

        //tex:
        //$$
        //\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}}{d}\\
        //(\mathtt{gcf}, [c_1', c_2', \dots, c_n']) = \text{CommonFactors}(c_1, c_2, \dots, c_n)_n
        //$$
        (int gcf, int[] associatedFactors) =
            ExactMath.CommonFactors(
                numeratorTerms
                    .Select(x => x.coefficient)
                    .ToArray()
            )
            .Last();

        //tex:
        //$$
        //N := [ c_1'\sqrt{r_1}, c_2'\sqrt{r_2}, \dots, c_n'\sqrt{r_n} ]
        //$$
        numeratorTerms = numeratorTerms
            .Zip(
                associatedFactors,
                (term, factor) =>
                    new Radical(
                        factor,
                        term.radicand
                    )
            );

        //tex:$$
        //\frac{c_0}{d} = \frac{m\frac{c_0}{m}}{m\frac{d}{m}} \mid m \in \mathbb{Z} \qquad (m \text{ may be } 1)
        //$$
        int coefGCF = ExactMath.GCF(gcf, denominator);

        //tex:$$c_0' := \frac{c_0}{m}$$
        int numeratorCoef = gcf / coefGCF;
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

        //tex:
        //For each unique radicand, find the sum of all coefficients to radicals sharing that radicand.
        //Let each such sum be an element $c''$.
        //Let the resulting set be $C$.
        //$$
        //C := \left\{
        //  \left(c'' := \sum_{t_c\sqrt{t_r} \in N} t_c \mid t_r = r\right) \sqrt{r}
        //  \;\middle|\;
        //  c'\sqrt{!\exists r} \in N,
        //  c''\sqrt{r} \ne 0
        //\right\}
        //\implies
        //\underbrace{\left\{
        //  c_1''\sqrt{r_1}, c_2''\sqrt{r_2}, \dots, c_n''\sqrt{r_n}
        //\right\}}_{1 \dots n \text{ does not refer to the original index}}
        //\\
        //0 \notin C
        //$$
        var combined = numeratorTerms
            .Select(x => x.radicand)
            .Distinct()
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

        // Every term in the numerator was equal to 0 and got excluded;
        // this is also known as "being equal to zero".
        //tex:$$C = \{\} \implies \frac{\sum_{t \in C}}{x \ne 0} = 0$$
        if (combined.Count() == 0)
        {
            return (Number)0;
        }

        var alg = new Algebraic(
            combined.Select(x => new Radical(x.coefficient * numeratorCoef, x.radicand)).ToArray(),
            denominatorCoef
        );

        if (alg.denominator < 0)
        {
            throw new Exception("That wasn't supposed to happen");
        }

        //tex:$$\frac{c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_m\sqrt{r_n}}{0} = \frac{1}{0}$$
        if (alg.denominator == 0)
        {
            return Bald.UNDEFINED;
        }
        //tex:$$\frac{c\sqrt{r}}{d}$$
        else if (alg.numeratorTerms.Length == 1)
        {
            var numerator = alg.numeratorTerms[0];
            //tex:$$\frac{c\sqrt{r}}{1}$$
            if (alg.denominator == 1)
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
                return new Fraction(numerator.coefficient, alg.denominator);
            }
            //tex:$$\frac{c\sqrt{r}}{d}$$
            else
            {
                return new RadicalFraction(numerator, alg.denominator);
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
    //\frac{\sqrt{c_1}+\sqrt{c_2}+\dots+\sqrt{c_m}}{d}\\
    // =\\
    //\frac{
    //  d\sqrt{a_1}+d\sqrt{a_2}+\dots+d\sqrt{a_n}
    //}{
    //  b\sqrt{c_1}+b\sqrt{c_2}+\dots+b\sqrt{c_m}
    //}\\
    // =\\
    // ?
    //\end{gathered}$$
    public static Algebraic operator /(Algebraic lhs, Algebraic rhs)
    {
        //tex:$$\begin{gathered}
        //\frac{\sqrt{a_1}+\sqrt{a_2}+\dots+\sqrt{a_n}}{b}
        //\div
        //\frac{c_0\sqrt{c_1}}{d}\\
        // =\\
        //\frac{d\sqrt{a_1}+d\sqrt{a_2}+\dots+d\sqrt{a_n}}{bc_0\sqrt{c_1}}\\
        // =\\
        //\frac{d\sqrt{a_1}\sqrt{c_1}+d\sqrt{a_2}\sqrt{c_1}+\dots+d\sqrt{a_n}\sqrt{c_1}}{bc_0\sqrt{c_1}\sqrt{c_1}}\\
        // =\\
        //\frac{d\sqrt{a_1c_1}+d\sqrt{a_2c_1}+\dots+d\sqrt{a_nc_1}}{bc_0c_1}
        //\end{gathered}$$
        if (rhs.numeratorTerms.Length == 1)
        {
            var rhsNumerator = rhs.numeratorTerms[0];
            var newNumeratorTerms = lhs.numeratorTerms.Select(
                x => new Radical(
                    x.coefficient * rhs.denominator, 
                    x.radicand * rhsNumerator.radicand
                )
            );
            return new(
                newNumeratorTerms.ToArray(),
                lhs.denominator * rhsNumerator.coefficient * rhsNumerator.radicand
            );
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
