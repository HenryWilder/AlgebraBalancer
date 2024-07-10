using System.Linq;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;

public class TermMultiplicand(string variable, int degree)
{
    public string variable = variable;
    public int degree = degree;

    public override string ToString() => $"{variable}{LatexUnicode.ToSuperscript(degree.ToString())}";

    public bool IsIdentity => degree == 0;
}

public class PolynomialTerm(int coefficient, params TermMultiplicand[] multiplicands)
{
    public int coefficient = coefficient;
    public TermMultiplicand[] multiplicands = multiplicands;

    public string MultiplicandsToString() => string.Concat(multiplicands.Where(mult => !mult.IsIdentity));
    public override string ToString()
    {
        string multStr = MultiplicandsToString();
        return string.IsNullOrEmpty(multStr)
            ? coefficient.ToString()
            : (coefficient == 1 ? "" : coefficient == -1 ? "-" : coefficient.ToString()) + multStr;
    }

    public bool IsZero => coefficient == 0;

    public int Degree() => multiplicands.Max(mult => mult.degree);
    public string[] Variables() => [.. multiplicands.Select(mult => mult.variable).Distinct()];
    public bool IsConstant => multiplicands.Length == 0;
}

public class Polynomial(PolynomialTerm[] terms) : IAlgebraicNotation
{
    public PolynomialTerm[] terms = terms;

    public bool IsInoperable => false;

    public int Degree() => terms.Max(term => term.Degree());
    public string[] Variables() => [.. terms.SelectMany(term => term.Variables()).Distinct()];

    public int LeadingCoefficient() => terms.Length > 0
        ? terms[0].coefficient
        : 0;

    public int ConstantTerm() => terms.Length > 0 && terms.Last().IsConstant
        ? terms.Last().coefficient
        : 0;

    public bool TryMonomial(out PolynomialTerm term)
    {
        bool isMonomial = terms.Length == 1;
        term = isMonomial ? terms[0] : null;
        return isMonomial;
    }

    public bool TryBinomial(out PolynomialTerm term1, out PolynomialTerm term2)
    {
        bool isBinomial = terms.Length == 2;
        (term1, term2) = isBinomial ? (terms[0], terms[1]) : (null, null);
        return isBinomial;
    }

    public bool TryTrinomial(out PolynomialTerm term1, out PolynomialTerm term2, out PolynomialTerm term3)
    {
        bool isTrinomial = terms.Length == 3;
        (term1, term2, term3) = isTrinomial ? (terms[0], terms[1], terms[2]) : (null, null, null);
        return isTrinomial;
    }

