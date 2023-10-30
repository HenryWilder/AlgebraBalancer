using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;

namespace AlgebraBalancer;

public class Algebra
{
    public interface IAlgebraicNotation
    {
        public abstract string ToString();
    }

    public interface IAlgebraicExpression : IAlgebraicNotation
    {
        public abstract IAlgebraicNotation Simplified();
    }

    public interface IAlgebraicAtomic : IAlgebraicNotation { }

    public struct Number : IAlgebraicAtomic
    {
        public Number(int value) =>
            this.value = value;

        public int value;

        public static implicit operator int(Number num) => num.value;
        public static implicit operator Number(int value) => new(value);
        public override readonly string ToString() => $"{value}";
    }

    public struct Complex : IAlgebraicAtomic
    {
        public override readonly string ToString() => "𝑖";
    }

    public struct Undefined : IAlgebraicAtomic
    {
        public override readonly string ToString() => "∅";
    }
    public static Undefined undefined = new();

    public struct Huge : IAlgebraicAtomic
    {
        public override readonly string ToString() => "𝓗";
    }
    public static Huge huge = new ();

    public struct Epsilon : IAlgebraicAtomic
    {
        public override readonly string ToString() => "ε";
    }
    public static Epsilon epsilon = new();

    public static bool IsOdd(int n) =>
        (n & 1) != 0;

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

    public static int GCF(params int[] parameters)
    {
        int[] absParams = parameters.Select(Math.Abs).ToArray();

        for (int gcf = absParams.Min(); gcf > 1; --gcf)
        {
            if (absParams.All((x) => gcf % x == 0))
            {
                return gcf;
            }
        }

        return 1;
    }

    public static IAlgebraicNotation LCM(params int[] parameters)
    {
        int[] absParams = parameters.Select(Math.Abs).ToArray();

        int product;
        try
        {
            checked
            {
                product = parameters.Aggregate(1, (total, next) => total * next);
            }
        }
        catch (OverflowException)
        {
            return huge;
        }

        for (int lcm = absParams.Max(); lcm < product; ++lcm)
        {
            if (absParams.All((x) => lcm % x == 0))
            {
                return new Number(lcm);
            }
        }

        return new Number(product);
    }

    public struct Fraction : IAlgebraicExpression
    {
        public Fraction() { }

        public Fraction(int numerator, int denominator) =>
            (this.numerator, this.denominator) = (new Number(numerator), new Number(denominator));

        public Fraction(IAlgebraicAtomic numerator, IAlgebraicAtomic denominator) =>
            (this.numerator, this.denominator) = (numerator, denominator);

        public IAlgebraicAtomic numerator   = new Number(1);
        public IAlgebraicAtomic denominator = new Number(1);

        public override readonly string ToString() =>
            $"{numerator}/{denominator}";

        public readonly IAlgebraicNotation Simplified()
        {
            if (this.numerator is null || this.denominator is null)
            {
                throw new NullReferenceException("fractions should not contain null");
            }
            if (this.numerator is Undefined || this.denominator is Undefined)
            {
                return undefined;
            }
            if (this.numerator is not Number || this.denominator is not Number)
            {
                return this;
            }

            var numerator   = (Number)this.numerator;
            var denominator = (Number)this.denominator;

            if (denominator == 0)
            {
                return new Undefined();
            }

            if (numerator % denominator == 0)
            {
                return new Number(numerator / denominator);
            }

            int sign = (numerator < 0 != denominator < 0) ? -1 : 1;

            int numeratorAbs   = Math.Abs(numerator);
            int denominatorAbs = Math.Abs(denominator);

            int gcf = GCF(numeratorAbs, denominatorAbs);
            return new Fraction(sign * numeratorAbs / gcf, denominatorAbs / gcf);
        }
    }

    public static bool IsPrime(int n) =>
        (n % 5 != 0) && (Factors(n).Count == 1);

    public static int? SqrtI(int n)
    {
        if      (n < 0) { return null; }
        else if (n < 2) { return n; }

        for (int root = 2; (root * root) <= n; ++root)
        {
            if (root * root == n) { return root; }
        }

        return null;
    }

    public struct Radical : IAlgebraicExpression
    {
        public Radical() { }

        public Radical(int radicand) =>
            this.radicand = radicand;

        public Radical(int coefficient, int radicand) =>
            (this.coefficient, this.radicand) = (coefficient, radicand);

        public int coefficient = 1;
        public int radicand    = 1;

        public static Radical operator *(Radical radical, int mult) =>
            new() { coefficient = radical.coefficient * mult, radicand = radical.radicand };

        public readonly int Squared() =>
            coefficient * coefficient * radicand;

        public override readonly string ToString()
        {
            if      (radicand    == 1) { return $"{coefficient}";            }
            else if (coefficient == 1) { return              $"√{radicand}"; }
            else                       { return $"{coefficient}√{radicand}"; }
        }

        public readonly IAlgebraicNotation Simplified()
        {
            switch (radicand)
            {
                case < 0: return new Complex(); // todo
                case 0: return new Number(0);
                case 1: return new Number(coefficient);
            }

            // Simple
            if (SqrtI(radicand) is int root)
            {
                return new Number(coefficient * root);
            }

            int n = Squared();

            // Perfect squares
            int gpsFactor = 1; // Greatest perfect square factor
            int gpsMultip = n; // Associated factor with gpsFactor
            foreach (var (common, associated) in Factors(n))
            {
                var permutations = new (int, int)[]
                {
                    (common, associated),
                    (associated, common)
                };

                foreach ((int a, int b) in permutations)
                {
                    if (SqrtI(a) is int aRoot && aRoot > gpsFactor)
                    {
                        (gpsFactor, gpsMultip) = (aRoot, b);
                    }
                }
            }

            return new Radical(gpsFactor, gpsMultip);
        }
    }

    public struct RadicalFraction : IAlgebraicExpression
    {
        public RadicalFraction() { }

        public RadicalFraction(Radical numerator, int denominator) =>
            (this.numerator, this.denominator) = (numerator, denominator);

        public RadicalFraction(int numerator, Radical denominator) =>
            (this.numerator, this.denominator) = (denominator * numerator, denominator.Squared());

        public RadicalFraction(int addSubNumerator, Radical numerator, int denominator) =>
            (this.addSubNumerator, this.numerator, this.denominator) = (addSubNumerator, numerator, denominator);

        public int     addSubNumerator = 0;
        public Radical numerator       = default;
        public int     denominator     = 1;

        public override readonly string ToString()
        {
            bool isQuadratic = addSubNumerator != 0;
            bool isIntegral  = denominator == 1;
            string extendedNumerator = isQuadratic
                ? $"{addSubNumerator}±{numerator}"
                : $"{numerator}";

            return isIntegral
                ? extendedNumerator
                : $"({extendedNumerator})/{denominator}";
        }

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
            return isNegativeExponent ? epsilon : huge;
        }

        return isNegativeExponent ? new Fraction(1, power) : new Number(power);
    }
}
