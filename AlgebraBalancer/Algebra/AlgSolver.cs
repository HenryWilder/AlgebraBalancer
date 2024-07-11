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
            (?: # numerator
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

    private interface IOperationArgument
    {
        public Algebraic GetAlgebraic();
    }

    private class ValueArg(Algebraic value) : IOperationArgument
    {
        public Algebraic value = value;
        public Algebraic GetAlgebraic() => value;
    }

    private class SubExprArg : IOperationArgument
    {
        public SubExprArg() => subExpr = new();
        public SubExprArg(OperationTreeNode subExpr) => this.subExpr = subExpr;
        
        public OperationTreeNode subExpr;
        public Algebraic GetAlgebraic() => subExpr.EvaluateRecursively();
    }

    enum AlgOp
    {
        Add,
        Sub,
        Mul,
        Div,
    }

    private class OperationTreeNode
    {
        public List<AlgOp> ops = []; // Should be 1 smaller than args
        public IOperationArgument args;

        public Algebraic EvaluateRecursively()
        {
            Algebraic acc;


            var lhs = this.lhs.GetAlgebraic();
            var rhs = this.rhs.GetAlgebraic();
            return op switch
            {
                AlgOp.Add => lhs + rhs,
                AlgOp.Sub => lhs - rhs,
                AlgOp.Mul => lhs * rhs,
                AlgOp.Div => lhs / rhs,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static Algebraic SolveAlgebraic(string expr)
    {
        expr = CleanExpr(expr);

        var algebraicMatches = rxAlgebraic.Matches(expr);

        var algebraics = algebraicMatches.Select(match =>
        {
            Radical[] numerTerms = [..
                match.Groups["numerTerms"].Captures.Select(capture =>
                {
                    // I know it's inefficient to match *again*, but I can't get subgroups from captures :c
                    var term = rxTerm.Match(capture.Value);
                    if (!term.Success) throw new Exception("Should be match if within capture");
                    var coef = term.Groups["coef"];
                    var imag = term.Groups["imag"];
                    var radi = term.Groups["radi"];
                    int coefficient = coef.Success ? int.Parse(coef.Value) : 1;
                    int radicand = (imag.Success ? -1 : 1) * (radi.Success ? int.Parse(radi.Value) : 1);
                    return new Radical(coefficient, radicand);
                })
            ];
            var denom = match.Groups["denom"];
            int denominator = denom.Success ? int.Parse(denom.Value) : 1;
            var alg = new Algebraic(numerTerms, denominator);
            return (match, alg);
        }).ToList();

        var root = new OperationTreeNode();

        Stack<bool> currentPath = [];
        currentPath.Push(false);
        void IncrementPath()
        {
            while (currentPath.Count > 0 && currentPath.Pop()) { }
            if (currentPath.Count == 0)
            {
                root = new() { lhs = new SubExprArg(root) };
                currentPath.Push(true);
            }
            else
            {
                currentPath.Push(true);
            }
        }

        var algIndexes = algebraics.Select(x => x.match.Index).ToList();
        for (int i = 0; i < expr.Length;)
        {
            int algHere = algIndexes.IndexOf(i);
            if (algHere != -1) // An element exists
            {
                var (match, alg) = algebraics[algHere];

                root[[.. currentPath]] = new ValueArg(alg);
                IncrementPath();

                i += match.Length; // Skip to next item
            }
            else
            {
                char ch = expr[i];

                bool[] pathTemp = [.. currentPath];
                var currentNode = root[pathTemp];
                if (currentNode is SubExprArg subExpr)
                {
                    if (ch == '(')
                    {
                        throw new NotImplementedException();
                    }
                    else if (ch == ')')
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        subExpr.subExpr.op = ch switch
                        {
                            '+' => AlgOp.Add,
                            '-' => AlgOp.Sub,
                            '*' => AlgOp.Mul,
                            '/' => AlgOp.Div,
                            _ => throw new NotImplementedException(),
                        };
                    }
                }
                else
                {
                    throw new Exception("oops");
                }
                root[pathTemp] = currentNode;

                ++i;
            }
        }

        return root.EvaluateRecursively();
    }
}
