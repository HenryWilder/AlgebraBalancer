using System;

using AlgebraBalancer.Notation;

using static AlgebraBalancer.Notation.IAlgebraicNotation;

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

    public readonly NotationKind Kind => NotationKind.Radical;

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

    public readonly IAlgebraicNotation Add(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Sub(IAlgebraicNotation rhs) => throw new NotImplementedException();
    public readonly IAlgebraicNotation Mul(IAlgebraicNotation rhs)
    {
        if (rhs is Number num)
        {
            return new Radical(coefficient * num, radicand);
        }
        else if (rhs is Fraction frac)
        {
            return new RadicalFraction(new Radical(coefficient * frac.numerator, radicand), frac.denominator);
        }
        else if (rhs is Radical rad)
        {
            return new Radical(coefficient * rad.coefficient, radicand * rad.radicand);
        }
        else if (rhs is Imaginary imag)
        {
            return new Radical(coefficient * imag.coef, -radicand);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public readonly IAlgebraicNotation Div(IAlgebraicNotation rhs)
    {
        if (rhs is Number num)
        {
            return new RadicalFraction(this, num);
        }
        else if (rhs is Fraction frac)
        {
            return new RadicalFraction(new Radical(coefficient * frac.denominator, radicand), frac.numerator);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public readonly IAlgebraicNotation Pow(int exponent)
    {
        switch (exponent)
        {
            case 0: return (Number)1;
            case 1: return this;
        }

        // Negative exponent
        if (exponent < 0)
        {
            return Pow(-exponent).Reciprocal();
        }
        // Positive exponent
        else
        {
            var powCoef = ExactMath.Power(coefficient, exponent);
            if (powCoef is Number powCoefNum)
            {
                int rationalPart = exponent / 2;
                var powRad = ExactMath.Power(radicand, rationalPart);
                if (powRad is Number powRadNum)
                {
                    int irrationalPart = exponent % 2;
                    int newCoef = powCoefNum * powRadNum;
                    return irrationalPart == 0 
                        ? (Number)newCoef
                        : new Radical(newCoef);
                }
                else
                {
                    return powRad;
                }
            }
            else
            {
                return powCoef;
            }
        }
    }
    public readonly IAlgebraicNotation Neg() => new Radical(-coefficient, radicand);
    public readonly IAlgebraicNotation Reciprocal() => new RadicalFraction(1, this);
}
