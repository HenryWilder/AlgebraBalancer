using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using Substitution = (string variable, string value);

namespace AlgebraBalancer.Algebra.Balancer;

public enum Operation
{
    //tex: $$a - b = c \to a = c + b$$
    Add,

    //tex: $$a + b = c \to a = c - b$$
    Sub,

    //tex: $$\frac{a}{b} = c \to a = bc$$
    Mul,

    //tex: $$ab = c \to a = \frac{c}{b} \iff b \ne 0$$
    Div,

    //tex: $$\sqrt[b]{a} = c \to a = c^b$$
    Pow,

    //tex: $$a^b = c \to a = \pm\sqrt[b]{c}$$
    Root,

    //tex: $$b^a = c \to a = \log_b{c}$$
    Log,

    //tex: $$e^a = b \to a = \ln{b}$$
    Ln,

    //tex: $$\ln a = b \to a = e^b$$
    Exp,

    //tex: $$\lvert a \rvert = b \to a = \pm b$$
    Sign,

    //tex: $$ax^2 + bx + c = 0 \to x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a} \iff a \ne 0$$
    Quadratic,
}

public enum Refactor
{
    //tex: $$\begin{aligned}
    //ab + cb &\to b(a + c) \\
    //ab^2 + cb &\to b(ab + c)
    //\end{aligned}$$
    Factor,

    //tex:
    //$$\begin{aligned}
    //(a+b)(c+d) &\to ac + ad + bc + bd \\
    //(a+b)^2 &\to a^2 + 2ab + b^2 \\
    //\end{aligned}$$
    FOIL,
}

public interface IRelationshipItem { }

public struct RelSide(string value) : IRelationshipItem
{
    public string value = value;
    public static implicit operator string(RelSide side) => side.value;
    public static implicit operator RelSide(string value) => new(value);
}

public enum Comparator
{
    EQ, // =
    NE, // !=
    GT, // >
    GE, // >=
    LT, // <
    LE, // <=
    QE, // ?=
}

public struct RelComp(Comparator value) : IRelationshipItem
{
    public static readonly Regex reCmp =
        new(@"(&\s*)?([≠≥≤≟]|(==)|(!=)|(>=)|(<=)|(\?=)|((?<![!?>=<])=(?!=))|(>(?!=))|(<(?!=)))(\s*&)?",
            RegexOptions.ExplicitCapture);

    public Comparator value = value;
    public static implicit operator Comparator(RelComp cmp) => cmp.value;
    public static implicit operator RelComp(Comparator value) => new(value);

    public static RelComp FromString(string str) => new(str switch
    {
        "=" or "==" => Comparator.EQ,
        "≠" or "!=" => Comparator.NE,
        ">" => Comparator.GT,
        "≥" or ">=" => Comparator.GE,
        "<" => Comparator.LT,
        "≤" or "<=" => Comparator.LE,
        "≟" or "?=" => Comparator.QE,
        _ => throw new NotImplementedException($"The comparator \"{str}\" is not recognized"),
    });
}

public class Relationship
{
    private static readonly Regex reAroundCmp = new(@$"(?={RelComp.reCmp}\s*)|(?<={RelComp.reCmp})");
    public static Relationship Parse(string str)
    {
        string[] items = reAroundCmp.Split(str.Replace("&", ""))
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        return new Relationship(items);
    }

    public Relationship(params object[] items)
    {
        if (items.Length % 2 == 0) throw new Exception("Relationship must have an odd number of items so that each side has a comparison");
        this.items = new IRelationshipItem[items.Length];
        for (int i = 0; i < items.Length; ++i)
        {
            object item = items[i];
            if (i % 2 == 0)
            {
                if (item is RelSide side)
                {
                    this.items[i] = side;
                }
                else if (item is string sideStr)
                {
                    this.items[i] = new RelSide(sideStr);
                }
                else
                {
                    throw new Exception($"Expected item {i} to be RelSide");
                }
            }
            else
            {
                if (item is RelComp cmp)
                {
                    this.items[i] = cmp;
                }
                else if (item is Comparator cmpEnum)
                {
                    this.items[i] = new RelComp(cmpEnum);
                }
                else if (item is string cmpStr)
                {
                    this.items[i] = RelComp.FromString(cmpStr);
                }
                else
                {
                    throw new Exception($"Expected item {i} to be RelComp");
                }
            }
        }
    }

