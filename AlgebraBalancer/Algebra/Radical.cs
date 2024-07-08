using System;

using AlgebraBalancer.Notation;

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
        // Radicand is not negative if it is 1 or 0
        // It is also not negative if it is multiplied by 0
        if (coefficient == 0 || radicand == 0) return "0";
        else if (radicand == 1) return coefficient.ToString();

        // Negative radicand represents an imaginary coefficient
        IAlgebraicNotation coef = radicand < 0
            // Saves having to copy-paste the imaginary number notation code here
            ? new Imaginary(coefficient)
            : new Number(coefficient);

        // Special case where radicand is not shown even though it is neither 1 nor 0
        // Simultaneously covers the case where the coefficient is shown despite being 1
        if (radicand == -1) return coef.ToString();

        // Coefficient is not shown when it is 1 (1*i != 1)
        string coefStr = coef is Number num && num == 1 ? "" : coef.ToString();

        // By reaching this point, the radicand is not 1 and so must need a radical
        // Coefficient may be an empty string
        return $"{coefStr}√{Math.Abs(radicand)}";
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
