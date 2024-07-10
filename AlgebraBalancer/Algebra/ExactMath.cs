using System;
using System.Collections.Generic;
using System.Linq;

using AlgebraBalancer.Algebra;
using AlgebraBalancer.Notation;

using static AlgebraBalancer.Notation.Bald;

namespace AlgebraBalancer;

public class ExactMath
{
    private static IAlgebraicAtomic PossiblyHugeAccumulation(int[] values, Func<int, int, int> operation)
    {
        try
        {
            checked
            {
                int accumulation = values[0];
                foreach (int value in values.Skip(1))
                {
                    accumulation = operation(accumulation, value);
                }
                return new Number(accumulation);
            }
        }
        catch (OverflowException)
        {
            return HUGE;
        }
    }

    public static IAlgebraicAtomic Sum(params int[] values)
    {
        return PossiblyHugeAccumulation(values, (a, b) => a + b);
    }

    public static IAlgebraicAtomic Product(params int[] values)
    {
        return PossiblyHugeAccumulation(values, (a, b) => a * b);
    }

    public static IAlgebraicNotation Power(int basePart, int exponent)
    {
        bool isNegativeExponent = exponent < 0;
        int power = 1;

        try
        {
            checked
            {
                for (int i = 0; i < Math.Abs(exponent); ++i)
                {
                    power *= basePart;
                }
            }
        }
        catch (OverflowException)
        {
            return isNegativeExponent ? TINY : HUGE;
        }

        return isNegativeExponent ? new Algebra.Fraction(1, power) : new Number(power);
    }

    public static int UncheckedUIntPower(int basePart, uint exponent)
    {
        int power = basePart;
        for (uint i = 1; i < exponent; ++i)
        {
            power *= basePart;
        }
        return power;
    }

    public static bool IsOdd(int n) => (n & 1) != 0;

    public static List<(int common, int[] associated)> CommonFactors(params int[] parameters)
    {
        int[] absParams = parameters.Select(Math.Abs).ToArray();

        var factors = new List<(int, int[])>() { (1, parameters) };

        for (int i = 2; i <= absParams.Min(); ++i)
        {
            if (absParams.All((x) => x % i == 0))
            {
                factors.Add((i, parameters.Select((x) => x / i).ToArray()));
            }
        }

        return factors;
    }

    public static List<(int common, int associated)> Factors(int n) =>
        CommonFactors(n).Select((x) => (x.common, x.associated[0])).ToList();

    public static List<(int prime, int exponent)> PrimeFactors(int n)
    {
        if (n < 0)
        {
            var positiveFactorization = PrimeFactors(-n);
            positiveFactorization.Insert(0, (-1, 1));
            return positiveFactorization;
        }

        var factors = Factors(n);

        if (factors.Count > 2)
        {
            var (a, b) = factors[1];
            var primes = PrimeFactors(b);
            primes.Insert(0, (a, 1));
            return primes
                .Select(x => x.prime)
                .Distinct()
                .Select(p =>
                    (p, primes
                        .Where(x => x.prime == p)
                        .Sum(x => x.exponent)
                    )
                ).ToList();
        }
        else
        {
            return [(n, 1)];
        }
    }

    public static int GCF(List<(int common, int[] associated)> commonFactors)
    {
        return commonFactors.Last().common;
    }

    public static int GCF(params int[] parameters)
    {
        return GCF(CommonFactors(parameters));
    }

    public static IAlgebraicNotation LCM(List<(int common, int[] associated)> commonFactors, params int[] parameters)
    {
        int gcf = GCF(commonFactors);

        var product = Product(parameters);

        if (product is Number prod)
        {
            return new Number(Math.Abs(prod) / gcf);
        }
        else
        {
            return product;
        }
    }

    public static IAlgebraicNotation LCM(params int[] parameters)
    {
        return LCM(CommonFactors(parameters), parameters);
    }

    public static bool IsPrime(int n) => Factors(n).Count == 2; // 1*n and n*1

    // todo: index currently does nothing
    public static IAlgebraicNotation Sqrt(int n, int index = 2)
    {
        // Super easy, barely an inconvenience
        if (n is 0 or 1) return new Number(n);

        var primeFactors = PrimeFactors(n);

        // Perfect square
        if (primeFactors.Count == 1 && primeFactors[0].exponent % 2 == 0)
        {
            var (prime, exponent) = primeFactors[0];
            if (Power(prime, exponent / 2) is Number num)
            {
                return num;
            }
        }
        // (Perfect) Square root of a negative
        else if (primeFactors.Count == 2 && primeFactors[0].prime == -1 && primeFactors[1].exponent % 2 == 0)
        {
            var (prime, exponent) = primeFactors[1];
            if (Power(prime, exponent / 2) is Number num)
            {
                return new Imaginary(num);
            }
        }

        return new Radical(n);
    }

    public static MultipleSolutions Quadratic(int a, int b, int c)
    {
        //tex:$$-b$$
        int leadingTerm = -b;

        //tex:$$\sqrt{b^2 - 4ac}$$
        var radical = new Radical(b * b - 4 * a * c);

        //tex:$$2a$$
        int denominator = 2 * a;

        //tex:$$\frac{-b+\sqrt{b^2-4ac}}{2a}$$
        var solution1 = (leadingTerm + radical) / denominator;

        //tex:$$\frac{-b-\sqrt{b^2-4ac}}{2a}$$
        var solution2 = (leadingTerm + -radical) / denominator;

        return new MultipleSolutions(solution1.Simplified(), solution2.Simplified());
    }

    public struct VertexForm(int a, IAlgebraicNotation h, IAlgebraicNotation k)
    {
        public int a = a;
        public IAlgebraicNotation h = h;
        public IAlgebraicNotation k = k;

        public override readonly string ToString() => $"{a}(𝑥-{h})²+{k}";
    }

    public static VertexForm CompleteSquare(int a, int b, int c)
    {
        var negB2ASquared = -new Algebraic(new Fraction(b, 2 * a)).Squared();

        return new(a, negB2ASquared.Simplified(), (c + negB2ASquared).Simplified());
    }

    public static RadicalFraction ImaginaryFraction(Imaginary numerator, int denominator)
    {
        return new(new Radical(numerator.coef, -1), denominator);
    }

    public static IAlgebraicNotation ImaginaryFraction(int numerator, Imaginary denominator)
    {
        var coef = new Fraction(numerator, denominator.coef).Simplified();

        if (coef is Number num)
        {
            return new Imaginary(num);
        }
        else if (coef is Fraction frac)
        {
            return ImaginaryFraction(new Imaginary(frac.numerator), frac.denominator);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
