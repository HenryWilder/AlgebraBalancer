using System;
using System.Collections.Generic;
using System.Linq;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.Bald;

namespace AlgebraBalancer;
internal class ExactCalculations
{
    private static List<string> Calculations(int x)
    {
        List<string> result = [];

        // Categorization
        {
            string posOrNeg = x > 0 ? "⁺" : x < 0 ? "⁻" : "";
            if (x > 0 && ExactMath.IsPrime(x))
            {
                result.Add($"{x} ∈ ℙ");
            }
            else
            {
                string oddOrEven = ExactMath.IsOdd(x) ? "+1" : "";
                result.Add($"{x} ∈ 2ℤ{posOrNeg}{oddOrEven}");
            }
        }

        // Prime Factors
        var pfac = ExactMath.PrimeFactors(x);
        result.Add($"{x} = " + string.Join(" × ", pfac.Select(p =>
            p.prime.ToString() + (p.exponent == 1 ? "" : LatexUnicode.ToSuperscript(p.exponent.ToString()))
        )));

        // Root
        var root = new Algebra.Radical(x).Simplified();
        string middlestep;
        if (root is Algebra.Radical radical && radical.coefficient != 1 && radical.radicand != 1)
        {
            middlestep = $" = (√{radical.coefficient * radical.coefficient})√{radical.radicand}";
        }
        else
        {
            middlestep = "";
        }
        result.Add(root.AsEquality($"√{x}" + middlestep));

        // Square
        result.Add(ExactMath.Power(x, 2).AsEquality($"{x}²"));

        // Cube
        result.Add(ExactMath.Power(x, 3).AsEquality($"{x}³"));

        // Factors
        {
            List<(string a, string b, string sum, string diff)>
                factorStrings = ExactMath.Factors(x)
                    .Select((f) => ($"{f.common}", $"{f.associated}", $"{f.common + f.associated}", $"{f.associated - f.common}"))
                    .ToList();

            int aPad = factorStrings.Max((f) => f.a.Length);
            int bPad = factorStrings.Max((f) => f.b.Length);

            result.Add("Factors:\n" + string.Join("\n", factorStrings.Select((x) => $"{x.a.PadLeft(aPad)} × {x.b.PadLeft(bPad)}")));
        }

        return result;
    }

    private static List<string> Calculations(int a, int b)
    {
        List<string> result = [];

        {
            string inequality = a > b ? ">" : a < b ? "<" : "=";
            result.Add($"{a} {inequality} {b}");
        }

        result.Add(ExactMath.Sum(a, b).AsEquality($"{a} + {b}"));
        result.Add(ExactMath.Sum(a, -b).AsEquality($"{a} - {b}"));
        result.Add(ExactMath.Product(a, b).AsEquality($"{a} × {b}"));
        result.Add(new Algebra.Fraction(a, b).Simplified().AsEquality($"{a} ÷ {b}"));
        result.Add((b != 0 ? new Number(a % b) : UNDEFINED as IAlgebraicAtomic).AsEquality($"{a} % {b}"));
        result.Add(ExactMath.Power(a, b).AsEquality($"{a}{LatexUnicode.ToSuperscript(b.ToString())}"));


        result.Add($"GCF({a}, {b}) = {ExactMath.GCF(a, b)}");
        result.Add($"LCM({a}, {b}) = {ExactMath.LCM(a, b)}");

        // Common factors
        {
            List<(string common, string a, string b)>
                factorStrings = ExactMath.CommonFactors(a, b)
                    .Select((f) => ($"{f.common}", $"{f.associated[0]}", $"{f.associated[1]}"))
                    .ToList();

            int commonPad = factorStrings.Max((f) => f.common.Length);
            int aPad = factorStrings.Max((f) => f.a.Length);
            int bPad = factorStrings.Max((f) => f.b.Length);

            result.Add("Common Factors:\n" + string.Join("\n",
                factorStrings.Select((x) => $"{x.common.PadLeft(commonPad)} × ({x.a.PadLeft(aPad)}, {x.b.PadLeft(bPad)})")));
        }

        return result;
    }

    private static List<string> Calculations(int a, int b, int c)
    {
        List<string> result = [];

        var a2 = ExactMath.Power(a, 2);
        var b2 = ExactMath.Power(b, 2);
        var c2 = ExactMath.Power(c, 2);
        if (a2 is Number a2Num && b2 is Number b2Num && c2 is Number c2Num)
        {
            var magnitude = new Algebra.Radical(a2Num + b2Num + c2Num).Simplified();
            result.Add(magnitude.AsEquality("|A|"));
            {
                IAlgebraicNotation aPart, bPart, cPart;
                if (magnitude is Algebra.Radical radMag)
                {
                    aPart = new Algebra.RadicalFraction(a, radMag).Simplified();
                    bPart = new Algebra.RadicalFraction(b, radMag).Simplified();
                    cPart = new Algebra.RadicalFraction(c, radMag).Simplified();
                }
                else if (magnitude is Number numMag)
                {
                    aPart = new Algebra.Fraction(a, numMag).Simplified();
                    bPart = new Algebra.Fraction(b, numMag).Simplified();
                    cPart = new Algebra.Fraction(c, numMag).Simplified();
                }
                else if (magnitude is IAlgebraicAtomic atomMag)
                {
                    aPart = new Algebra.Fraction(new Number(a), atomMag).Simplified();
                    bPart = new Algebra.Fraction(new Number(b), atomMag).Simplified();
                    cPart = new Algebra.Fraction(new Number(c), atomMag).Simplified();
                }
                else
                {
                    throw new NotImplementedException();
                }
                result.Add($"Â = ({aPart}, {bPart}, {cPart})");
            }
        }
        else
        {
            result.Add($"|A| = ?");
            result.Add($"Â = ({a}, {b}, {c})(?⁻¹)");
        }
        result.Add($"Σ({a}, {b}, {c}) = {a + b + c}");
        result.Add($"∏({a}, {b}, {c}) = {a * b * c}");

        result.Add($"GCF: {ExactMath.GCF(a, b, c)}");

        result.Add($"LCM: {ExactMath.LCM(a, b, c)}");

        // Quadratic
        {
            var formula = new Algebra.RadicalFraction(-b, new Algebra.Radical(b * b - 4 * a * c), 2 * a);
            string unsimplified = formula.ToString();
            string simplified = formula.Simplified().ToString();

            result.Add($"{a}𝑥² + {b}𝑥 + {c} = 0 => 𝑥 =\n  (-({b})±√(({b})²-4({a})({c})))/2({a})\n  {unsimplified}" +
                ((simplified != unsimplified) ? $"\n  {simplified}" : ""));
        }

        return result;
    }

    public static List<string> Calculations(List<int> parameters) =>
        parameters.Count switch {
            1 => Calculations(parameters[0]),
            2 => Calculations(parameters[0], parameters[1]),
            3 => Calculations(parameters[0], parameters[1], parameters[2]),
            _ => ["..."],
        };
}
