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
    public string Name { get; }

    private static readonly Regex rxAnonymousFunction =
        new(@$"\((?:(?'arg1'{Variable.rxName})(?:\s+(?'argn'{Variable.rxName}))*)\s*(?:=>|⇒|\|->|↦)\s*(?'expr'(?:[^()]|(?'eo'\()|(?'-eo'\)))*(?(eo)(?!)))\)\((?'call'(?:[^()]|(?'co'\()|(?'-co'\)))*(?(co)(?!)))\)",
        RegexOptions.Compiled);

    public string GetReplacement(string capture) => throw new NotImplementedException();

    //public static string AnonymousCalls(string expr, int allowedSubCalls = 20)
    //{
    //    return CleanParentheses(rxAnonymousFunction.Replace(expr, (Match match) =>
    //    {
    //        string[] defArgs = match.Groups["argn"].Captures
    //            .Select(x => x.Value)
    //            .Prepend(match.Groups["arg1"].Value)
    //            .ToArray();
    //        string def = match.Groups["expr"].Value;
    //        string[] callArgs = match.Groups["call"].Value.Split(",");

    //        for (int i = 0; i < Math.Min(defArgs.Length, callArgs.Length); ++i)
    //        {
    //            string defArg = defArgs[i]; // Should already be trimmed by regex
    //            string callArg = AnonymousCalls(callArgs[i].Trim(), allowedSubCalls - 1);
    //            def = CleanParentheses(def.Replace(defArg, $"({callArg})"));
    //        }

    //        return "(" + (allowedSubCalls > 0 ? AnonymousCalls(def, allowedSubCalls - 1) : def) + ")";
    //    }));
    //}
}
