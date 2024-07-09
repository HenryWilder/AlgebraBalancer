using System;
using System.Linq;

using AlgebraBalancer.Notation;

using static AlgebraBalancer.Notation.IAlgebraicNotation;

namespace AlgebraBalancer.Algebra;
public struct Fraction(int numerator = 1, int denominator = 1) : IAlgebraicExpression
{
    public int numerator = numerator;
    public int denominator = denominator;

    public readonly NotationKind Kind => NotationKind.Fraction;

    public override readonly string ToString() =>
        $"{numerator}/{denominator}";

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Simplified()
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

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => new Fraction(
                numerator + num * denominator,
                denominator),
            Fraction frac => new Fraction(
                numerator * frac.denominator + frac.numerator * denominator,
                denominator * frac.denominator),
            _ => throw new NotImplementedException("Can't add radical fractions"),
        };
    }
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => new Fraction(
                numerator + num * denominator,
                denominator),
            Fraction frac => new Fraction(
                numerator * frac.denominator - frac.numerator * denominator,
                denominator * frac.denominator),
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => new Fraction(
                numerator * num,
                denominator),
            Fraction frac => new Fraction(
                numerator * frac.numerator,
                denominator * frac.denominator),
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        return rhs switch
        {
            Number num => new Fraction(
                numerator * num,
                denominator),
            Fraction frac => new Fraction(
                numerator * frac.denominator,
                denominator * frac.numerator),
            _ => throw new NotImplementedException(),
        };
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
        var newNumerator = ExactMath.Power(numerator, exponent);
        var newDenominator = ExactMath.Power(denominator, exponent);
        return newNumerator.Div(newDenominator);
    }
    public readonly IAlgebraicNotation Neg()
    {
        return new Fraction(-numerator, denominator);
    }
    public readonly IAlgebraicNotation Reciprocal() => new Fraction(denominator, numerator);

    public static Fraction operator -(Fraction rhs) => new(-rhs.numerator, rhs.denominator);
}
