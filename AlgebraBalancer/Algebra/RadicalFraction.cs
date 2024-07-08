using System;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public struct RadicalFraction : IAlgebraicExpression
{
    public RadicalFraction() { }

    public RadicalFraction(Radical numerator, int denominator) =>
        (this.numerator, this.denominator) = (numerator, denominator);

    public RadicalFraction(int numerator, Radical denominator) =>
        (this.numerator, this.denominator) = (denominator * numerator, denominator.Squared());

    public RadicalFraction(int addSubNumerator, Radical numerator, int denominator) =>
        (this.addSubNumerator, this.numerator, this.denominator) = (addSubNumerator, numerator, denominator);

    public int addSubNumerator = 0;
    public Radical numerator = default;
    public int denominator = 1;

    public override readonly string ToString()
    {
        bool isQuadratic = addSubNumerator != 0;
        bool isIntegral = denominator == 1;
        string extendedNumerator = isQuadratic
            ? $"{addSubNumerator}±{numerator}"
            : $"{numerator}";

        return isIntegral
            ? extendedNumerator
            : $"({extendedNumerator})/{denominator}";
    }

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Simplified()
    {
        var coefficient = new Fraction(numerator.coefficient, denominator).Simplified();
        if (coefficient is Fraction fracCoefficient)
        {
            var radNumerator = new Radical((Number)fracCoefficient.numerator, numerator.radicand);
            return new RadicalFraction(radNumerator, (Number)fracCoefficient.denominator);
        }
        else if (coefficient is Number numCoefficient)
        {
            return new Radical(numCoefficient, numerator.radicand);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
