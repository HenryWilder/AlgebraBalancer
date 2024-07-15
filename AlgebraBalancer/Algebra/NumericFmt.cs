using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlgebraBalancer.Algebra;
public static class NumericFmt
{
    public enum FormatOptions
    {
        /// <summary><c>2+6*8(3-1)</c> => <c>2 + 6 * 8(3 - 1)</c></summary>
        SpaceAroundBinaryOperators = 1,
        /// <summary><c>2+2=6-2</c> => <c>2+2 = 6-2</c></summary>
        SpaceAroundRelationalOperators = 1 << 1,

        /// <summary><c>√-1</c> => <c>i</c></summary>
        AsciiImaginary = 0,
        /// <summary><c>√-1</c> => <c>𝑖</c></summary>
        ItalicImaginary = 1 << 2,
        /// <summary><c>√-1</c> => <c>ⅈ</c></summary>
        ComplexImaginary = 1 << 3,

        /// <summary><c>*</c> => <c>*</c></summary>
        AsciiMul = 0,
        /// <summary><c>*</c> => <c>×</c></summary>
        TimesMul = 1 << 4,
        /// <summary><c>*</c> => <c>⋅</c></summary>
        CDotMul = 1 << 5,

        /// <summary><c>/</c> => <c>/</c></summary>
        SlashDiv = 0,
        /// <summary><c>/</c> => <c>÷</c></summary>
        DivDiv = 1 << 6,
    }

    private static readonly Regex rxBinaryOperator =
        new(@$"(?<!(?:^|\(|{rxRelationalOperator})\s*)\-|[+*/×÷⋅%]",
            RegexOptions.Compiled);

    private static readonly Regex rxTestSpaceAroundBinaryOperators =
        new(@$"(?<!^|\(|{rxRelationalOperator})\s+\-\s+|\s+[+*/×÷⋅%]\s+",
            RegexOptions.Compiled);

    private static readonly Regex rxRelationalOperator =
        new(@"[>=<!]=?|[≈≠≡≢≤≥∈∋∉∌⊂⊃⊄⊅⊆⊇⊈⊉⊊⊋⊏⊐⊄⊅⊑⊒⋢⋣⋤⋥]",
            RegexOptions.Compiled);

    private static readonly Regex rxTestSpaceAroundRelationalOperators =
        new(@$"\s+(?:{rxRelationalOperator})\s+",
            RegexOptions.Compiled);

    public static FormatOptions IdentifyPreference(string example)
    {
        FormatOptions result = 0;

        if (rxTestSpaceAroundBinaryOperators.IsMatch(example))
        {
            result |= FormatOptions.SpaceAroundBinaryOperators
                | FormatOptions.SpaceAroundRelationalOperators; // "1 + 1=3 - 1" looks weird
        }
        else if (rxTestSpaceAroundRelationalOperators.IsMatch(example))
        {
            result |= FormatOptions.SpaceAroundRelationalOperators;
        }

        if (example.Contains("ⅈ"))
        {
            result |= FormatOptions.ComplexImaginary;
        }
        else if (example.Contains("𝑖"))
        {
            result |= FormatOptions.ItalicImaginary;
        }
        else // i
        {
            result |= FormatOptions.AsciiImaginary;
        }

        if (example.Contains("⋅"))
        {
            result |= FormatOptions.CDotMul;
        }
        else if (example.Contains("×"))
        {
            result |= FormatOptions.TimesMul;
        }
        else // *
        {
            result |= FormatOptions.AsciiMul;
        }

        if (example.Contains("÷"))
        {
            result |= FormatOptions.DivDiv;
        }
        else // /
        {
            result |= FormatOptions.SlashDiv;
        }

        return result;
    }

    private const string RX_NON_DIGIT_OPERAND =
        /* lang=regex */ @"\p{L}[₀₁₂₃₄₅₆₇₈₉ₐₑₕₖₗₘₙₒₚₛₜₓ'""`′″‴‵‶‷]*|\((?:[^()]+|(?'open'\()|(?'-open'\)))*?(?(open)(?!))\)";

    // The right side of an operand
    // Comes with an "ldigit" optional capture
    private const string RX_L_OPERAND =
        /* lang=regex */ @"(?'ldigit'\d)|\p{L}[₀₁₂₃₄₅₆₇₈₉ₐₑₕₖₗₘₙₒₚₛₜₓ'""`′″‴‵‶‷]*|\)";

    // The left side of an operand
    // Comes with an "ldigit" optional capture
    private const string RX_R_OPERAND =
        /* lang=regex */ @"(?'rdigit'\-?\d)|\p{L}|\(|√";

    private static readonly Regex rxOperand =
        new(@$"\d+|{RX_NON_DIGIT_OPERAND}",
            RegexOptions.Compiled);

    private static readonly Regex rxImpliedMul =
        new(@$"(?<={RX_L_OPERAND})(?={RX_R_OPERAND})(?(ldigit)(?(rdigit)(?!)))",
            RegexOptions.Compiled);

    // Should come last
    private static readonly Regex rxImpliableMul =
        new(@$"(?<={RX_L_OPERAND})\s*\*\s*(?={RX_R_OPERAND})(?(ldigit)(?(rdigit)(?!)))",
            RegexOptions.Compiled);

