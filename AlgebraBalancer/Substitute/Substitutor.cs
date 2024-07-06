using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public class Substitutor(string document, int lineStart, int lineEnd)
{
    private readonly ISubstitutible[] substitutions = [..SubstitutionParser.Parse(document, lineStart, lineEnd)];
    private readonly string expr = document.Substring(lineStart, lineEnd - lineStart);

    public string Substitute(int allowedDepth = 100)
    {
        //foreach (var substitution in substitutions)
        //{
        //    var signatureMatch = rxFunction.Match(variable);
        //    if (signatureMatch.Success)
        //    {
        //        string functionName =
        //            signatureMatch.Groups[1].Value;

        //        string[] signatureParamList =
        //            signatureMatch.Groups[2].Value
        //                .Split(",")
        //                .Select(x => x.Trim())
        //                .ToArray();

        //        string functionDefinition = value;

        //        Func<string[], string> fnReplacement;
        //        var mappedFunctionDef = rxMappedFunction.Match(functionDefinition);
        //        if (mappedFunctionDef.Success)
        //        {
        //            var inputs = mappedFunctionDef.Groups["in"].Captures;
        //            var outputs = mappedFunctionDef.Groups["out"].Captures;
        //            var mapping = inputs
        //                .Zip(outputs, (i, o) => (i.Value.Trim(), o.Value.Trim()))
        //                .ToDictionary(x => x.Item1, x => x.Item2);

        //            fnReplacement = (string[] callParams) =>
        //                mapping.TryGetValue(SolveIfTrivial(callParams[0]), out string associated)
        //                    ? associated
        //                    : "∄";
        //        }
        //        else
        //        {
        //            fnReplacement = (string[] callParams) =>
        //            {
        //                string replacement = functionDefinition;
        //                for (int i = 0; i < Math.Min(signatureParamList.Length, callParams.Length); ++i)
        //                {
        //                    string paramName = signatureParamList[i];
        //                    string paramValue = "(" + CleanParentheses(callParams[i]) + ")";
        //                    replacement = replacement.Replace(paramName, paramValue);
        //                }
        //                return replacement;
        //            };
        //        }

        //        expr = rxFunction.Replace(
        //            expr,
        //            (Match callMatch) =>
        //            {
        //                string callName = callMatch.Groups[1].Value;
        //                if (callName == functionName)
        //                {
        //                    string[] callParams;
        //                    {
        //                        string callParamListStr = callMatch.Groups[2].Value;
        //                        // Expand recursively (up to recursionsRemaining) before passing parameters
        //                        string callParamList = allowedDepth > 0
        //                            ? Substitute(callParamListStr, substitutions, allowedDepth - 1)
        //                            : callParamListStr;

        //                        string[] callParamItems;
        //                        // todo: there's probably a cleaner way to do this
        //                        if (rxBracket.IsMatch(callParamList))
        //                        {
        //                            string semicolonDelimitedParams =
        //                                callParamList.Replace(",", ";");

        //                            string minorCommaDelimitedParams =
        //                                rxContainedSemicolons.Replace(semicolonDelimitedParams, ",");

        //                            callParamItems = minorCommaDelimitedParams.Split(";");
        //                        }
        //                        else
        //                        {
        //                            callParamItems = callParamList.Split(",");
        //                        }
        //                        callParams = callParamItems
        //                            .Select(CleanParentheses)
        //                            .Select(x => x.Trim())
        //                            .ToArray();
        //                    }

        //                    return "(" + AnonymousCalls(fnReplacement(callParams)) + ")";
        //                }
        //                else
        //                {
        //                    return callMatch.Groups[0].Value;
        //                }
        //            }
        //        );
        //    }
        //    else
        //    {
        //        expr = expr.Replace(variable, $"({value})");
        //    }
        //}
        //return CleanParentheses(expr);

        return "";
    }
}
