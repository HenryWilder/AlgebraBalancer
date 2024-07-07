using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class Substitutor(string document, int lineStart, int lineEnd)
{
    private readonly List<ISubstitutible> substitutions = SubstitutionParser.Parse(document, lineStart, lineEnd);
    private readonly string expr = document.Substring(lineStart, lineEnd - lineStart);

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
