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

    public override int GetHashCode() => base.GetHashCode();

    public bool IsIdentity => degree == 0;

    public class ByDescendingDegree
        : IComparer<TermMultiplicand>
    {
        public int Compare(TermMultiplicand x, TermMultiplicand y) =>
            y.degree.CompareTo(x.degree);
    }
}

public class PolynomialTerm
{
    public PolynomialTerm(int coefficient, SortedSet<TermMultiplicand> multiplicands) =>
        (this.coefficient, this.multiplicands) = (coefficient, multiplicands);

    public PolynomialTerm(SortedSet<TermMultiplicand> multiplicands) =>
        (coefficient, this.multiplicands) = (1, multiplicands);

    public PolynomialTerm(int coefficient, params TermMultiplicand[] multiplicands) =>
        (this.coefficient, this.multiplicands) = (
            coefficient,
            new SortedSet<TermMultiplicand>(
                multiplicands.Where(mult => mult.degree != 0),
                new TermMultiplicand.ByDescendingDegree()
            )
        );

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
    public SortedSet<TermMultiplicand> multiplicands;

    public string MultiplicandsToString() => string.Join("*", multiplicands.Where(mult => !mult.IsIdentity));
    public override string ToString()
    {
        string multStr = MultiplicandsToString();
        return string.IsNullOrWhiteSpace(multStr)
            ? $"{coefficient}"
            : $"{coefficient}*{MultiplicandsToString()}";
    }

    public override bool Equals(object obj) =>
        obj is PolynomialTerm other &&
        coefficient == other.coefficient &&
        multiplicands.SequenceEqual(other.multiplicands);

    public override int GetHashCode() => base.GetHashCode();

    public class ByDescendingDegree
        : IComparer<PolynomialTerm>
    {
        public int Compare(PolynomialTerm x, PolynomialTerm y) =>
            y.Degree.CompareTo(x.Degree);
    }

    public bool IsZero => coefficient == 0;

    public int Degree => multiplicands.Count == 0 ? 0 : multiplicands.First().degree;
    public string[] Variables() => [.. multiplicands.Select(mult => mult.variable).Distinct()];

    public bool IsConstant => multiplicands.Count == 0;

    public static PolynomialTerm operator -(PolynomialTerm rhs) =>
        new(-rhs.coefficient, rhs.multiplicands);
    public static PolynomialTerm operator *(PolynomialTerm lhs, int rhs) =>
        new(lhs.coefficient * rhs, lhs.multiplicands);
    public static PolynomialTerm operator *(int lhs, PolynomialTerm rhs) =>
        rhs * lhs;

    public static PolynomialTerm operator *(PolynomialTerm lhs, PolynomialTerm rhs) =>
        new(
            lhs.coefficient * rhs.coefficient,
            SimplifiedMultiplicands(lhs.multiplicands.Union(rhs.multiplicands))
        );

    private static TermMultiplicand[] SimplifiedMultiplicands(IEnumerable<TermMultiplicand> multiplicands) =>
        [.. multiplicands
            .GroupBy(mult => mult.variable)
            .Select(group => new TermMultiplicand(
                group.Key,
                group.Sum(mult => mult.degree)
            ))
            .Where(mult => !mult.IsIdentity)
        ];

    public PolynomialTerm SimplifiedToTerm() =>
        new(coefficient, SimplifiedMultiplicands(multiplicands));
}

