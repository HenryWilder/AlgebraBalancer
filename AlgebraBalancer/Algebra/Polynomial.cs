using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;

public class TermMultiplicand(string variable, int degree = 1)
{
    public string variable = variable;
    public int degree = degree;

    public override string ToString() => variable + LatexUnicode.ToSuperscript(degree.ToString());

    public override bool Equals(object obj) =>
        obj is TermMultiplicand other &&
        variable == other.variable &&
        degree == other.degree;

    public bool IsIdentity => degree == 0;
}

public class PolynomialTerm
{
    public PolynomialTerm(int coefficient, params TermMultiplicand[] multiplicands) =>
        (this.coefficient, this.multiplicands) = (coefficient, [.. multiplicands.Where(mult => mult.degree != 0)]);

    public PolynomialTerm(params TermMultiplicand[] multiplicands) =>
        new PolynomialTerm(1, multiplicands);

    private static readonly Regex rxTerm =
        new(@"^(?'coef'[-+]?\d*)(?:(?'var'\p{L}[₀₁₂₃₄₅₆₇₈₉₌ₐₑₕₖₗₘₙₒₚₛₜₓ'""`′″‴‵‶‷]*)(?:\^(?'openbr'\{)?(?'deg'\d+)(?(openbr)\})|(?'deg'[⁰¹²³⁴⁵⁶⁷⁸⁹]*)))*$",
            RegexOptions.Compiled);
    public static PolynomialTerm Parse(string str)
    {
        var match = rxTerm.Match(str.Replace(" ", ""));
        if (match.Success)
        {
            string coefStr = match.Groups["coef"].Value;
            int coef = coefStr == "" ? 1 : coefStr == "-" ? -1 : int.Parse(coefStr);
            if (coef == 0) return null;
            var varCaps = match.Groups["var"].Captures;
            var degCaps = match.Groups["deg"].Captures;
            TermMultiplicand[] mult = [..
                varCaps.Zip(
                    degCaps,
                    (varCap, degCap) =>
                        new TermMultiplicand(
                            varCap.Value,
                            string.IsNullOrEmpty(degCap.Value) ? 1 : int.Parse(LatexUnicode.SuperscriptToNumber(degCap.Value).Replace("^", ""))
                        )
                )
                .Where(x => x.degree != 0)
            ];
            return new(coef, mult);
        }
        else
        {
            throw new Exception($"Failed to parse \"{str}\" to a polynomial term");
        }
    }
    public static bool TryParse(string str, out PolynomialTerm term)
    {
        try
        {
            term = Parse(str);
            return true;
        }
        catch
        {
            term = null;
            return false;
        }
    }

    public int coefficient;
    public TermMultiplicand[] multiplicands;

    public string MultiplicandsToString() => string.Join("*", multiplicands.Where(mult => !mult.IsIdentity));
    public override string ToString() => $"{coefficient}*{MultiplicandsToString()}";

    public override bool Equals(object obj) =>
        obj is PolynomialTerm other &&
        coefficient == other.coefficient &&
        multiplicands.SequenceEqual(other.multiplicands);

    public bool IsZero => coefficient == 0;

    public int Degree() => multiplicands.Length != 0 ? multiplicands.Max(mult => mult.degree) : 0;
    public string[] Variables() => [.. multiplicands.Select(mult => mult.variable).Distinct()];
    public bool IsConstant => multiplicands.Length == 0;

    public static PolynomialTerm operator -(PolynomialTerm rhs) =>
        new(-rhs.coefficient, rhs.multiplicands);

    public static PolynomialTerm operator *(PolynomialTerm lhs, PolynomialTerm rhs) =>
        new(lhs.coefficient * rhs.coefficient, [.. lhs.multiplicands.Concat(rhs.multiplicands)]);

    public static PolynomialTerm operator *(PolynomialTerm lhs, int rhs) =>
        new(lhs.coefficient * rhs, lhs.multiplicands);
    public static PolynomialTerm operator *(int lhs, PolynomialTerm rhs) =>
        rhs * lhs;
}

public class Polynomial : IAlgebraicNotation
{
    public Polynomial(params PolynomialTerm[] terms) =>
        this.terms = [.. terms.Where(term => term.coefficient != 0)];

    public Polynomial(int convertFrom) =>
        terms = [new PolynomialTerm(convertFrom)];

    private static readonly Regex rxTermSeparator = new(@"(?<!^)(?:\+|(?=\-))", RegexOptions.Compiled);
    public static Polynomial Parse(string str) =>
        new([.. rxTermSeparator.Split(str.Replace(" ", "")).Select(PolynomialTerm.Parse).Where(x => x is not null)]);

    public static bool TryParse(string str, out Polynomial poly)
    {
        try
        {
            poly = Parse(str);
            return true;
        }
        catch
        {
            poly = null;
            return false;
        }
    }

    public bool IsConstantMonomial => terms.Length == 1 && terms[0].IsConstant;

    public PolynomialTerm[] terms;

    public override string ToString() =>
        string.Join("+", terms.Select(term => term.ToString())).Replace("+-", "-");

    public override bool Equals(object obj) =>
        obj is Polynomial other &&
        terms.SequenceEqual(other.terms);

    public bool IsInoperable => false;

    public int Degree() => terms.Max(term => term.Degree());
    public string[] Variables() => [.. terms.SelectMany(term => term.Variables()).Distinct()];

    public int LeadingCoefficient() =>
        terms.OrderByDescending(term => term.Degree()).First().coefficient;

    public PolynomialTerm LeadingTerm() =>
        terms.OrderByDescending(term => term.Degree()).First();

    public int ConstantTerm()
    {
        var lastTerm = terms.OrderBy(term => term.Degree()).First();
        return lastTerm.IsConstant ? lastTerm.coefficient : 0;
    }

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
            