    public override string ToString()
    {
        return string.Join(" ", items.Select(item =>
        {
            if (item is RelSide side)
            {
                return side.value;
            }
            else if (item is RelComp cmp)
            {
                return cmp.value switch
                {
                    Comparator.EQ => "=",
                    Comparator.NE => "!=",
                    Comparator.GT => ">",
                    Comparator.GE => ">=",
                    Comparator.LT => "<",
                    Comparator.LE => "<=",
                    Comparator.QE => "?=",
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }));
    }

    public IRelationshipItem[] items;

    public int NumSides => (items.Length + 1) / 2;

    private IEnumerable<string> Sides
    {
        get => items
            .Where(x => x is RelSide)
            .Select(x => (string)(RelSide)x);

        set
        {
            int numValues = value.Count();
            if (numValues != NumSides) throw new Exception($"Wrong number of items; expected {NumSides}, got {numValues}");
            for (int i = 0; i < numValues; ++i)
            {
                items[i * 2] = new RelSide(value.ElementAt(i));
            }
        }
    }

    private static readonly Regex reMultipleSpaces = new(@"[\t ]+");
    private static readonly Regex reLeadingPlus    = new(@"(?<=^|\()\s*\+\s*");
    private static readonly Regex reUnaryMinus     = new(@"(?<=^|\()\s*\-\s+");
    private static readonly Regex reBinaryOp       = new(@"(?<!^|\()\s*([-+/*])\s*");
    public void ApplyOperation(Operation operation, string value)
    {
        static string Cleanup(string str)
        {
            str = reLeadingPlus.Replace(str, "");
            str = reBinaryOp.Replace(str, (Match match) => " " + match.Groups[1].Value + " ");
            str = reUnaryMinus.Replace(str, "-");
            str = reMultipleSpaces.Replace(str, " ");
            return str.Trim();
        }
        switch (operation)
        {
            case Operation.Add:
                Regex reAddInv = new(@$"\s*\-\s*{value}\s*(?=[+-]|$)");
                Sides = Sides.Select(x => {
                    foreach (Match match in reAddInv.Matches(x))
                    {
                        string pre = x.Substring(0, match.Index);
                        // Not inside parentheses
                        if (pre.Count(ch => ch == '(') == pre.Count(ch => ch == ')'))
                        {
                            return Cleanup(x.Remove(match.Index, match.Length));
                        }
                    }
                    return $"{x} + {value}";
                });
                break;
            case Operation.Sub:
                Regex reSubInv = new(@$"\s*\+\s*{value}\s*(?=[+-]|$)");
                Sides = Sides.Select(x => reSubInv.IsMatch(x)
                    ? Cleanup(reSubInv.Replace(x, "", 1))
                    : $"{x} - {value}");
                break;
            case Operation.Mul:
                break;
            case Operation.Div:
                break;
            case Operation.Pow:
                break;
            case Operation.Root:
                break;
            case Operation.Log:
                break;
            case Operation.Ln:
                break;
            case Operation.Exp:
                break;
            case Operation.Sign:
                break;
            case Operation.Quadratic:
                break;

            default: throw new NotImplementedException();
        }
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

    ///// <summary>
    ///// Like <see cref="rxUnnededTermParentheses"/> but without allowing internal multiplication
    ///// </summary>
    //private static readonly Regex rxUnnededExponentBaseParentheses = new(
    //    @"(?<=^|[^\d\s)])\s*\(\s*(\d+|.)\s*\)\s*(?=[^\d\s(]|$)",
    //    RegexOptions.Compiled);

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

    private static readonly DataTable dt = new();
    private static readonly Regex rxImpliedMul = new(@"(?<=\))\s*(?=[\d\(])|(?<=[\d\)])\s*(?=\()");

    public static string SolveIfTrivial(string expr)
    {
        // Smallest we expect to display
        const double EPSILON = 0.000000000000001;

        try
        {
            object computed = dt.Compute(rxImpliedMul.Replace(expr, "*"), "");
            if (computed is not null)
            {
                double computedDouble = Convert.ToDouble(computed);

                // Only integers are "trivial" here
                if (Math.Abs(computedDouble - Math.Round(computedDouble)) < EPSILON)
                {
                    return Convert.ToInt64(computed).ToString();
                }
            }
        }
        catch { }

        return expr;
    }

    private static readonly Regex rxBracket = new(@"[(){}\[\]]");

    private static readonly Regex rxName =
        new(@"(?:["
                // Normal
                + @"_"
                + @"A-Z"
                + @"a-z"

                // Greek
                + @"ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡϴΣΤΥΦΧΨΩ∇"
                + @"αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ"

                // Math-style
                + @"𝐴𝐵𝐶𝐷𝐸𝐹𝐺𝐻𝐼𝐽𝐾𝐿𝑀𝑁𝑂𝑃𝑄𝑅𝑆𝑇𝑈𝑉𝑊𝑋𝑌𝑍"
                + @"𝑎𝑏𝑐𝑑𝑒𝑓𝑔𝑖𝑗𝑘𝑙𝑚𝑛𝑜𝑝𝑞𝑟𝑠𝑡𝑢𝑣𝑤𝑥𝑦𝑧"
                + @"𝚤𝚥"

                // Blackboard Bold
                + @"𝟘𝟙𝟚𝟛𝟜𝟝𝟞𝟟𝟠𝟡"
                + @"𝔸𝔹ℂ𝔻𝔼𝔽𝔾ℍ𝕀𝕁𝕂𝕃𝕄ℕ𝕆ℙℚℝ𝕊𝕋𝕌𝕍𝕎𝕏𝕐ℤ"
                + @"𝕒𝕓𝕔𝕕𝕖𝕗𝕘𝕙𝕚𝕛𝕜𝕝𝕞𝕟𝕠𝕡𝕢𝕣𝕤𝕥𝕦𝕧𝕨𝕩𝕪𝕫"
                + @"ℼℽℾℿ⅀"

                // Fraktur
                + @"𝔄𝔅ℭ𝔇𝔈𝔉𝔊ℌ𝔍𝔎𝔏𝔐𝔑𝔒𝔓𝔔𝔖𝔗𝔘𝔙𝔚𝔛𝔜ℨ"
                + @"𝔞𝔟𝔠𝔡𝔢𝔣𝔤𝔥𝔦𝔧𝔨𝔩𝔪𝔫𝔬𝔭𝔮𝔯𝔰𝔱𝔲𝔳𝔴𝔵𝔶𝔷"

                // Caligraphic
                + @"𝒜ℬ𝒞𝒟ℰℱ𝒢ℋℐ𝒥𝒦ℒℳ𝒩𝒪𝒫𝒬ℛ𝒮𝒯𝒰𝒱𝒲𝒳𝒴𝒵"
                + @"𝒶𝒷𝒸𝒹ℯ𝒻ℊ𝒽𝒾𝒿𝓀𝓁𝓂𝓃ℴ𝓅𝓆𝓇𝓈𝓉𝓊𝓋𝓌𝓍𝓎𝓏"

                // Subscript
                + @"₀₁₂₃₄₅₆₇₈₉"
                + @"ₐₑₕₖₗₘₙₒₚₛₜₓ"

                // Prime
                + @"`'"""
                + @"′″‴"
                + @"‵‶‷"
            + "]|⁻¹)+",
            RegexOptions.Compiled);

    private static readonly Regex rxFunction =
        new(@$"({rxName})\(((?:[^(){{}}\[\]]|(?'open'[({{\[])|(?'-open'[)}}\]]))*(?(open)(?!)))\)",
            RegexOptions.Compiled);

    private static readonly Regex rxContainedSemicolons =
        new(@"(?<=^(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*(?(open)(?!)));",
            RegexOptions.Compiled);

    private static readonly Regex rxSubstitutionSplit = new(@"\s*(?:;|\sand\s)\s*");

    private static readonly Regex rxMappedFunction = 
        new(@"\{(?:\s*(?'in'.+?)\s*(?:\|?->|↦|→)\s*(?'out'[^(),]+|\((?:[^()]|(?'open'\()|(?'-open'\)))*(?(open)(?!))\)),?)+\s*\}",
            RegexOptions.Compiled);

    public static Substitution[] ParseSubstitutions(string substitutionStr)
    {
        return rxSubstitutionSplit.Split(substitutionStr)
            .Select((x) =>
            {
                string[] parts = x.Split("=");
                return (parts[0].Trim(), parts[1].Trim());
            })
            .ToArray();
    }

    private static readonly Regex rxAnonymousFunction =
        new(@$"\((?:(?'arg1'{rxName})(?:\s+(?'argn'{rxName}))*)\s*(?:=>|⇒|\|->|↦)\s*(?'expr'(?:[^()]|(?'eo'\()|(?'-eo'\)))*(?(eo)(?!)))\)\((?'call'(?:[^()]|(?'co'\()|(?'-co'\)))*(?(co)(?!)))\)",
            RegexOptions.Compiled);

    public static string AnonymousCalls(string expr, int allowedSubCalls = 20)
    {
        return CleanParentheses(rxAnonymousFunction.Replace(expr, (Match match) =>
        {
            string[] defArgs = match.Groups["argn"].Captures
                .Select(x => x.Value)
                .Prepend(match.Groups["arg1"].Value)
                .ToArray();
            string def = match.Groups["expr"].Value;
            string[] callArgs = match.Groups["call"].Value.Split(",");

            for (int i = 0; i < Math.Min(defArgs.Length, callArgs.Length); ++i)
            {
                string defArg = defArgs[i]; // Should already be trimmed by regex
                string callArg = AnonymousCalls(callArgs[i].Trim(), allowedSubCalls - 1);
                def = CleanParentheses(def.Replace(defArg, $"({callArg})"));
            }

            return "(" + (allowedSubCalls > 0 ? AnonymousCalls(def, allowedSubCalls - 1) : def) + ")";
        }));
    }

    public static string Substitute(string expr, Substitution[] substitutions, int allowedDepth = 100)
    {
        foreach (var (variable, value) in substitutions)
        {
            var signatureMatch = rxFunction.Match(variable);
            if (signatureMatch.Success)
            {
                string functionName =
                    signatureMatch.Groups[1].Value;

                string[] signatureParamList =
                    signatureMatch.Groups[2].Value
                        .Split(",")
                        .Select(x => x.Trim())
                        .ToArray();

                string functionDefinition = value;

                Func<string[], string> fnReplacement;
                var mappedFunctionDef = rxMappedFunction.Match(functionDefinition);
                if (mappedFunctionDef.Success)
                {
                    var inputs  = mappedFunctionDef.Groups["in"].Captures;
                    var outputs = mappedFunctionDef.Groups["out"].Captures;
                    var mapping = inputs
                        .Zip(outputs, (i, o) => (i.Value.Trim(), o.Value.Trim()))
                        .ToDictionary(x => x.Item1, x => x.Item2);

                    fnReplacement = (string[] callParams) =>
                        mapping.TryGetValue(callParams[0], out string associated)
                            ? associated
                            : "∄";
                }
                else
                {
                    fnReplacement = (string[] callParams) =>
                    {
                        string replacement = functionDefinition;
                        for (int i = 0; i < Math.Min(signatureParamList.Length, callParams.Length); ++i)
                        {
                            string paramName = signatureParamList[i];
                            string paramValue = "(" + CleanParentheses(SolveIfTrivial(callParams[i])) + ")";
                            replacement = replacement.Replace(paramName, paramValue);
                        }
                        return replacement;
                    };
                }

                expr = rxFunction.Replace(
                    expr,
                    (Match callMatch) =>
                    {
                        string callName = callMatch.Groups[1].Value;
                        if (callName == functionName)
                        {
                            string[] callParams;
                            {
                                string callParamListStr = callMatch.Groups[2].Value;
                                // Expand recursively (up to recursionsRemaining) before passing parameters
                                string callParamList = allowedDepth > 0
                                    ? Substitute(callParamListStr, substitutions, allowedDepth - 1)
                                    : callParamListStr;

                                string[] callParamItems;
                                // todo: there's probably a cleaner way to do this
                                if (rxBracket.IsMatch(callParamList))
                                {
                                    string semicolonDelimitedParams =
                                        callParamList.Replace(",", ";");

                                    string minorCommaDelimitedParams =
                                        rxContainedSemicolons.Replace(semicolonDelimitedParams, ",");

                                    callParamItems = minorCommaDelimitedParams.Split(";");
                                }
                                else
                                {
                                    callParamItems = callParamList.Split(",");
                                }
                                callParams = callParamItems
                                    .Select(CleanParentheses)
                                    .Select(x => x.Trim())
                                    .ToArray();
                            }

                            return "(" + AnonymousCalls(fnReplacement(callParams)) + ")";
                        }
                        else
                        {
                            return callMatch.Groups[0].Value;
                        }
                    }
                );
            }
            else
            {
                expr = expr.Replace(variable, $"({value})");
            }
        }
        return CleanParentheses(expr);
    }

    public static string Substitute(string expr, string substitutionStr) =>
        Substitute(expr, ParseSubstitutions(substitutionStr));

    public static string Factor(string expr)
    {


        return expr;
    }
}
