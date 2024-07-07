using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AlgebraBalancer.Substitute;
public class AnonymousFormula : ISubstitutible
{
    private AnonymousFormula(string[] parameterNames, SubstitutableString definition)
    {
        this.parameterNames = parameterNames;
        this.definition = definition;
    }

    public static readonly Regex rxArgList =
        new(@$"(?:(?'args'{Variable.rxName})(?:(?(wrapper)[,\s]|\s)\s*(?'args'{Variable.rxName}))*)",
            RegexOptions.Compiled);

    private static readonly Regex rxArgDefSep =
        new(@"\s*(?:=>|⇒|\|->|↦)\s*",
            RegexOptions.Compiled);

    private static readonly Regex rxDef =
        new(@"(?:[^()]|(?'eo'\()|(?'-eo'\)))*(?(eo)(?!))",
            RegexOptions.Compiled);

    private static readonly Regex rxAnonymousFunction =
        new(@$"(?'wrapper'\()?{rxArgList}{rxArgDefSep}(?'expr'{rxDef})(?(wrapper)\))",
            RegexOptions.Compiled);

    /// <summary>
    /// <paramref name="val"/> = "(a b => 2a-b)"
    /// </summary>
    public static AnonymousFormula TryDefine(string val)
    {
        var match = rxAnonymousFunction.Match(val);
        if (match.Success)
        {
            string[] argNames = match.Groups["args"].Captures
                .Select(x => x.Value)
                .ToArray();
            string definition = match.Groups["expr"].Value;
            return new AnonymousFormula(argNames, new SubstitutableString(definition, argNames));
        }
        return null;
    }

    public readonly string[] parameterNames;
    public readonly SubstitutableString definition;

    public string Name => string.Empty;

    /// <summary>
    /// <paramref name="capture"/> = "(6)"
    /// </summary>
    public string GetReplacement(string capture, Substitutor substitutor, int maxDepth = 20)
    {
        var match = Formula.rxParameterList.Match(capture);
        if (match.Success)
        {
            var argCaptures = match.Groups["args"].Captures;
            var callArgs = parameterNames
                .Zip(argCaptures, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => "(" + (maxDepth > 0 ? substitutor.Substitute(x.v.Value) : x.v.Value) + ")");

            return definition.GetSubstituted(callArgs);
        }
        return capture;
    }

    public Regex GetRegex() => throw new NotImplementedException();
}
