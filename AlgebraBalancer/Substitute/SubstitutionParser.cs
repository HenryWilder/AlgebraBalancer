using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public static class SubstitutionParser
{
    private static readonly Regex rxAnd =
        new(@"\s*(?:\sand\s|;)\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxBe =
        new(@"(?'var'.+)\s+(?:(be(?:ing)?|is)|:?=|≔)\s+(?'val'.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxLet =
        new(@"^let\s+('statement'.+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxWith =
        new(@"(?<=.+)\s+(?:with|for|using|where|in which|given(?: that)?)\s+(?'statement'.+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static List<ISubstitutible> Parse(string document, int startOfLine, int endOfLine)
    {
        List<ISubstitutible> substitutions = [];

        List<(string variable, string value)> items = [];

        string[] lines = document
            .Substring(0, Math.Max(startOfLine - 1, 0))
            .Split("\r");

        static IEnumerable<(string, string)> GetStatementClauses(string statement)
        {
            string[] clauses = rxAnd.Split(statement);
            return clauses
                .Select(clause =>
                {
                    var match = rxBe.Match(clause);
                    return match.Success
                        ? (match.Groups["var"].Value, match.Groups["val"].Value)
                        : (null, null);
                })
                .Where(clause => clause != (null, null));
        }

        foreach (string line in lines.Reverse())
        {
            var letStatement = rxLet.Match(line);
            if (letStatement.Success)
            {
                items.AddRange(GetStatementClauses(letStatement.Groups["statement"].Value));
            }
        }

        string lineBeingOperated = document.Substring(startOfLine, endOfLine - startOfLine);
        var withStatement = rxWith.Match(lineBeingOperated);
        if (withStatement.Success)
        {
            items.AddRange(GetStatementClauses(withStatement.Groups["statement"].Value));
        }

        return substitutions;
    }

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

    private static readonly Regex rxBracket = new(@"[(){}\[\]]");

    private static readonly Regex rxContainedSemicolons =
        new(@"(?<=^(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*(?(open)(?!)));",
            RegexOptions.Compiled);
}
