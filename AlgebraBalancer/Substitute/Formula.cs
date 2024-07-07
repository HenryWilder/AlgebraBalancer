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
        this.name = name;
        this.parameterNames = parameterNames;
        this.definition = definition;
    }

    public static readonly Regex rxParameterList =
        new(@"\((?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!)))(?:,\s*(?'args'(?:[^(){}\[\]]|(?'open'[({\[])|(?'-open'[)}\]]))*?(?(open)(?!))))*\)",
            RegexOptions.Compiled);

    /// <summary>
    /// <paramref name="key"/> = "f(a, b)"
    /// <paramref name="val"/> = "2a+b"
    /// </summary>
    public static Formula TryDefine(string key, string val)
    {
        var match = rxParameterList.Match(key);
        if (match.Success)
        {
            string name = key.Substring(0, match.Index);
            string[] argNames = match.Groups["args"].Captures.Select(x => x.Value).ToArray();
            return new Formula(name, argNames, new SubstitutableString(val, argNames));
        }
        return null;
    }

    public readonly string name;
    public readonly string[] parameterNames;
    public readonly SubstitutableString definition;

    public string Name => name;

    /// <summary>
    /// <paramref name="capture"/> = "f(43, 7)"
    /// </summary>
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
        return capture;
    }

    public Regex GetRegex() => new(name + rxParameterList);
}
