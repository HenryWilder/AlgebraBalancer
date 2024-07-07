using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class SubstitutableString
{
    private readonly string str;
    private readonly (int pos, string arg)[] replacements;

    public SubstitutableString(string str, params string[] argNames)
    {
        this.str = str;

        List<(int pos, string arg)> repl = [];

        Regex anyArg = new(@$"({string.Join("|", argNames.OrderByDescending(x => x.Length))})");

        foreach (Match match in anyArg.Matches(this.str))
        {
            repl.Add((match.Index, match.Groups[1].Value));
        }

        replacements = [.. repl.OrderByDescending(rep => rep.pos)];
    }

    public string GetSubstituted(Dictionary<string, string> parameters)
    {
        string subStr = str;
        foreach (var (pos, arg) in replacements)
        {
            if (parameters.TryGetValue(arg, out string replaceWith))
            {
                subStr = subStr
                    .Remove(pos, arg.Length)
                    .Insert(pos, replaceWith);
            }
        }
        return subStr;
    }
}
