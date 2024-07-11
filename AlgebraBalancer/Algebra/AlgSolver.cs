using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlgebraBalancer.Algebra;
public static class AlgSolver
{
    public readonly static Regex rxImpliedMul =
        new(@"(?x)
            # Left side
            (?<=(?'lparen'\))|\d)

            # Optional whitespace between (can't be linebreaks)
            [\s-[\n\r]]*

            # Right side
            # If the left side didn't have parentheses, this side MUST
            # It can have parentheses either way, though
            (?=(?(lparen)[\d(]|\())
            (?-x)",
            RegexOptions.Compiled);

    public static string CleanExpr(string expr) =>
        rxImpliedMul.Replace(LatexUnicode.SuperscriptToNumber(expr), "*")
            .Replace("×", "*")
            .Replace(" ", "");

    public static readonly Regex rxTerm =
        new(@"(?x)
            (?'sign'
                # Minus needs to be a sign and not an operator, so it must either be at the
                # start of a(n) (sub)expression or following an operator.
                # Whitespace is not a concern because it is already removed by the function.
                (?<=^|[-+/*(])
                -
            )?

            (?'coef' # coefficient
                \d+
            )? # Negative with no number represents coefficient of -1

            (?'imag' # imaginary
                i|𝑖|ⅈ
            )?

            (?:√ # Don't need to capture the radical, it just marks where the radicand is
                (?'radi' # radicand
                    -? # Minus here is always a sign because it is necessarily preceeded by the radical
                    \d+
                )
            )?

            # Deny any match that doesn't have a coefficient, imaginary, nor radical.
            # A match is required to have AT LEAST ONE of these.
            # Sign is optional.
            (?(coef)|(?(imag)|(?(radi)|(?!))))
            (?-x)",
            RegexOptions.Compiled);

    // language=regex
    private const string RX_ALGEBRAIC =
        @"(?x)
        (?: # numerator
            (?'numerOpenParen'\()?
                (?'numerTerms'{rxTerm}) # First term doesn't need parentheses or operators
                (?(numerOpenParen) # Only allow multiple terms if there are parentheses
                    (?:
                        # Must have an addition/subtraction operator between terms
                        # A negative term doesn't need an addition operator in front of it
                        (?=[-+])[-+]?
                        (?'numerTerms'{rxTerm})
                    )*
                )
            # Require a closing parenthes if there was an opening one, deny match if there wasn't one.
            (?(numerOpenParen)\))
        )
        (?:
            /
            (?'denom' # denominator
                -? # Minus here is always a sign because it is necessarily preceeded by the division
                \d+ # If the denominator is not an integer, it is not a singular Algebraic.
                    # This Regex is specifically for singular Algebraics, not expressions.
            )
        )? # Doesn't need to be a fraction
        (?-x)";

    public static readonly Regex rxAlgebraic =
        new(RX_ALGEBRAIC.Replace("{rxTerm}", $"(?:{rxTerm})"),
            RegexOptions.Compiled);

    // Expressions matching this Regex are incompatible with DataTable.
    public static readonly Regex rxNeedsAlgebraic = new(@"i|𝑖|ⅈ|√");

    private static readonly Regex rxSubExpression =
        new(@"(?x)
            # A subexpression must be enclosed within parentheses
            \(
                (?'expr' # The subexpression itself
                    # Any parentheses inside must be balanced
                    (?:[^()]|(?'open'\()|(?'-open'\)))+(?(open)(?!))
                )
            \)
            (?-x)",
            RegexOptions.Compiled);

    private static readonly char[][] operatorPrecedences = [['*', '/'], ['+', '-']];

    public static Algebraic SolveAlgebraic(string expr)
    {
        expr = CleanExpr(expr);
        // Replace subexpressions with their solutions first
        expr = rxSubExpression.Replace(expr, match =>
        {
            string matchStr = match.Value;
            // Don't replace if the subexpression is a single algebraic on its own
            if (rxAlgebraic.IsMatch(matchStr)) return matchStr;

            var solution = SolveAlgebraic(match.Groups["expr"].Value).Simplified();
            string solutionStr = solution.ToString();

            // Needs to be wrapped in parentheses
            if (
                (solution is SumOfRadicals sum && sum.terms.Length > 1) ||
                (solution is Algebraic alg && alg.numerator.terms.Length > 1 && alg.denominator == 1)
            )
            {
                return $"({solutionStr})";
            }

            return solution.ToString();
        }
        );

        var algebraics = rxAlgebraic.Matches(expr).Select(match =>
        {
            Radical[] numerTerms = [..
                match.Groups["numerTerms"].Captures.Select(capture =>
                {
                    // TODO: This doesn't handle subtracting negatives

                    // I know it's inefficient to match *again*, but I can't get subgroups from captures :c
                    var term = rxTerm.Match(capture.Value);
                    if (!term.Success) throw new Exception($"Capture \"{capture.Value}\": Should be match if within capture");

                    var sign = term.Groups["sign"];
                    var coef = term.Groups["coef"];
                    var imag = term.Groups["imag"];
                    var radi = term.Groups["radi"];

                    int coefficient =
                        (sign.Success ? -1 : 1) * (coef.Success ? int.Parse(coef.Value) : 1);

                    int radicand =
                        (imag.Success ? -1 : 1) * (radi.Success ? int.Parse(radi.Value) : 1);

                    var radical = new Radical(coefficient, radicand);
                    return radical;
                })
            ];
            if (numerTerms.Length == 0) throw new Exception($"Match \"{match.Value}\": Numerator can't be empty");
            var denom = match.Groups["denom"];
            int denominator = denom.Success ? int.Parse(denom.Value) : 1;
            var numerator = new SumOfRadicals(numerTerms);
            var algebraic = new Algebraic(numerator, denominator);
            return algebraic;
        }).ToList();

        // I know. Performing the same regex to the same string AGAIN. SUPER inefficient.
        // I'll fix it later.
        string operations = rxAlgebraic.Replace(expr, "");


        // By this point in the function, our string should contain no subexpressions.
        // The index of an operator that is not part of an algebraic should itself be
        // the index (into the algebraics list) of its left hand argument.

        // By replacing the left & right arguments inside the list with their solution,
        // we should be able to keep this assumption true.

        // This function has relatively detailed exceptions because the user sees them.

        foreach (char[] precedence in operatorPrecedences)
        {
            while (operations.IndexOfAny(precedence) is int nextOpIndex and not -1)
            {
                if (algebraics.Count == 0)
                {
                    throw new Exception($"Expression \"{expr}\": No values to operate on.");
                }

                char op = operations[nextOpIndex];
                if (nextOpIndex >= algebraics.Count)
                {
                    var prevValue = algebraics[Math.Min(nextOpIndex, algebraics.Count - 1)];
                    throw new Exception($"Expression \"{expr}\": At operation '{op}' following {prevValue}: Not enough values.");
                }

                var lhs = algebraics[nextOpIndex];
                var rhs = algebraics[nextOpIndex + 1];

                var result = op switch
                {
                    '+' => lhs + rhs,
                    '-' => lhs - rhs,
                    '*' => lhs * rhs,
                    '/' => lhs / rhs,
                    _ => throw new Exception($"Missing definition for '{op}' operator"),
                };

                algebraics[nextOpIndex] = result;
                algebraics.RemoveAt(nextOpIndex + 1);
                operations = operations.Remove(nextOpIndex, 1);
            }
        }

        if (operations.Length != 0)
        {
            throw new Exception($"Expression \"{expr}\": Unrecognized operations: {string.Join(", ", operations.Select(x => $"'{x}'"))}");
        }

        if (algebraics.Count > 1)
        {
            throw new Exception($"Expression \"{expr}\": More values than operations. Values: {string.Join("; ", algebraics)}");
        }
        else if (algebraics.Count == 0)
        {
            throw new Exception($"Expression \"{expr}\": No values in result.");
        }

        return algebraics.First();
    }
}