    public Polynomial SimplifiedToPolynomial()
    {
        return new Polynomial([..terms
            //Simplify terms and order by degree
            
            //tex:$$\begin{gathered}
            //5x^2x + 65x^3xx^0 + 2x^0x^0 - 4x - 4x^2x^{-1} + 3x^2x^{-2} + 7xx + 8x - x^2 + 7yyy^3y^2y^{-4} - 2xyy\\
            //\Downarrow\\
            //\begin{bmatrix}
            //5x^2x^1\\
            //65x^3x^1x^0\\
            //2x^0x^0\\
            //-4x^1\\
            //-4x^2x^{-1}\\
            //3x^2x^{-2}\\
            //7x^1x^1\\
            //8x^1\\
            //-1x^2\\
            //7yyy^3y^2y^{-4}\\
            //-2xyy\\
            //\end{bmatrix}
            //\end{gathered}$$

            .Select(term => new PolynomialTerm(
                //tex:$$
                //\begin{bmatrix}
                //5x^2x^1\\
                //65x^3x^1x^0\\
                //2x^0x^0\\
                //-4x^1\\
                //-4x^2x^{-1}\\
                //3x^2x^{-2}\\
                //7x^1x^1\\
                //8x^1\\
                //-1x^2\\
                //7yyy^3y^2y^{-4}\\
                //-2xyy\\
                //\end{bmatrix}
                //\implies
                //\begin{bmatrix}
                //5\\
                //65\\
                //2\\
                //-4\\
                //-4\\
                //3\\
                //7\\
                //8\\
                //-1\\
                //7\\
                //-2\\
                //\end{bmatrix}
                //$$
                term.coefficient,

                //tex:$$
                //\begin{bmatrix}
                //5x^2x^1\\
                //65x^3x^1x^0\\
                //2x^0x^0\\
                //-4x^1\\
                //-4x^2x^{-1}\\
                //3x^2x^{-2}\\
                //7x^1x^1\\
                //8x^1\\
                //-1x^2\\
                //7yyy^3y^2y^{-4}\\
                //-2xyy\\
                //\end{bmatrix}
                //\implies
                //\begin{bmatrix}
                //x^2 & x^1\\
                //x^3 & x^1 & x^0\\
                //x^0 & x^0\\
                //x^1\\
                //x^2 & x^{-1}\\
                //x^2 & x^{-2}\\
                //x^1 & x^1\\
                //x^1\\
                //x^2\\
                //y^1 & y^1 & y^3 & y^2 & y^{-4}\\
                //x^1 & y^1 & y^1\\
                //\end{bmatrix}
                //$$
                [.. term.multiplicands
                    //tex:$$
                    //\begin{bmatrix}
                    //x^2 & x^1\\
                    //x^3 & x^1 & x^0\\
                    //x^0 & x^0\\
                    //x^1\\
                    //x^2 & x^{-1}\\
                    //x^2 & x^{-2}\\
                    //x^1 & x^1\\
                    //x^1\\
                    //x^2\\
                    //y^1 & y^1 & y^3 & y^2 & y^{-4}\\
                    //x^1 & y^1 & y^1\\
                    //\end{bmatrix}
                    //\implies
                    //\begin{bmatrix}
                    //x:\begin{bmatrix}x^2 & x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^3 & x^1 & x^0\end{bmatrix}\\
                    //x:\begin{bmatrix}x^0 & x^0\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2 & x^{-1}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2 & x^{-2}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1 & x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2\end{bmatrix}\\
                    //y:\begin{bmatrix}y^1 & y^1 & y^3 & y^2 & y^{-4}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix} & y:\begin{bmatrix}y^1 & y^1\end{bmatrix}\\
                    //\end{bmatrix}
                    //$$
                    .GroupBy(mult => mult.variable)
                    //tex:$$
                    //\begin{bmatrix}
                    //x:\begin{bmatrix}x^2 & x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^3 & x^1 & x^0\end{bmatrix}\\
                    //x:\begin{bmatrix}x^0 & x^0\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2 & x^{-1}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2 & x^{-2}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1 & x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix}\\
                    //x:\begin{bmatrix}x^2\end{bmatrix}\\
                    //y:\begin{bmatrix}y^1 & y^1 & y^3 & y^2 & y^{-4}\end{bmatrix}\\
                    //x:\begin{bmatrix}x^1\end{bmatrix} & y:\begin{bmatrix}y^1 & y^1\end{bmatrix}\\
                    //\end{bmatrix}
                    //\implies
                    //\begin{bmatrix}
                    //x^{\sum\begin{bmatrix}2 & 1\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}3 & 1 & 0\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}0 & 0\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}1\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}2 & -1\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}2 & -2\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}1 & 1\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}1\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}2\end{bmatrix}}\\
                    //y^{\sum\begin{bmatrix}1 & 1 & 3 & 2 & -4\end{bmatrix}}\\
                    //x^{\sum\begin{bmatrix}1\end{bmatrix}} & y^{\sum\begin{bmatrix}1 & 1\end{bmatrix}}\\
                    //\end{bmatrix}
                    //\implies
                    //\begin{bmatrix}
                    //x^3\\
                    //x^4\\
                    //x^0\\
                    //x^1\\
                    //x^1\\
                    //x^0\\
                    //x^2\\
                    //x^1\\
                    //x^2\\
                    //y^3\\
                    //x^1 & y^2\\
                    //\end{bmatrix}
                    //$$
                    .Select(variable => new TermMultiplicand(
                        variable.Key,
                        variable.Sum(mult => mult.degree)))
                    //tex:$$
                    //\begin{bmatrix}
                    //x^3\\
                    //x^4\\
                    //x^0\\
                    //x^1\\
                    //x^1\\
                    //x^0\\
                    //x^2\\
                    //x^1\\
                    //x^2\\
                    //y^3\\
                    //x^1 & y^2\\
                    //\end{bmatrix}
                    //\implies
                    //\begin{bmatrix}
                    //x^3\\
                    //x^4\\
                    //\\
                    //x^1\\
                    //x^1\\
                    //\\
                    //x^2\\
                    //x^1\\
                    //x^2\\
                    //y^3\\
                    //x^1 & y^2\\
                    //\end{bmatrix}
                    //$$
                    .Where(mult => !mult.IsIdentity)
                    //tex:$$
                    //\begin{bmatrix}
                    //x^3\\
                    //x^4\\
                    //\\
                    //x^1\\
                    //x^1\\
                    //\\
                    //x^2\\
                    //x^1\\
                    //x^2\\
                    //y^3\\
                    //x^1 & y^2\\
                    //\end{bmatrix}
                    //\implies
                    //\begin{bmatrix}
                    //x^3\\
                    //x^4\\
                    //\\
                    //x^1\\
                    //x^1\\
                    //\\
                    //x^2\\
                    //x^1\\
                    //x^2\\
                    //y^3\\
                    //y^2 & x^1\\
                    //\end{bmatrix}
                    //$$
                    .OrderByDescending(mult => mult.degree)]))
            //tex:$$
            //\begin{bmatrix}
            //5x^{\boxed{3}}\\
            //65x^{\boxed{4}}\\
            //2\phantom{x}^{\boxed{0}}\\
            //-4x^{\boxed{1}}\\
            //-4x^{\boxed{1}}\\
            //3\phantom{x}^{\boxed{0}}\\
            //7x^{\boxed{2}}\\
            //8x^{\boxed{1}}\\
            //-1x^{\boxed{2}}\\
            //7y^{\boxed{3}}\\
            //-2y^{\boxed{2}}x^1\\
            //\end{bmatrix}
            //\implies
            //\begin{bmatrix}
            //65x^{\boxed{4}}\\
            //5x^{\boxed{3}}\\
            //7y^{\boxed{3}}\\
            //7x^{\boxed{2}}\\
            //-1x^{\boxed{2}}\\
            //-2y^{\boxed{2}}x^1\\
            //-4x^{\boxed{1}}\\
            //-4x^{\boxed{1}}\\
            //8x^{\boxed{1}}\\
            //2\phantom{x}^{\boxed{0}}\\
            //3\phantom{x}^{\boxed{0}}\\
            //\end{bmatrix}
            //$$
            .OrderByDescending(term => term.Degree())

            //Combine like terms
            
            //tex:$$
            //\begin{bmatrix}
            //65x^4\\
            //5x^3\\
            //7y^3\\
            //7x^2\\
            //-1x^2\\
            //-2y^2x^1\\
            //-4x^1\\
            //-4x^1\\
            //8x^1\\
            //2\\
            //3
            //\end{bmatrix}
            //\implies
            //\begin{bmatrix}
            //`` x^4  \text{''}: & \begin{bmatrix} 65          \end{bmatrix}\\
            //`` x^3  \text{''}: & \begin{bmatrix} 5 & 7       \end{bmatrix}\\
            //`` x^2  \text{''}: & \begin{bmatrix} 7 & -1      \end{bmatrix}\\
            //`` y^2x \text{''}: & \begin{bmatrix} -2          \end{bmatrix}\\
            //`` x    \text{''}: & \begin{bmatrix} -4 & -4 & 8 \end{bmatrix}\\
            //``      \text{''}: & \begin{bmatrix} 2 & 3       \end{bmatrix}\\
            //\end{bmatrix}
            //$$
            .GroupBy(term => term.MultiplicandsToString())
            //tex:$$
            //\begin{bmatrix}
            //`` x^4  \text{''}: & \begin{bmatrix} 65          \end{bmatrix}\\
            //`` x^3  \text{''}: & \begin{bmatrix} 5 & 7       \end{bmatrix}\\
            //`` x^2  \text{''}: & \begin{bmatrix} 7 & -1      \end{bmatrix}\\
            //`` y^2x \text{''}: & \begin{bmatrix} -2          \end{bmatrix}\\
            //`` x    \text{''}: & \begin{bmatrix} -4 & -4 & 8 \end{bmatrix}\\
            //``      \text{''}: & \begin{bmatrix} 2 & 3       \end{bmatrix}\\
            //\end{bmatrix}
            //\implies
            //\begin{bmatrix}
            //`` x^4  \text{''}: & (\sum\begin{bmatrix} 65          \end{bmatrix}) & x^4 \\
            //`` x^3  \text{''}: & (\sum\begin{bmatrix} 5 & 7       \end{bmatrix}) & x^3 \\
            //`` x^2  \text{''}: & (\sum\begin{bmatrix} 7 & -1      \end{bmatrix}) & x^2 \\
            //`` y^2x \text{''}: & (\sum\begin{bmatrix} -2          \end{bmatrix}) & y^2x\\
            //`` x    \text{''}: & (\sum\begin{bmatrix} -4 & -4 & 8 \end{bmatrix}) & x   \\
            //``      \text{''}: & (\sum\begin{bmatrix} 2 & 3       \end{bmatrix})       \\
            //\end{bmatrix}
            //\implies
            //\begin{bmatrix}
            //65x^4 \\
            //12x^3 \\
            // 6x^2 \\
            //-2y^2x\\
            // 0x   \\
            // 5    \\
            //\end{bmatrix}
            //$$
            .Select(likeTerms => new PolynomialTerm(
                likeTerms.Sum(term => term.coefficient),
                likeTerms.First().multiplicands))

            //Remove terms equal to zero

            //tex:$$
            //\begin{bmatrix}
            //65x^4 \\
            //12x^3 \\
            // 6x^2 \\
            //-2y^2x\\
            // 0x   \\
            // 5    \\
            //\end{bmatrix}
            //\implies
            //\begin{bmatrix}
            //65x^4 \\
            //12x^3 \\
            // 6x^2 \\
            //-2y^2x\\
            // 5    \\
            //\end{bmatrix}
            //$$
            .Where(term => !term.IsZero)

            //tex:$$
            //\begin{bmatrix}
            //65x^4 \\
            //12x^3 \\
            // 6x^2 \\
            //-2y^2x\\
            // 5    \\
            //\end{bmatrix}
            //\implies
            //65x^4 + 12x^3 + 6x^2 - 2y^2x + 5    
            //$$
        ]);
    }

    public IAlgebraicNotation Simplified()
    {
        var poly = SimplifiedToPolynomial();

        if (poly.TryMonomial(out var term) && term.IsConstant)
        {
            return (Number)term.coefficient;
        }

        return poly;
    }

    public string AsEquality(string lhs) => $"{lhs} = {ToString()}";
}
