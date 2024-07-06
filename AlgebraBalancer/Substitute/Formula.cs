using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AlgebraBalancer.Substitute;

public class SubstitutableString
{
    private readonly string str;
    private readonly (int pos, string arg)[] replacements;

    public SubstitutableString(string str, params string[] argNames)
    {
        this.str = str;

        List<(int pos, string arg)> repl = [];

        Regex anyArg = new(@$"({string.Join("|", argNames)})");

        foreach (Match match in anyArg.Matches(this.str))
        {
            repl.Add((match.Index, match.Groups[1].Value));
        }

        replacements = [..repl.OrderByDescending(rep => rep.pos)];
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

public class Formula : ISubstitutible
{
    private Formula(string name, string[] parameterNames, SubstitutableString definition)
    {
        Name = name;
        this.parameterNames = parameterNames;
        this.definition = definition;
    }

    public static Formula Define(string variable, string value)
    {
        string name = Variable.rxName.Match(variable).Groups[0].Value;
        return new Formula(name, [""], new SubstitutableString(""));
    }

    public string Name { get; }
    private string[] parameterNames;
    private SubstitutableString definition;

    public static readonly Regex rxParameterList =
        new(@"\((?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!)))(?:,\s*(?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!))))*\)$",
            RegexOptions.Compiled);

    public string GetReplacement(string capture)
    {
        //rxParameterList.Match(capture);
        return "";
    }
}
