using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AlgebraBalancer.Substitute;

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
        var match = rxParameterList.Match(variable);
        if (match.Success)
        {
            string name = variable.Substring(0, match.Index);
            string[] argNames = match.Groups["args"].Captures.Select(x => x.Value).ToArray();
            return new Formula(name, argNames, new SubstitutableString(value, argNames));
        }
        else
        {
            throw new Exception("Formula missing argument list");
        }
    }

    public string Name { get; }
    private readonly string[] parameterNames;
    private readonly SubstitutableString definition;

    public static readonly Regex rxParameterList =
        new(@"\((?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!)))(?:,\s*(?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!))))*\)",
            RegexOptions.Compiled);

    public string GetReplacement(string capture)
    {
        var match = rxParameterList.Match(capture);
        if (match.Success)
        {
            var argCaptures = match.Groups["args"].Captures;
            var callArgs = parameterNames
                .Zip(argCaptures, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => "(" + x.v.Value + ")");

            return definition.GetSubstituted(callArgs);
        }
        else
        {
            return capture;
        }
    }
}
