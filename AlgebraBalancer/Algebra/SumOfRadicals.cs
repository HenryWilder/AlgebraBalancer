using System;
using System.Linq;

namespace AlgebraBalancer.Algebra;
public class SumOfRadicals(params Radical[] terms)
{
    public bool IsInoperable => false;

    public Radical[] terms = terms;

    public static SumOfRadicals operator *(SumOfRadicals lhs, int rhs) =>
        new([.. lhs.terms.Select(term => new Radical(term.coefficient * rhs, term.radicand))]);

    public static Algebraic operator /(SumOfRadicals lhs, int rhs) =>
        new(lhs, rhs);

    public static Algebraic operator /(SumOfRadicals lhs, SumOfRadicals rhs) =>
        new Algebraic(lhs) / new Algebraic(rhs);

    public static Algebraic operator /(int lhs, SumOfRadicals rhs) =>
        lhs / new Algebraic(rhs);

    public static SumOfRadicals operator *(SumOfRadicals lhs, SumOfRadicals rhs) =>
        new([.. lhs.terms.SelectMany(lterm => rhs.terms.Select(rterm => lterm * rterm))]);

    public static SumOfRadicals operator -(SumOfRadicals rhs) =>
        new([.. rhs.terms.Select(term => -term)]);

    public static SumOfRadicals operator +(SumOfRadicals lhs, Radical rhs) =>
        new([.. lhs.terms.Append(rhs)]);

    public static SumOfRadicals operator +(SumOfRadicals lhs, SumOfRadicals rhs) =>
        new([.. lhs.terms.Concat(rhs.terms)]);

    public static SumOfRadicals operator -(SumOfRadicals lhs, Radical rhs) =>
        new([.. lhs.terms.Append(-rhs)]);

    public static SumOfRadicals operator -(SumOfRadicals lhs, SumOfRadicals rhs) =>
        new([.. lhs.terms.Concat(rhs.terms.Select(term => -term))]);

    public SumOfRadicals Squared() =>
        this * this;

    //tex:$$
    //\overline{\sqrt{2} + \sqrt{3}} = \sqrt{2} - \sqrt{3}\\
    //\overline{\sqrt{2} + \sqrt{3} + \sqrt{5}} = \sqrt{2} + \sqrt{3} - \sqrt{5}\\
    //\vdots
    //$$
    public SumOfRadicals Conjugate() =>
        terms.Length switch
        {
            < 0 => throw new IndexOutOfRangeException(),
            0 or 1 => this,
            > 1 => new SumOfRadicals([.. terms.Take(terms.Length - 1)]) - terms.Last(),
        };

    public SumOfRadicals TermsSimplified()
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

    public (int commonFactor, SumOfRadicals newSum) Factored()
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
        (int gcf, int[] associatedFactors) = ExactMath.CommonFactors([.. terms.Select(x => x.coefficient)]).Last();

        //tex:
        //$$
        //c_0(c_1'\sqrt{r_1} + c_2'\sqrt{r_2} + \dots + c_n'\sqrt{r_n})
        //$$
        return (gcf, new([.. terms.Zip(associatedFactors, (term, factor) => new Radical(factor, term.radicand))]));
    }

    public SumOfRadicals LikeTermsCombined()
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

    public bool TryMonomial(out Radical term)
    {
        bool isMonomial = terms.Length == 1;
        term = isMonomial ? terms[0] : default;
        return isMonomial;
    }

    public bool TryBinomial(out Radical term1, out Radical term2)
    {
        bool isBinomial = terms.Length == 2;
        (term1, term2) = isBinomial ? (terms[0], terms[1]) : (default, default);
        return isBinomial;
    }
}
