using System.Collections.Generic;

namespace AlgebraBalancer.Substitute;
public class Substitutor
{
    public Substitutor(string document, int lineStart, int lineEnd, out string expr)
    {
        substitutions = SubstitutionParser.Parse(document, lineStart, lineEnd, out int newEndOfLine);
        expr = document.Substring(lineStart, newEndOfLine - lineStart);
    }

    private readonly List<ISubstitutible> substitutions;

    public string Substitute(string expr, int maxDepth = 20)
    {
        foreach (var item in substitutions)
        {
            expr = item.GetRegex().Replace(expr, (match) => item.GetReplacement(match.Value, this, maxDepth - 1));
        }

        return ParenCleaner.CleanParentheses(expr);
    }
}
