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

    /// <summary>
    /// <paramref name="capture"/> = "f(6)"
    /// </summary>
    public string GetReplacement(string capture)
    {
        var match = Formula.rxParameterList.Match(capture);
        if (match.Success)
        {
            // Expects only one capture
            string lookupKey = match.Groups["args"].Value;
            return mapping.TryGetValue(lookupKey, out string value)
                ? value
                : "∄"; 
        }
        return capture;
    }
}
