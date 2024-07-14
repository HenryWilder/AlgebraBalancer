using System.Text.RegularExpressions;

namespace AlgebraBalancer.Algebra;
public static class NumericFmt
{
    private static readonly Regex rxOperand =
        new(@"\d+|\p{L}[₀₁₂₃₄₅₆₇₈₉₌ₐₑₕₖₗₘₙₒₚₛₜₓ'""`′″‴‵‶‷]*|\((?:[^()]+|(?'open'\()|(?'-open'\)))*?(?(open)(?!))\)",
            RegexOptions.Compiled);

    private const string RX_ADD =
        /* lang=regex */ @"(?'lhs'{operand})[*×⋅]?(?'rhs'{operand})";

    private const string RX_SUB =
        /* lang=regex */ @"(?'lhs'{operand})[*×⋅]?(?'rhs'{operand})";

    private const string RX_MUL =
        /* lang=regex */ @"(?'lhs'{operand})[*×⋅]?(?'rhs'{operand})";

    private const string RX_DIV =
        /* lang=regex */ @"(?'lhs'{operand})[*×⋅]?(?'rhs'{operand})";

    private const string RX_POW =
        /* lang=regex */ @"(?'base'{operand})(?:\^(?'powbrace'\{)(?'exp'\d+)(?(powbrace)\})|(?'exp'[⁰¹²³⁴⁵⁶⁷⁸⁹]+))";

    public static string FormatExpr(string expr)
    {
        return expr
            .Replace("+-", "-")
            .Replace("-+", "-")
            .Replace("--", "+")
            .Replace("++", "+")
        ;
    }
}
