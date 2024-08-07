﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        new(@"\s*(?:\s(?:be(?:ing)?|is)\s|:=|≔|(?<!:)=)\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxClause =
        new(@$"(?'key'{rxKey}){rxKeyValSep}(?'val'{rxVal})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxClauseSep =
        new(@"\s*(?:\sand\s|;|,)\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex rxStatement =
        new(@$"^\s*(?:(?'clause'{rxClause})(?:{rxClauseSep}(?'clause'{rxClause}))*)\s*$",
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

    public static List<(string key, string value)> ParseDefines(string document, int startOfLine, int endOfLine, out int newEndOfLine)
    {
        List<(string key, string value)> items = [];

        string lineBeingOperated = document.Substring(startOfLine, endOfLine - startOfLine);
        var withStatement = rxWith.Match(lineBeingOperated);
        if (withStatement.Success)
        {
            string statement = withStatement.Groups["statement"].Value;
            items.AddRange(GetStatementClauses(statement).Reverse());
            newEndOfLine = startOfLine + withStatement.Index;
        }
        else
        {
            newEndOfLine = endOfLine;
        }

        string[] linesBefore = document
            .Substring(0, Math.Max(startOfLine - 1, 0))
            .Split("\r");

        foreach (string line in linesBefore.Reverse())
        {
            var letStatement = rxLet.Match(line);
            if (letStatement.Success)
            {
                string statement = letStatement.Groups["statement"].Value;
                items.AddRange(GetStatementClauses(statement).Reverse());
            }
        }

        return items;
    }

    public static List<ISubstitutible> Parse(string document, int startOfLine, int endOfLine, out int newEndOfLine)
    {
        var items = ParseDefines(document, startOfLine, endOfLine, out int parsedEndOfLine);
        newEndOfLine = parsedEndOfLine;

        List<ISubstitutible> substitutions = [];

        foreach (var (key, val) in items)
        {
            ISubstitutible sub;
            if (AnonymousFormula.TryDefine(val) is AnonymousFormula a and not null)
            {
                sub = a;
            }
            else if (MappedFormula.TryDefine(key, val) is MappedFormula m and not null)
            {
                sub = m;
            }
            else if (Formula.TryDefine(key, val) is Formula f and not null)
            {
                sub = f;
            }
            else if (Variable.TryDefine(key, val) is Variable x and not null)
            {
                sub = x;
            }
            else
            {
                continue;
            }

            if (!substitutions.Exists(x => x.Name == sub.Name))
            {
                substitutions.Add(sub);
            }
        }

        return substitutions;
    }
}
