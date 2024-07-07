using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public string name;
    private (Func<string, bool>, string)[] cases;

    /// <summary>
    /// <paramref name="capture"/> = "f(5, 3)"
    /// </summary>
    public string GetReplacement(string capture)
    {
        return capture;
    }
}