            //tex:$$\begin{gather}
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
            //\end{gather}$$

            .Select(term => new PolynomialTerm(
                //tex:$$\begin{bmatrix}
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
                //\end{bmatrix}$$
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
        else if (poly.terms.Length == 0)
        {
            return (Number)0;
        }

        return poly;
    }

    public string AsEquality(string lhs) => $"{lhs}={ToString()}";

    public static Polynomial operator +(Polynomial lhs, Polynomial rhs) =>
        new([.. lhs.terms.Concat(rhs.terms)]);
    public static Polynomial operator +(Polynomial lhs, PolynomialTerm rhs) =>
        new([.. lhs.terms.Append(rhs)]);
    public static Polynomial operator +(Polynomial lhs, int rhs) =>
        new([.. lhs.terms.Append(new(rhs))]);

    public static Polynomial operator -(Polynomial rhs) =>
        new([.. rhs.terms.Select(term => -term)]);

    public static Polynomial operator -(Polynomial lhs, Polynomial rhs) =>
        new([.. lhs.terms.Concat((-rhs).terms)]);

    // FOIL
    public static Polynomial operator *(Polynomial lhs, Polynomial rhs) =>
        new([.. lhs.terms.SelectMany(lterm => rhs.terms.Select(rterm => lterm * rterm))]);
    public static Polynomial operator *(Polynomial lhs, int rhs) =>
        new([.. lhs.terms.Select(term => term * rhs)]);
    public static Polynomial operator *(int lhs, Polynomial rhs) =>
        rhs * lhs;

    public static (Polynomial quotient, Polynomial remainder) SyntheticDivision(Polynomial numer, Polynomial denom)
    {
        if (numer.terms.Length == 0 || denom.terms.Length == 0)
            throw new ArgumentException("Polynomials must have at least one term each to divide");

        if (denom.Degree() != 1)
            throw new ArgumentException($"Cannot synthetic divide by {denom}; must have degree of 1");

        if (denom.LeadingCoefficient() != 1)
            throw new ArgumentException($"Cannot synthetic divide by {denom}; must have leading coefficient of 1");

        if (numer.Variables().Length != 1)
            throw new ArgumentException($"Cannot synthetic divide {numer}; cannot have multiple variables");

        string variable = numer.Variables()[0];

        int constDenom = -denom.ConstantTerm();
        var numerCoefficients = numer.SimplifiedToPolynomial()
            .terms.ToDictionary(term => term.Degree(), term => term.coefficient);

        Polynomial quotient = new();

        int remainder = 0;
        for (int degree = numer.Degree(); degree > 0; --degree)
        {
            remainder += numerCoefficients.GetValueOrDefault(degree, 0);
            quotient += new PolynomialTerm(remainder, new TermMultiplicand(variable, degree - 1));
            remainder *= constDenom;
        }
        remainder += numerCoefficients.GetValueOrDefault(0, 0);

        return (quotient, new(remainder));
    }

    public static (Polynomial quotient, Polynomial remainder) LongDivision(Polynomial numer, Polynomial denom)
    {
        if (numer.terms.Length == 0 || denom.terms.Length == 0)
            throw new ArgumentException("Polynomials must have at least one term each to divide");

        numer = numer.SimplifiedToPolynomial();
        denom = denom.SimplifiedToPolynomial();

        var numerLeadingTerm = numer.LeadingTerm();
        var denomLeadingTerm = denom.LeadingTerm();

        if (numerLeadingTerm.multiplicands.Length > 1)
            throw new NotImplementedException("Cannot divide with multiple variables"); // todo

        if (numerLeadingTerm.multiplicands.Length > 0 &&
            denomLeadingTerm.multiplicands.Length > 0 &&
            numerLeadingTerm.multiplicands[0].variable != denomLeadingTerm.multiplicands[0].variable)
        {
            throw new NotImplementedException("Must have same variable(s) to divide"); // todo
        }

        int numerDeg = numerLeadingTerm.Degree();
        int denomDeg = denomLeadingTerm.Degree();

        int degreeDiff = numerDeg - denomDeg;
        if (degreeDiff < 0)
            throw new NotImplementedException("Numerator degree cannot be less than denominator degree");

        int numerLeadCoef = numerLeadingTerm.coefficient;
        int denomLeadCoef = denomLeadingTerm.coefficient;

        if (degreeDiff == 0 && numerLeadCoef % denomLeadCoef != 0)
        {
            return (new(0), numer);
        }

         var quotient = new Polynomial(
            new PolynomialTerm(
                numerLeadCoef / denomLeadCoef,
                [new(numerLeadingTerm.multiplicands[0].variable, degreeDiff)]
            )
        ).SimplifiedToPolynomial();

        if (quotient.terms.Length == 0)
        {
            return (new(0), numer);
        }

        var remainder = (numer - denom * quotient).SimplifiedToPolynomial();

        if (remainder.terms.Length == 0)
        {
            return (quotient, new(0));
        }
        else if (remainder.Degree() == 0)
        {
            return (quotient, new Polynomial(remainder.ConstantTerm()));
        }
        else if (numer == remainder)
        {
            return (new(0), remainder);
        }
        else
        {
            var (subQuotient, subRemainder) = LongDivision(remainder, denom); // Recursive
            return (quotient + subQuotient, subRemainder);
        }
    }

    // Polynomial Long Division
    public static (Polynomial quotient, Polynomial remainder) operator /(Polynomial numer, Polynomial denom)
    {
        var denomLeadingTerm = denom.LeadingTerm();
        return (denomLeadingTerm.Degree() == 1 && denomLeadingTerm.coefficient == 1)
            ? SyntheticDivision(numer, denom)
            : LongDivision(numer, denom);
    }
}
