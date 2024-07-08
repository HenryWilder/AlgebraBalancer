using System.Text.RegularExpressions;

namespace AlgebraBalancer.Substitute;
public class ParenCleaner
{
    private static readonly Regex rxVectorTest = new(
        @"^\((?:[^()]|(?'a'\()|(?'-a'\)))*(?(a)(?!))(,(?:[^()]|(?'b'\()|(?'-b'\)))*(?(b)(?!)))+\)$",
        RegexOptions.Compiled);

    private static readonly Regex rxUnneededOuterParens = new(
        @"\(\s*(\((?:[^()]|(?'inner'\()|(?'-inner'\)))*(?(inner)(?!))\))\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex rxUnneededFullParens = new(
        @"^\s*\(((?:[^()]|(?'inner'\()|(?'-inner'\)))*(?(inner)(?!)))\)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex rxUnnededTermParentheses = new(
        @"(?<=^|[-+/*%=|<>({\[])\s*\(\s*(\w+)\s*\)\s*(?=[-+/*%=|<>)}\]]|$)",
        RegexOptions.Compiled);

    public static string CleanParentheses(string expr)
    {
        while (rxUnneededOuterParens.IsMatch(expr))
        {
            expr = rxUnneededOuterParens.Replace(expr, (x) => x.Groups[1].Value);
        }
        while (rxUnneededFullParens.IsMatch(expr))
        {
            bool hasNonVector = false;
            expr = rxUnneededFullParens.Replace(expr, (x) => {
                string outer = x.Groups[0].Value;
                if (!rxVectorTest.IsMatch(outer))
                {
                    hasNonVector = true;
                    return x.Groups[1].Value;
                }
                else
                {
                    return outer;
                }
            });
            if (!hasNonVector) break;
        }
        expr = rxUnnededTermParentheses.Replace(expr, (x) => x.Groups[1].Value);
        return expr;
    }
}
