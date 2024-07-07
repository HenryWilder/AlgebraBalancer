using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class MappedFormula : ISubstitutible
{
    private MappedFormula(string name, Dictionary<string, string> mapping)
    {
        this.name = name;
        this.mapping = mapping;
    }

    private static readonly Regex rxMappedFormula =
        new(@"\{(?:\s*(?'in'.+?)\s*(?:\|?->|↦|→)\s*(?'out'[^(),]+|\((?:[^()]|(?'open'\()|(?'-open'\)))*(?(open)(?!))\)),?)+\s*\}",
            RegexOptions.Compiled);

    /// <summary>
    /// <paramref name="key"/> = "f(a)"
    /// <paramref name="val"/> = "{5->2,6->3}"
    /// </summary>
    public static MappedFormula TryDefine(string key, string val)
    {
        var nameMatch = Variable.rxName.Match(key);
        var mappingMatch = rxMappedFormula.Match(val);
        if (nameMatch.Success && mappingMatch.Success)
        {
            var inputs = mappingMatch.Groups["in"].Captures;
            var outputs = mappingMatch.Groups["out"].Captures;
            var dict = inputs
                .Zip(outputs, (k, v) => (k.Value, v.Value))
                .ToDictionary(x => x.Item1, x => x.Item2);
            return new MappedFormula(nameMatch.Value, dict);
        }
        return null;
    }

    public readonly string name;
    public readonly Dictionary<string, string> mapping;

    public string Name => name;

    /// <summary>
    /// <paramref name="capture"/> = "f(6)"
    /// </summary>
    public string GetReplacement(string capture, Substitutor substitutor, int maxDepth = 20)
    {
        var match = Formula.rxParameterList.Match(capture);
        if (match.Success)
        {
            // Expects only one capture
            string lookupKey = match.Groups["args"].Value;
            lookupKey = ParenCleaner.CleanParentheses(maxDepth > 0 ? substitutor.Substitute(lookupKey, maxDepth) : lookupKey);
            bool solved = Solver.TrySolveInteger(lookupKey, out string solution);
            return mapping.TryGetValue(solved ? solution : lookupKey, out string value)
                ? "(" + (maxDepth > 0 ? substitutor.Substitute(value, maxDepth) : value) + ")"
                : "(∄)"; 
        }
        return capture;
    }

    public Regex GetRegex() => new(name + Formula.rxParameterList);
}
