using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class MappedFormula : ISubstitutible
{
    public string Name { get; }

    private static readonly Regex rxMappedFormula =
        new(@"\{(?:\s*(?'in'.+?)\s*(?:\|?->|↦|→)\s*(?'out'[^(),]+|\((?:[^()]|(?'open'\()|(?'-open'\)))*(?(open)(?!))\)),?)+\s*\}",
            RegexOptions.Compiled);

    public string GetReplacement(string capture) => throw new NotImplementedException();
}
