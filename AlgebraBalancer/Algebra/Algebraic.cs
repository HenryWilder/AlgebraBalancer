using System;
using System.Linq;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.IAlgebraicNotation;

namespace AlgebraBalancer.Algebra;
public struct Algebraic : IAlgebraicExpression
{
    public struct SumOfRadicals(params Radical[] terms)
    {
        public Radical[] terms = terms;

        public static SumOfRadicals operator *(SumOfRadicals lhs, int rhs) =>
            new([..lhs.terms.Select(term => new Radical(term.coefficient * rhs, term.radicand))]);

        public static SumOfRadicals operator *(SumOfRadicals lhs, SumOfRadicals rhs) =>
            new([..lhs.terms.SelectMany(lterm => rhs.terms.Select(rterm => lterm * rterm))]);

        public static SumOfRadicals operator -(SumOfRadicals rhs) =>
            new([..rhs.terms.Select(term => -term)]);

        public static SumOfRadicals operator +(SumOfRadicals lhs, Radical rhs) =>
            new([..lhs.terms.Append(rhs)]);

        public static SumOfRadicals operator +(SumOfRadicals lhs, SumOfRadicals rhs) =>
            new([..lhs.terms.Concat(rhs.terms)]);

        public static SumOfRadicals operator -(SumOfRadicals lhs, Radical rhs) =>
            new([..lhs.terms.Append(-rhs)]);

        public static SumOfRadicals operator -(SumOfRadicals lhs, SumOfRadicals rhs) =>
            new([..lhs.terms.Concat(rhs.terms.Select(term => -term))]);

        public readonly SumOfRadicals TermsSimplified()
        {
            return new([..terms
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
                })]);
        }

        public readonly (int commonFactor, SumOfRadicals newSum) Factored()
        {
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

            //tex:$$c_1\sqrt{r_1}+c_2\sqrt{r_2}+\dots+c_n\sqrt{r_n}$$

            //tex:$$(c_0, [c_1', c_2', \dots, c_n']) = \text{CommonFactors}(c_1, c_2, \dots, c_n)$$
            (int gcf, int[] associatedFactors) =
                ExactMath.CommonFactors([.. terms.Select(x => x.coefficient)])
                .Last();

            //tex:
            //$$
            //c_0(c_1'\sqrt{r_1} + c_2'\sqrt{r_2} + \dots + c_n'\sqrt{r_n})
            //$$
            return (gcf, new([.. terms.Zip(associatedFactors, (term, factor) => new Radical(factor, term.radicand))]));
        }

        public readonly SumOfRadicals LikeTermsCombined()
        {
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
            
            var terms = this.terms;
            return new([..terms
                .Select(x => x.radicand)
                .Distinct()
                .Select(uniqueRadicand =>
                    new Radical(
                        terms
                            .Where(x => x.radicand == uniqueRadicand)
                            .Sum(x => x.coefficient),
                        uniqueRadicand
                    )
                )
                .Where(x =>
                    x.coefficient != 0 &&
                    x.radicand != 0
                )
            ]);
        }

        public readonly bool TryMonomial(out Radical term)
        {
            if (terms.Length == 1)
            {
                term = terms[0];
                return true;
            }
            else
            {
                term = default;
                return false;
            }
        }

        public readonly bool TryBinomial(out Radical term1, out Radical term2)
        {
            if (terms.Length == 2)
            {
                term1 = terms[0];
                term2 = terms[1];
                return true;
            }
            else
            {
                term1 = default;
                term2 = default;
                return false;
            }
        }
    }

    public readonly NotationKind Kind => NotationKind.Algebraic;

    public Algebraic() =>
        numerator = new();

    public Algebraic(int num) =>
        numerator = new(new Radical(num, 1));

    public Algebraic(Number num) =>
        numerator = new(new Radical(num, 1));

    public Algebraic(Fraction frac) =>
        (numerator, denominator) = (
            new(new Radical((frac.denominator < 0 ? -1 : 1) * frac.numerator, 1)),
            Math.Abs(frac.denominator)
        );

    public Algebraic(Radical rad) =>
        numerator = new(rad);

    public Algebraic(RadicalFraction radFrac) =>
        (numerator, denominator) = (
            new(new Radical((radFrac.denominator < 0 ? -1 : 1) * radFrac.numerator.coefficient, radFrac.numerator.radicand)),
            Math.Abs(radFrac.denominator)
        );

    public Algebraic(Imaginary imag) =>
        numerator = new(new Radical(imag.coef, -1));

    public Algebraic(Complex cmplx) =>
        numerator = new SumOfRadicals(new Radical(cmplx.real, 1), new Radical(cmplx.imag.coef, -1));

    public Algebraic(Undefined _) =>
        (numerator, denominator) = (new SumOfRadicals(new Radical(0, 1)), 0);

    public Algebraic(SumOfRadicals numerator, int denominator) =>
        (this.numerator, this.denominator) = (
            denominator < 0 ? -numerator : numerator,
            Math.Abs(denominator)
        );

    public Algebraic(Radical[] numeratorTerms, int denominator) =>
        new Algebraic(new SumOfRadicals(numeratorTerms), denominator);

    public SumOfRadicals numerator;
    public int denominator = 1;

    public readonly IAlgebraicNotation Simplified()
    {
        // Because we are only simplifying, there is no case in which
        // having a 0-denominator at the start isn't undefined.
        if (denominator == 0)
        {
            return Bald.UNDEFINED;
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

        var (gcf, factoredNumerator) = simplifiedNumerator.Factored();

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

        var combinedNumerator = factoredNumerator.LikeTermsCombined();

        //tex:
        //Every term in the numerator was equal to 0 and got excluded;
        //this is also known as "being equal to zero".
        //$$C = \{\} \implies \frac{\displaystyle \sum_{x \in C} = 0}{d} = 0 \iff d \ne 0$$
        if (combinedNumerator.terms.Length == 0) return (Number)0;

        //tex:$$
        //\mathtt{alg} :=
        //\begin{cases}
        //0 & \text{if } C \equiv \varnothing\\
        //\dfrac{c_0'c_1''\sqrt{r_1} + c_0'c_2''\sqrt{r_2} + \dots + c_0'c_n''\sqrt{r_n}}{d'}  & \text{if } C \not\equiv \varnothing\\
        //\end{cases}$$
        var alg = new Algebraic(combinedNumerator * numeratorCoef, denominatorCoef);

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
                    return numeratorTerm;
                }
            }
            //tex:$$\frac{c\sqrt{1}}{d} = \frac{c}{d}$$
            else if (numeratorTerm.radicand == 1)
            {
                return new Fraction(numeratorTerm.coefficient, alg.denominator);
            }
            //tex:$$\frac{c\sqrt{r}}{d}$$
            else
            {
                return new RadicalFraction(numeratorTerm, alg.denominator);
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

    public override readonly string ToString() => $"({string.Join("+", numerator.terms)})/{denominator}";

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
        if (rhs.numerator.TryMonomial(out var rhsNumerator))
        {
            SumOfRadicals newNumeratorTerms = new([
                ..lhs.numerator.terms.Select(
                    x => new Radical(
                        x.coefficient * rhs.denominator, 
                        x.radicand * rhsNumerator.radicand
                    )
                )
            ]);
            return new(
                newNumeratorTerms,
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
