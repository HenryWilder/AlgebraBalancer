using System;
using System.Data;
using System.Text.RegularExpressions;

namespace AlgebraBalancer;
public static class Solver
{
    private static readonly DataTable dt = new();
    private static readonly Regex rxImpliedMul = new(@"(?<=\))\s*(?=[\d\(])|(?<=[\d\)])\s*(?=\()");

    public static bool TrySolveInteger(string expr, out string result)
    {
        // Smallest we expect to display
        const double EPSILON = 0.000000000000001;

        try
        {
            object computed = dt.Compute(rxImpliedMul.Replace(expr, "*"), "");
            if (computed is not null)
            {
                double computedDouble = Convert.ToDouble(computed);

                // Only integers are "trivial" here
                if (Math.Abs(computedDouble - Math.Round(computedDouble)) < EPSILON)
                {
                    result = Convert.ToInt64(computed).ToString();
                    return true;
                }
                else
                {
                    result = "not an integer";
                }
            }
            else
            {
                result = "null";
            }
        }
        catch (Exception err)
        {
            result = err.Message;
        }

        return false;
    }

    public static bool TrySolveDouble(string expr, out string result)
    {
        try
        {
            object computed = dt.Compute(rxImpliedMul.Replace(expr, "*"), "");
            if (computed is not null)
            {
                result = Convert.ToDouble(computed).ToString();
                return true;
            }
            else
            {
                result = "null";
            }
        }
        catch (Exception err)
        {
            result = err.Message;
        }

        return false;
    }
}
