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

        public static Algebraic operator /(SumOfRadicals lhs, int rhs) =>
            new(lhs, rhs);

        public static Algebraic operator /(SumOfRadicals lhs, SumOfRadicals rhs) =>
            new Algebraic(lhs) / new Algebraic(rhs);

        public static Algebraic operator /(int lhs, SumOfRadicals rhs) =>
            lhs / new Algebraic(rhs);

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

        public readonly SumOfRadicals Squared() =>
            this * this;

        //tex:$$
        //\overline{\sqrt{2} + \sqrt{3}} = \sqrt{2} - \sqrt{3}\\
        //\overline{\sqrt{2} + \sqrt{3} + \sqrt{5}} = \sqrt{2} + \sqrt{3} - \sqrt{5}\\
        //\vdots
        //$$
        public readonly SumOfRadicals Conjugate() =>
            terms.Length switch
            {
                < 0 => throw new IndexOutOfRangeException(),
                0 or 1 => this,
                > 1 => new SumOfRadicals([..terms.Take(terms.Length - 1)]) - terms.Last(),
            };

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
                    //  \left(p,e-e\bmod2\right)
                    //  \mid
                    //  p^e \in F_p,
                    //  e \ge 2
                    //\right\}
                    //$$
                    (int prime, uint exponentRoot)[] simplifyableFactors = [..primeFactors
                        .Where(x => x.exponent >= 2)
                        .Select(x => (x.prime, (uint)(x.exponent / 2)))]; // Prime factors cannot have negative powers

                    //tex:$$
                    //\nexists e \in F_p \mid e \ge 2
                    //\implies
                    //\text{radical term is in simplest form}
                    //\iff
                    //\text{radical index = 2}
                    //$$
                    if (simplifyableFactors.Length == 0) return term;
                    
                    //tex:$$x := \prod_{p^e \in F_s}\left(\sqrt{p^e} = p^{\frac{e}{2}}\right)$$
                    int root = simplifyableFactors
                        .Select(x => ExactMath.UncheckedUIntPower(x.prime, x.exponentRoot))
                        .Aggregate(1, (a, b) => a * b);
                    
                    //tex:$$c' := cx$$
                    int newCoefficient = term.coefficient * root;

                    //tex:$$r' := \frac{r}{x^2}$$
                    int newRadicand = term.radicand / (root * root);

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
            (int gcf, int[] associatedFactors) = ExactMath.CommonFactors([..terms.Select(x => x.coefficient)]).Last();

            //tex:
            //$$
            //c_0(c_1'\sqrt{r_1} + c_2'\sqrt{r_2} + \dots + c_n'\sqrt{r_n})
            //$$
            return (gcf, new([..terms.Zip(associatedFactors, (term, factor) => new Radical(factor, term.radicand))]));
        }

        public readonly SumOfRadicals LikeTermsCombined()
        {
            //tex:
            //For each unique radicand, find the sum of all coefficients to radicals sharing that radicand.
            //$$
            //\left\{
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
            //$$

            return new([..terms
                .GroupBy(term => term.radicand)
                .Select(group => new Radical(
                    group.Sum(term => term.coefficient),
                    group.Key))
                .Where(term => !term.IsZero())]);
        }

        public readonly bool TryMonomial(out Radical term)
        {
            bool isMonomial = terms.Length == 1;
            term = isMonomial ? terms[0] : default;
            return isMonomial;
        }

        public readonly bool TryBinomial(out Radical term1, out Radical term2)
        {
            bool isBinomial = terms.Length == 2;
            (term1, term2) = isBinomial ? (terms[0], terms[1]) : (default, default);
            return isBinomial;
        }
    }

    public readonly NotationKind Kind => NotationKind.Algebraic;

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
        (numerator, denominator) = (new SumOfRadicals(new Radical(0, 1)), 0);

    public Algebraic(SumOfRadicals numerator, int denominator) =>
        (this.numerator, this.denominator) = (
            denominator < 0 ? -numerator : numerator,
            Math.Abs(denominator)
        );

    public Algebraic(SumOfRadicals numerator) =>
        this.numerator = numerator;

    public static implicit operator Algebraic(SumOfRadicals numerator) =>
        new(numerator);

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

    public static Algebraic operator /(Algebraic lhs, Algebraic rhs)
    {
        //tex:$$\begin{gathered}
        //\frac{\sqrt{a_1} + \sqrt{a_2} + \dots + \sqrt{a_n}}{b}
        //\div
        //\frac{\sqrt{c_1} + \sqrt{c_2} + \dots + \sqrt{c_m}}{d}\\
        // =\\
        //\frac{d\sqrt{a_1} + d\sqrt{a_2} + \dots + d\sqrt{a_n}}{b\sqrt{c_1} + b\sqrt{c_2} + \dots + b\sqrt{c_m}}
        //\end{gathered}$$
        var numerator   = lhs.numerator * rhs.denominator;
        var denominator = rhs.numerator * lhs.denominator;

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
            throw new Exception("Assumption failed");
        }

        return new Algebraic(numerator, term.coefficient);
    }

    public override readonly bool Equals(object obj) =>
        obj is Algebraic alg &&
        denominator == alg.denominator &&
        numerator.terms.Length == alg.numerator.terms.Length &&
        numerator.terms.Zip(alg.numerator.terms, Tuple.Create)
        .All(pair => 
            pair.Item1.coefficient == pair.Item2.coefficient &&
            pair.Item1.radicand == pair.Item2.radicand
        );

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
