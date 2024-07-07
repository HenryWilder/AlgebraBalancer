using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public static class SubstitutionParser
{
    private static readonly Regex rxKey =
        new(@"(?:[^(){}[\]]|(?'open'[({\[])|(?'-open'[)}\]]))+?(?(open)(?!))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxVal =
        new(@"(?:[^(){}[\]]|(?'open'[({\[])|(?'-open'[)}\]]))+?(?(open)(?!))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxKeyValSep =
        new(@"\s+(?:(be(?:ing)?|is)|:=|≔|(?<!:)=)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxClause =
        new(@$"(?'key'{rxKey}){rxKeyValSep}(?'val'{rxVal})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxClauseSep =
        new(@"\s*(?:\sand\s|;|,)\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxStatement =
        new(@$"^(?:(?'clause'{rxClause})(?:{rxClauseSep}(?'clause'{rxClause}))*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxLet =
        new(@"^let\s+(?'statement'.+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxWith =
        new(@"\s+(?:with|for|using|where|in which|given(?: that)?)\s+(?'statement'.+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static (string key, string value)[] GetStatementClauses(string statement)
    {
        var match = rxStatement.Match(statement);
        var keys = match.Groups["key"].Captures;
        var vals = match.Groups["val"].Captures;
        return keys.Zip(vals, (k, v) => (k.Value, v.Value)).ToArray();
    }

    public static List<(string key, string value)> ParseDefines(string document, int startOfLine, int endOfLine)
    {
        List<(string key, string value)> items = [];

        string[] lines = document
            .Substring(0, Math.Max(startOfLine - 1, 0))
            .Split("\r");

        foreach (string line in lines.Reverse())
        {
            var letStatement = rxLet.Match(line);
            if (letStatement.Success)
            {
                string statement = letStatement.Groups["statement"].Value;
                items.AddRange(GetStatementClauses(statement));
            }
        }

        string lineBeingOperated = document.Substring(startOfLine, endOfLine - startOfLine);
        var withStatement = rxWith.Match(lineBeingOperated);
        if (withStatement.Success)
        {
            string statement = withStatement.Groups["statement"].Value;
            items.AddRange(GetStatementClauses(statement));
        }

        return items;
    }

    public static List<ISubstitutible> Parse(string document, int startOfLine, int endOfLine)
    {
        var items = ParseDefines(document, startOfLine, endOfLine);

        List<ISubstitutible> substitutions = [];

        // todo

        return substitutions;
    }
}