public class Polynomial
    : IAlgebraicNotation
{
    public Polynomial(params PolynomialTerm[] terms) =>
        this.terms = new SortedSet<PolynomialTerm>(
            terms.Where(term => term.coefficient != 0),
            new PolynomialTerm.ByDescendingDegree()
        );

    public Polynomial(int convertFrom) =>
        terms = new SortedSet<PolynomialTerm>(
            [new PolynomialTerm(convertFrom)],
            new PolynomialTerm.ByDescendingDegree()
        );

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

    public bool IsConstantMonomial => terms.Count == 1 && terms.First().IsConstant;

    public SortedSet<PolynomialTerm> terms;

    public override string ToString() =>
        string.Join("+", terms.Select(term => term.ToString())).Replace("+-", "-");

    public override bool Equals(object obj) =>
        obj is Polynomial other &&
        terms.SequenceEqual(other.terms);

    public override int GetHashCode() => base.GetHashCode();

    public bool IsInoperable => false;

    public PolynomialTerm LeadingTerm => terms.First();

    public int Degree => terms.Count == 0 ? 0 : LeadingTerm.Degree;

    public string[] Variables() =>
        [.. terms.SelectMany(term => term.Variables()).Distinct()];

    public int LeadingCoefficient => LeadingTerm.coefficient;

    public int ConstantTerm()
    {
        if (terms.Count == 0) return 0;
        var lastTerm = terms.Last();
        return lastTerm.IsConstant ? lastTerm.coefficient : 0;
    }

    public bool TryMonomial(out PolynomialTerm term)
    {
        bool isMonomial = terms.Count == 1;
        term = isMonomial ? terms.First() : null;
        return isMonomial;
    }

    public bool TryBinomial(out PolynomialTerm term1, out PolynomialTerm term2)
    {
        bool isBinomial = terms.Count == 2;
        (term1, term2) = isBinomial ? (terms.First(), terms.Skip(1).First()) : (null, null);
        return isBinomial;
    }

    public bool TryTrinomial(out PolynomialTerm term1, out PolynomialTerm term2, out PolynomialTerm term3)
    {
        bool isTrinomial = terms.Count == 3;
        (term1, term2, term3) = isTrinomial ? (terms.First(), terms.Skip(1).First(), terms.Skip(2).First()) : (null, null, null);
        return isTrinomial;
    }

    public Polynomial SimplifiedToPolynomial()
    {
        return new Polynomial([.. terms
            .Select(term => term.SimplifiedToTerm())
            .GroupBy(term => term.MultiplicandsToString())
            .Select(group => new PolynomialTerm(
                group.Sum(term => term.coefficient),
                group.First().multiplicands
            ))
            .Where(term => !term.IsZero)
        ]);
    }

    public IAlgebraicNotation Simplified()
    {
        var poly = SimplifiedToPolynomial();

        if (poly.TryMonomial(out var term) && term.IsConstant)
        {
            return (Number)term.coefficient;
        }
        else if (poly.terms.Count == 0)
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
        if (numer.terms.Count == 0 || denom.terms.Count == 0)
            throw new ArgumentException("Polynomials must have at least one term each to divide");

        if (denom.Degree != 1)
            throw new ArgumentException($"Cannot synthetic divide by {denom}; must have degree of 1");

        if (denom.LeadingCoefficient != 1)
            throw new NotImplementedException($"Cannot synthetic divide by {denom}; must have leading coefficient of 1"); // todo

        if (numer.Variables().Length != 1)
            throw new NotImplementedException($"Cannot synthetic divide {numer}; cannot have multiple variables"); // todo

        string variable = numer.Variables()[0];

        int constDenom = -denom.ConstantTerm();
        var numerCoefficients = numer.SimplifiedToPolynomial()
            .terms.ToDictionary(term => term.Degree, term => term.coefficient);

        Polynomial quotient = new();

        int remainder = 0;
        for (int degree = numer.Degree; degree > 0; --degree)
        {
            remainder += numerCoefficients.GetValueOrDefault(degree, 0);
            quotient += new PolynomialTerm(remainder, new TermMultiplicand(variable, degree - 1));
            remainder *= constDenom;
        }
        remainder += numerCoefficients.GetValueOrDefault(0, 0);

        return (quotient, new(remainder));
    }

    public static (Polynomial quotient, Polynomial remainder) LongDivision(Polynomial numerator, Polynomial denominator)
    {
        if (numerator.terms.Count == 0 || denominator.terms.Count == 0)
            throw new ArgumentException("Polynomials must have at least one term each to divide");

        numerator   =   numerator.SimplifiedToPolynomial();
        denominator = denominator.SimplifiedToPolynomial();

        var   numeratorLeadingTerm =   numerator.LeadingTerm;
        var denominatorLeadingTerm = denominator.LeadingTerm;

        if (numeratorLeadingTerm.multiplicands.Count > 1)
            throw new NotImplementedException("Cannot divide with multiple variables"); // todo

        if (numeratorLeadingTerm.multiplicands.Count > 0 &&
            denominatorLeadingTerm.multiplicands.Count > 0 &&
            numeratorLeadingTerm.multiplicands.First().variable != denominatorLeadingTerm.multiplicands.First().variable)
        {
            throw new NotImplementedException("Must have same variable(s) to divide"); // todo
        }

        int numeratorDegree   =   numeratorLeadingTerm.Degree;
        int denominatorDegree = denominatorLeadingTerm.Degree;

        int degreeDifference = numeratorDegree - denominatorDegree;
        if (degreeDifference < 0)
            throw new NotImplementedException("Numerator degree cannot be less than denominator degree");

        int numeratorLeadingCoefficient   =   numeratorLeadingTerm.coefficient;
        int denominatorLeadingCoefficient = denominatorLeadingTerm.coefficient;

        if (degreeDifference == 0 && numeratorLeadingCoefficient % denominatorLeadingCoefficient != 0)
        {
            return (new(0), numerator);
        }

         var quotient = new Polynomial(
            new PolynomialTerm(
                numeratorLeadingCoefficient / denominatorLeadingCoefficient,
                (TermMultiplicand[])[new(numeratorLeadingTerm.multiplicands.First().variable, degreeDifference)]
            )
        ).SimplifiedToPolynomial();

        if (quotient.terms.Count == 0)
        {
            return (new(0), numerator);
        }

        var remainder = (numerator - denominator * quotient).SimplifiedToPolynomial();

        // Degree() is 0 if there are no terms (because max(∅)=0)
        if (remainder.Degree < denominator.Degree)
        {
            // ConstantTerm() is 0 if no constant term is stated explicitly
            return (quotient, new Polynomial(remainder.ConstantTerm()));
        }
        else
        {
            var (subQuotient, subRemainder) = LongDivision(remainder, denominator); // Recursive
            return (quotient + subQuotient, subRemainder);
        }
    }

    // Polynomial Long Division
    public static (Polynomial quotient, Polynomial remainder) operator /(Polynomial numer, Polynomial denom)
    {
        var denomLeadingTerm = denom.LeadingTerm;
        return (denomLeadingTerm.Degree == 1 && denomLeadingTerm.coefficient == 1)
            ? SyntheticDivision(numer, denom)
            : LongDivision(numer, denom);
    }
}