    // Should come first
    private static readonly Regex rxAddSubChain =
        new(@"[-+]{2,}",
            RegexOptions.Compiled);

    private static readonly Regex rxPow =
        new(@"\^(?'open'[({])?(?'exponent'\-?\d+)(?(open)[)}])",
            RegexOptions.Compiled);

    // Should follow Pow conversion to superscript
    private static readonly Regex rxIdentityPower =
        new(@$"(?<={RX_L_OPERAND})¹(?![⁰¹²³⁴⁵⁶⁷⁸⁹])",
            RegexOptions.Compiled);

    // Should follow Pow conversion to superscript
    private static readonly Regex rxPowerToIdentity =
        new(@$"(?'lmul'[*/])?(?:{rxOperand})⁰(?(lmul)|\*)",
            RegexOptions.Compiled);

    // Should follow PowerToIdentity
    private static readonly Regex rxIdentityProductQuotient =
        new(@$"1\*(?={RX_R_OPERAND})|(?<={RX_L_OPERAND})[*/]1",
            RegexOptions.Compiled);

    // Should follow AddSubChain
    private static readonly Regex rxIdentitySumDifference =
        new(@$"0[-+](?={RX_R_OPERAND})|(?<={RX_L_OPERAND})[-+]0",
            RegexOptions.Compiled);

    // Should follow AddSubChain
    private static readonly Regex rxSpacedUnaryPrefixMinus =
        new(@$"(?:(?<=^|\()\s*|(?<={rxRelationalOperator}\s*))\-\s*",
            RegexOptions.Compiled);

    private static readonly Regex rxMultipleSpaces =
        new(@" +", RegexOptions.Compiled);

    /// <summary>
    /// Takes a string from the user and expands it for parsing.
    /// 
    /// Example:
    /// <c>3(4 + 2 - 1)² - 7 × 3 + -(3x + 2)</c> => <c>3*(4+2+-1)^2+-7*3+-(3x + 2)</c>
    /// </summary>
    public static string ParserFormat(string expr)
    {
        expr = expr
            .Replace(" ", "")
            .Replace("×", "*")
            .Replace("⋅", "*")
            .Replace("÷", "/")
            .Replace("𝑖", "i")
            .Replace("ⅈ", "i")
        ;

        expr = rxImpliedMul.Replace(expr, "*");
        expr = LatexUnicode.SuperscriptToNumber(expr);

        return expr;
    }

    /// <summary>
    /// Takes a string generated by the program and simplifies it for display.
    /// 
    /// Example:
    /// <c>3*(4+2+-1)^2+-7*3*-1x</c> => <c>3(4 + 2 - 1)² - 7 × 3 × -x</c>
    /// </summary>
    public static string DisplayFormat(string expr, FormatOptions fmt = 0)
    {
        expr = rxAddSubChain.Replace(expr, (chain) =>
        {
            string chainStr = chain.Value;
            int subCount = chainStr.Count(ch => ch == '-');
            return (subCount % 2 == 0) ? "+" : "-";
        });

        expr = rxPow.Replace(expr, (match) => LatexUnicode.ToSuperscript(match.Groups["exponent"].Value));

        expr = rxIdentityProductQuotient.Replace(expr, "");
        expr = rxIdentitySumDifference  .Replace(expr, "");
        expr = rxIdentityPower          .Replace(expr, "");
        expr = rxPowerToIdentity        .Replace(expr, "");

        expr = rxImpliableMul.Replace(expr, "");

        switch (fmt & (FormatOptions.AsciiImaginary | FormatOptions.ItalicImaginary | FormatOptions.ComplexImaginary))
        {
            case FormatOptions.AsciiImaginary:
                break;
            case FormatOptions.ItalicImaginary:
                expr = expr.Replace("i", "𝑖");
                break;
            case FormatOptions.ComplexImaginary:
                expr = expr.Replace("i", "ⅈ");
                break;
            default:
                throw new Exception("Imaginary cannot simultaneously be 'ⅈ' and '𝑖'");
        }

        switch (fmt & (FormatOptions.AsciiMul | FormatOptions.TimesMul | FormatOptions.CDotMul))
        {
            case FormatOptions.AsciiMul:
                break;
            case FormatOptions.TimesMul:
                expr = expr.Replace("*", "×");
                break;
            case FormatOptions.CDotMul:
                expr = expr.Replace("*", "⋅");
                break;
            default:
                throw new Exception("Multiplication cannot simultaneously be '×' and '⋅'");
        }

        switch (fmt & (FormatOptions.SlashDiv | FormatOptions.DivDiv))
        {
            case FormatOptions.SlashDiv:
                break;
            case FormatOptions.DivDiv:
                expr = expr.Replace("/", "÷");
                break;
        }

        if (fmt.HasFlag(FormatOptions.SpaceAroundRelationalOperators))
        {
            expr = rxRelationalOperator.Replace(expr, (match) => " " + match.Value + " ");
        }

        if (fmt.HasFlag(FormatOptions.SpaceAroundBinaryOperators))
        {
            expr = rxBinaryOperator.Replace(expr, (match) => " " + match.Value + " ");
        }

        expr = rxSpacedUnaryPrefixMinus.Replace(expr, "-");

        expr = rxMultipleSpaces.Replace(expr, " ");

        return expr;
    }
}
