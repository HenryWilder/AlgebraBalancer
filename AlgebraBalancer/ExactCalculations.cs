using System;
using System.Collections.Generic;
using System.Linq;

using AlgebraBalancer.Algebra;
using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.Bald;

namespace AlgebraBalancer;
internal class ExactCalculations
{
    private static List<(string name, string value)> Calculations(int x)
    {
        List<(string name, string value)> result = [];

        // Categorization
        {
            string posOrNeg = x > 0 ? "⁺" : x < 0 ? "⁻" : "";
            if (x > 0 && ExactMath.IsPrime(x))
            {
                result.Add((null, $"{x}∈ℙ"));
            }
            else
            {
                string oddOrEven = ExactMath.IsOdd(x) ? "+1" : "";
                result.Add((null, $"{x}∈2ℤ{posOrNeg}{oddOrEven}"));
            }
        }

        // Prime Factors
        var pfac = ExactMath.PrimeFactors(x);
        result.Add((null, $"{x}=" + string.Join("*", pfac.Select(p =>
            $"{p.prime}{LatexUnicode.ToSuperscript(p.exponent.ToString())}"
        ))));

        // Root
        var root = new Radical(x).Simplified();
        string middlestep;
        if (root is Radical radical && radical.coefficient != 1 && radical.radicand != 1)
        {
            middlestep = $"=(√{radical.coefficient * radical.coefficient})√{radical.radicand}";
        }
        else
        {
            middlestep = "";
        }
        result.Add((null, root.AsEquality($"√{x}" + middlestep)));

        // Square
        result.Add((null, ExactMath.Power(x, 2).AsEquality($"{x}²")));

        // Cube
        result.Add((null, ExactMath.Power(x, 3).AsEquality($"{x}³")));

        // Factors
        {
            List<(string a, string b, string sum, string diff)>
                factorStrings = ExactMath.Factors(x)
                    .Select((f) => ($"{f.common}", $"{f.associated}", $"{f.common + f.associated}", $"{f.associated - f.common}"))
                    .ToList();

            int aPad = factorStrings.Max((f) => f.a.Length);
            int bPad = factorStrings.Max((f) => f.b.Length);

            result.Add(("Factors:\n", string.Join("\n", factorStrings.Select((x) => $"{x.a.PadLeft(aPad)},{x.b.PadLeft(bPad)}"))));
        }

        return result;
    }

    private static List<(string name, string value)> Calculations(int a, int b)
    {
        List<(string name, string value)> result = [];

        {
            string inequality = a > b ? ">" : a < b ? "<" : "=";
            result.Add((null, $"{a}{inequality}{b}"));
        }

        result.Add((null, ExactMath.Sum(a, b).AsEquality($"{a}+{b}")));
        result.Add((null, ExactMath.Sum(a, -b).AsEquality($"{a}-{b}")));
        result.Add((null, ExactMath.Product(a, b).AsEquality($"{a}*{b}")));
        result.Add((null, new Fraction(a, b).Simplified().AsEquality($"{a}/{b}")));
        result.Add((null, (b != 0 ? new Number(a % b) : UNDEFINED as IAlgebraicAtomic).AsEquality($"{a}%{b}")));
        result.Add((null, ExactMath.Power(a, b).AsEquality($"{a}{LatexUnicode.ToSuperscript(b.ToString())}")));


        result.Add(($"GCF({a},{b})", $"={ExactMath.GCF(a, b)}"));
        result.Add(($"LCM({a},{b})", $"={ExactMath.LCM(a, b)}"));

        // Common factors
        {
            List<(string common, string a, string b)>
                factorStrings = ExactMath.CommonFactors(a, b)
                    .Select((f) => ($"{f.common}", $"{f.associated[0]}", $"{f.associated[1]}"))
                    .ToList();

            int commonPad = factorStrings.Max((f) => f.common.Length);
            int aPad = factorStrings.Max((f) => f.a.Length);
            int bPad = factorStrings.Max((f) => f.b.Length);

            result.Add(("Common Factors:\n", string.Join("\n",
                factorStrings.Select((x) => $"{x.common.PadLeft(commonPad)},({x.a.PadLeft(aPad)},{x.b.PadLeft(bPad)})"))));
        }

        return result;
    }

