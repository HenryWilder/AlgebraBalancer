using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;

using static AlgebraBalancer.Algebra;

namespace AlgebraBalancer;

internal class Algebra
{
    public interface IAlgebraicNotation
    {
        public abstract IAlgebraicNotation Simplified();
        public abstract string ToString();
    }

    public struct Number : IAlgebraicNotation
    {
        public Number(int value) =>
            this.value = value;

        public int value;

        public readonly IAlgebraicNotation Simplified() =>
            this;

        public static implicit operator int(Number num) =>
            num.value;

        public override readonly string ToString() =>
            $"{value}";
    }

    public struct Complex : IAlgebraicNotation
    {
        // todo

        public readonly IAlgebraicNotation Simplified() =>
            throw new NotImplementedException();

        public override readonly string ToString() =>
            "𝑖";
    }

    public struct Undefined : IAlgebraicNotation
    {
        public readonly IAlgebraicNotation Simplified() => this;
        public override readonly string ToString() => "∅";
    }

    public static bool IsOdd(int n) =>
        (n & 1) != 0;

    public static List<(int a, int b)> Factors(int n)
    {
        int nAbs = Math.Abs(n);

        var factors = new List<(int, int)> { (1, n) };

        for (int i = 2; (i * i) <= nAbs; ++i)
        {
            if (nAbs % i == 0)
            {
                factors.Add((i, n / i));
            }
        }

        return factors;
    }

    public static List<(int common, int a, int b)> CommonFactors(int a, int b)
    {
        int aAbs = Math.Abs(a);
        int bAbs = Math.Abs(b);

        var factors = new List<(int, int, int)>() { (1, a, b) };

        for (int i = 2; i <= Math.Min(aAbs, bAbs); ++i)
        {
            if (aAbs % i == 0 && bAbs % i == 0)
            {
                factors.Add((i, a / i, b / i));
            }
        }

        return factors;
    }

    public static int GCF(int a, int b)
    {
        int aAbs = Math.Abs(a);
        int bAbs = Math.Abs(b);

        for (int gcf = Math.Min(aAbs, bAbs); gcf > 1; --gcf)
        {
            if (aAbs % gcf == 0 && bAbs % gcf == 0)
            {
                return gcf;
            }
        }

        return 1;
    }

    public static int LCM(int a, int b)
    {
        int aAbs = Math.Abs(a);
        int bAbs = Math.Abs(b);

        int product = a * b;
        for (int lcm = Math.Max(aAbs, bAbs); lcm < product; ++lcm)
        {
            if (lcm % aAbs == 0 && lcm % bAbs == 0)
            {
                return lcm;
            }
        }

        return product;
    }

    public struct Fraction : IAlgebraicNotation
    {
        public Fraction() { }
        public Fraction(int numerator, int denominator) =>
            (this.numerator, this.denominator) = (numerator, denominator);

        public int numerator = 1;
        public int denominator = 1;

        public override readonly string ToString() =>
            denominator == 1
                ? $"{numerator}"
                : $"{numerator}/{denominator}";

        public readonly IAlgebraicNotation Simplified()
        {
            if (denominator == 0)
            {
                return new Undefined();
            }

            if (numerator % denominator == 0)
            {
                return new Number(numerator / denominator);
            }

            int sign = (numerator < 0 != denominator < 0) ? -1 : 1;

            int numeratorAbs = Math.Abs(numerator);
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

    public struct Radical : IAlgebraicNotation
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
            foreach (var (a, b) in Factors(n))
            {
                if (SqrtI(a) is int aRoot && aRoot > gpsFactor)
                {
                    (gpsFactor, gpsMultip) = (aRoot, b);
                }
                else if (SqrtI(b) is int bRoot && bRoot > gpsFactor)
                {
                    (gpsFactor, gpsMultip) = (bRoot, a);
                }
            }

            return new Radical(gpsFactor, gpsMultip);
        }
    }

    public struct RadicalFraction : IAlgebraicNotation
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
                var radNumerator = new Radical(fracCoefficient.numerator, numerator.radicand);
                return new RadicalFraction(radNumerator, fracCoefficient.denominator);
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
}
