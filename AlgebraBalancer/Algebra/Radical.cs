using System;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.Bald;

namespace AlgebraBalancer.Algebra;
/// <summary>
/// Imaginary radical is represented by a negative radicand
/// </summary>
public struct Radical : IAlgebraicExpression
{
    public Radical() { }

    public Radical(int radicand) =>
        this.radicand = radicand;

    public Radical(int coefficient, int radicand) =>
        (this.coefficient, this.radicand) = (coefficient, radicand);

    public int coefficient = 1;
    public int radicand = 1;

    public static Radical operator *(Radical radical, int mult) =>
        new() { coefficient = radical.coefficient * mult, radicand = radical.radicand };

    public readonly int Squared() =>
        coefficient * coefficient * radicand;

    public override readonly string ToString()
    {
        if (coefficient == 0 || radicand == 0) return 0.ToString();
        else if (radicand == 1) return coefficient.ToString();

        string integerPart = coefficient == 1 ? "" : (radicand < 0
            ? new Imaginary(coefficient)
            : new Number(coefficient)
            as IAlgebraicNotation
        ).ToString();

        return $"{integerPart}√{Math.Abs(radicand)}";
    }

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Simplified()
    {
        switch (radicand)
        {
            case -1: return new Imaginary(coefficient);
            case 0: return new Number(0);
            case 1: return new Number(coefficient);
        }

        // Simple
        var sqrtValue = ExactMath.Sqrt(radicand);
        if (sqrtValue is Number real)
        {
            return new Number(coefficient * real);
        }
        else if (sqrtValue is Imaginary imag)
        {
            return coefficient * imag;
        }

        var primeFactors = ExactMath.PrimeFactors(radicand);
        int newCoefficient = coefficient;
        int newRadicand = radicand;
        foreach (var (prime, exponent) in primeFactors)
        {
            int x = exponent;
            while (x >= 2)
            {
                newCoefficient *= prime;
                newRadicand /= prime * prime;
                x -= 2;
            }
        }

        return new Radical(newCoefficient, newRadicand);
    }
}
