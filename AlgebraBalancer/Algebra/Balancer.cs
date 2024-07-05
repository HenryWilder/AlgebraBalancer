using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        return expr;
    }

    private static readonly Regex rxBracket = new(@"[(){}\[\]]");

    private static readonly Regex rxFunction =
        new(@"([^\s\d(){}\[\]]+)\(((?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*(?(open)(?!)))\)",
            RegexOptions.Compiled);

    private static readonly Regex rxContainedSemicolons =
        new(@"(?<=^(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*(?(open)(?!)));",
            RegexOptions.Compiled);

    private static readonly Regex rxSubstitutionSplit = new(@"\s*(?:;|\sand\s)\s*");

    public static string Substitute(string expr, string sub)
    {
        (string variable, string value)[] substitutions =
            rxSubstitutionSplit.Split(sub)
                .Select((x) =>
                {
                    string[] parts = x.Split("=");
                    return (parts[0].Trim(), parts[1].Trim());
                })
                .ToArray();

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

                expr = rxFunction.Replace(
                    expr,
                    (Match callMatch) => {
                        string callName = callMatch.Groups[1].Value;
                        if (callName == functionName)
                        {
                            string[] callParams;
                            {
                                string callParamList = callMatch.Groups[2].Value;
                                if (rxBracket.IsMatch(callParamList))
                                {
                                    string semicolonDelimitedParams =
                                        callParamList.Replace(",", ";");

                                    string minorCommaDelimitedParams =
                                        rxContainedSemicolons.Replace(semicolonDelimitedParams, ",");

                                    callParams = minorCommaDelimitedParams
                                        .Split(";")
                                        .Select(x => x.Trim())
                                        .ToArray();
                                }
                                else
                                {
                                    callParams = callParamList
                                        .Split(",")
                                        .Select(x => x.Trim())
                                        .ToArray();
                                }
                            }

                            string replacement = functionDefinition;
                            for (int i = 0; i < Math.Min(signatureParamList.Length, callParams.Length); ++i)
                            {
                                string paramName = signatureParamList[i];
                                string paramValue = $"({callParams[i]})";
                                replacement = replacement.Replace(paramName, paramValue);
                            }

                            return "(" + replacement + ")";
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

    public static string Factor(string expr)
    {


        return expr;
    }
}
