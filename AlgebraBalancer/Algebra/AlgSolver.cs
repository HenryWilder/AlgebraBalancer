using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlgebraBalancer.Algebra;
public static class AlgSolver
{
    public readonly static Regex rxImpliedMul =
        new(@"(?<=\))\s*(?=[0-9\(])|(?<=[0-9\)])\s*(?=\()",
            RegexOptions.Compiled);

    public static string CleanExpr(string expr) =>
        rxImpliedMul.Replace(LatexUnicode.SuperscriptToNumber(expr), "*")
            .Replace("×", "*")
            .Replace(" ", "");

    public static readonly Regex rxTerm =
        new(@"(?x)
            (?=-?\d|i|𝑖|ⅈ|√-?\d) # Deny empty match
            # Must match between 1 and 3, in order, not repeating, of the following groups:
            (?'coef' # coefficient
                # Minus needs to be a sign and not an operator,
                # so it must either be at the start of a(n) (sub)expression or following an operator.
                # Whitespace is not a concern because it is already removed by the function.
                (?: (?<=^|[-+/*(]) - )?
                \d+
            )?
            (?'imag' # imaginary
                i|𝑖|ⅈ
            )?
            (?:
                √ # Don't need to capture radical it just marks where the radicand is
                (?'radi' # radicand
                    -? # Minus here is always a sign because it is necessarily preceeded by the radical
                    \d+
                )
            )?
            (?-x)",
            RegexOptions.Compiled);

    // language=regex
    private const string RX_ALGEBRAIC =
        @"(?x)
            (?'numer' # numerator
                (?'numerOpenParen'\()?
                    (?'numerTerms'{rxTerm}) # First term doesn't need parentheses or operators
                    (?(numerOpenParen) # Only allow multiple terms if there are parentheses
                        (?:
                            # Must have an addition/subtraction operator between terms
                            # A negative term doesn't need an addition operator in front of it
                            (?=[-+])[-+]?? # Lazy to avoid stealing the sign away from the term
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

    enum AlgOp
    {
        Add,
        Sub,
        Mul,
        Div,
    }
    public static Algebraic SolveAlgebraic(string expr)
    {
        expr = CleanExpr(expr);

        var items = rxTerm.Matches(expr)
            .Select(match =>
            {
                var coef = match.Groups["coef"];
                var imag = match.Groups["imag"];
                var radi = match.Groups["radi"];
                int coefficient = coef.Success ? int.Parse(coef.Value) : 1;
                int radicand = (imag.Success ? -1 : 1) * (radi.Success ? int.Parse(radi.Value) : 1);
                Radical radical = new(coefficient, radicand);
                return (match, radical);
            })
            .ToList();

        Algebraic group = default;
        AlgOp? upcomingOperation = null;

        for (int i = 0; i < expr.Length; ++i)
        {
            int matchIndex = items.FindIndex(item => item.match.Index == i);
            if (matchIndex != -1)
            {
                var (match, radical) = items[matchIndex];
                i += match.Length;
                group = upcomingOperation is null
                    ? radical
                    : upcomingOperation switch
                    {
                        AlgOp.Add => group + radical,
                        AlgOp.Sub => group - radical,
                        AlgOp.Mul => group * radical,
                        AlgOp.Div => group / radical,
                        _ => throw new NotImplementedException()
                    };
                group.numerator = group.numerator.TermsSimplified().LikeTermsCombined();
                upcomingOperation = null;
                if (i == expr.Length) break;
            }

            char ch = expr[i];
            upcomingOperation = ch switch
            {
                '+' => AlgOp.Add,
                '-' => AlgOp.Sub,
                '*' => AlgOp.Mul,
                '/' => AlgOp.Div,
                _ => throw new NotImplementedException()
            };
        }

        return group;
    }
}
