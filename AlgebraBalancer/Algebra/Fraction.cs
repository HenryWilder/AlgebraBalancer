using System;
using System.Linq;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public class Fraction(int numerator = 1, int denominator = 1) : IAlgebraicExpression
{
    public int numerator = numerator;
    public int denominator = denominator;

    public bool IsInoperable => false;

    public override string ToString() =>
        $"{numerator}/{denominator}";

    public string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public IAlgebraicNotation Simplified()
    {
        if (denominator == 0)
        {
            return Bald.UNDEFINED;
        }

        if (numerator % denominator == 0)
        {
            return (Number)(numerator / denominator);
        }

        int[] gcfAssociated = ExactMath
            .CommonFactors(
                Math.Abs(numerator),
                Math.Abs(denominator))
            .Last()
            .associated;

        int simplifiedNumerator = gcfAssociated[0];
        int simplifiedDenominator = gcfAssociated[1];

        return new Fraction(
            (numerator < 0 != denominator < 0)
                ? -simplifiedNumerator
                : simplifiedNumerator,
            simplifiedDenominator);
    }

    public static Fraction operator -(Fraction rhs) => new(-rhs.numerator, rhs.denominator);
}