    private static List<(string name, string value)> Calculations(int a, int b, int c)
    {
        List<(string name, string value)> result = [];

        var a2 = ExactMath.Power(a, 2);
        var b2 = ExactMath.Power(b, 2);
        var c2 = ExactMath.Power(c, 2);
        if (a2 is Number a2Num && b2 is Number b2Num && c2 is Number c2Num)
        {
            var magnitude = new Radical(a2Num + b2Num + c2Num).Simplified();
            result.Add((null, magnitude.AsEquality("|A|")));
            {
                IAlgebraicNotation aPart, bPart, cPart;
                if (magnitude is Radical radMag)
                {
                    aPart = new RadicalFraction(a, radMag).Simplified();
                    bPart = new RadicalFraction(b, radMag).Simplified();
                    cPart = new RadicalFraction(c, radMag).Simplified();
                }
                else if (magnitude is Number numMag)
                {
                    aPart = new Fraction(a, numMag).Simplified();
                    bPart = new Fraction(b, numMag).Simplified();
                    cPart = new Fraction(c, numMag).Simplified();
                }
                else
                {
                    throw new NotImplementedException();
                }
                result.Add((null, $"Â=({aPart},{bPart},{cPart})"));
            }
        }
        else
        {
            result.Add((null, $"|A|=?"));
            result.Add((null, $"Â=({a}, {b}, {c})(?⁻¹)"));
        }
        result.Add((null, $"Σ({a},{b},{c})={a + b + c}"));
        result.Add((null, $"∏({a},{b},{c})={a * b * c}"));

        result.Add(($"GCF({a},{b},{c}): ", $"{ExactMath.GCF(a, b, c)}"));

        result.Add(($"LCM({a},{b},{c}): ", $"{ExactMath.LCM(a, b, c)}"));

        // Quadratic
        {
            string stepsStr =
                $"  {a}𝑥²+{b}𝑥+{c}=0 =>\n" +
                $"  𝑥 = (-𝑏±√(𝑏²-4𝑎𝑐))/(2𝑎)\n" +
                $"    = (-({b})±√(({b})²-4({a})({c})))/(2({a}))\n" +
                $"    = ({-b}±√({b * b}+{-(4 * a * c)}))/({2 * a})\n";

            try
            {
                var solutions = ExactMath.Quadratic(a, b, c);
                stepsStr += $"    = {solutions}";
            }
            catch (Exception ex)
            {
                stepsStr += $"    <{ex.Message}>";
            }

            result.Add(("Quadratic:\n", stepsStr));
        }

        // Vertex form
        {
            string stepsStr =
                $"  𝑦 = ({a}𝑥²+{b}𝑥)+{c}\n" +
                $"    = 𝑎(𝑥²+(𝑏/𝑎)𝑥+𝑏/(2𝑎))+𝑐-𝑏²/(4𝑎)\n" +
                $"    = 𝑎(𝑥+𝑏/(2𝑎))²+𝑐-𝑏²/(4𝑎)\n" +
                $"    = ({a})(𝑥+({b})/(2({a})))²+({c})-({b})²/(4({a}))\n";

            try
            {
                var vertexForm = ExactMath.CompleteSquare(a, b, c);
                stepsStr +=
                    $"    = {vertexForm}\n" +
                    $"  ℎ = -𝑏/(2𝑎)\n" +
                    $"    = -({b})/(2({a}))\n" +
                    $"    = {vertexForm.h}\n" +
                    $"  𝑘 = (4𝑎𝑐-𝑏²)/(4𝑎)\n" +
                    $"    = (4({a})({c})-({b})²)/(4({a}))\n" +
                    $"    = {vertexForm.k}";
            }
            catch (Exception ex)
            {
                stepsStr += $"    <{ex.Message}>";
            }

            result.Add(("Vertex form (complete square):\n", stepsStr));
        }

        return result;
    }

    public static List<(string name, string value)> Calculations(List<int> parameters) =>
        parameters.Count switch {
            1 => Calculations(parameters[0]),
            2 => Calculations(parameters[0], parameters[1]),
            3 => Calculations(parameters[0], parameters[1], parameters[2]),
            _ => [(null, "...")],
        };
}
