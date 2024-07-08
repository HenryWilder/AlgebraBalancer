using System;
using System.Text.RegularExpressions;

namespace AlgebraBalancer.Substitute;
public class PiecewiseFormula : ISubstitutible
{
    private PiecewiseFormula(string name, (Func<string, bool>, string)[] cases)
    {
        this.name = name;
        this.cases = cases;
    }

    /// <summary>
    /// <paramref name="key"/> = "f(a, b)"
    /// <paramref name="val"/> = "{ a if a > b; b otherwise }"
    /// </summary>
    public MappedFormula TryDefine(string key, string val)
    {
        return null;
    }

    public readonly string name;
    public readonly (Func<string, bool>, string)[] cases;

    public string Name => name;

    /// <summary>
    /// <paramref name="capture"/> = "f(5, 3)"
    /// </summary>
    public string GetReplacement(string capture, Substitutor substitutor, int maxDepth = 20)
    {
        return capture;
    }

    public Regex GetRegex() => throw new NotImplementedException();
}
