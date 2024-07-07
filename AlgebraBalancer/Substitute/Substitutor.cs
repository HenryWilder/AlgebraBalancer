using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class Substitutor
{
    public Substitutor(string document, int lineStart, int lineEnd)
    {
        substitutions = SubstitutionParser.Parse(document, lineStart, lineEnd, out int newEndOfLine);
        expr = document.Substring(lineStart, newEndOfLine - lineStart);
    }

    private readonly List<ISubstitutible> substitutions;
    private readonly string expr;

    public string Substitute()
    {
        string expr = this.expr;

        foreach (var item in substitutions)
        {
            expr = item.GetRegex().Replace(expr, (match) => item.GetReplacement(match.Value));
        }

        return ParenCleaner.CleanParentheses(expr);
    }
}
