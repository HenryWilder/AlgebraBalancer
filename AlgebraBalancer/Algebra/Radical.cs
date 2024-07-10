using System;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
/// <summary>
/// Imaginary radical is represented by a negative radicand
/// </summary>
public class Radical : IAlgebraicExpression
{
    public Radical() { }

    public Radical(int radicand) =>
        this.radicand = radicand;

    public Radical(int coefficient, int radicand) =>
        (this.coefficient, this.radicand) = (coefficient, radicand);

    public Radical(Imaginary imag) =>
        (coefficient, radicand) = (imag.coef, -1);

    public int coefficient = 1;
    public int radicand = 1;

    public bool IsInoperable => false;

    public int Squared() =>
        coefficient * coefficient * radicand;

    public override string ToString()
    {
        // Radicand is not negative if it is 1 or 0
        // It is also not negative if it is multiplied by 0
        if (IsZero()) return "0";
        else if (IsInteger()) return coefficient.ToString();

        // Negative radicand represents an imaginary coefficient
        IAlgebraicNotation coef = IsImaginary()
            // Saves having to copy-paste the imaginary number notation code here
            ? new Imaginary(coefficient)
            : new Number(coefficient);

        // Special case where radicand is not shown even though it is neither 1 nor 0
        // Simultaneously covers the case where the coefficient is shown despite being 1
        if (IsImaginary() && IsRational()) return coef.ToString();

        // Coefficient is not shown when it is 1 (1*i != 1)
        string coefStr = (IsReal() && IsPureRadical()) ? "" : coef.ToString();

        // By reaching this point, the radicand is not 1 and so must need a radical
        // Coefficient may be an empty string
        return $"{coefStr}√{Math.Abs(radicand)}";
    }

    public string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public IAlgebraicNotation Simplified()
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

    public static Radical operator *(Radical lhs, int rhs) => new(lhs.coefficient * rhs, lhs.radicand);
    public static Radical operator *(int lhs, Radical rhs) => new(lhs * rhs.coefficient, rhs.radicand);
    public static Radical operator *(Radical lhs, Radical rhs) => new(lhs.coefficient * rhs.coefficient, lhs.radicand * rhs.radicand);
    public static Radical operator -(Radical rhs) => new(-rhs.coefficient, rhs.radicand);
    public static RadicalFraction operator /(Radical lhs, int rhs) => new(lhs, rhs);
    public static RadicalFraction operator /(int lhs, Radical rhs) => new(lhs, rhs);
    public static SumOfRadicals operator +(Radical lhs, Radical rhs) => new([lhs, rhs]);
    public static SumOfRadicals operator +(Radical lhs, int rhs) => new([lhs, new(rhs, 1)]);
    public static SumOfRadicals operator +(int lhs, Radical rhs) => new([new(lhs, 1), rhs]);


    /// <summary>Simplifies to int or imaginary</summary>
    public bool IsRational() => radicand is 1 or -1;
    /// <summary>Simplifies to int</summary>
    public bool IsInteger() => radicand == 1;
    /// <summary>Coefficient of 1</summary>
    public bool IsPureRadical() => coefficient == 1;
    /// <summary>Equal to zero</summary>
    public bool IsZero() => coefficient == 0 || radicand == 0;
    /// <summary>Square root of negative</summary>
    public bool IsImaginary() => radicand < 0;
    /// <summary>Square root of positive</summary>
    public bool IsReal() => radicand >= 0;
}
