using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;

using static AlgebraBalancer.Algebra;

namespace AlgebraBalancer
{
    internal class Algebra
    {
        public static bool IsOdd(int n) =>
            (n & 1) != 0;

        public static bool IsInt(double x) =>
            Math.Abs(x % 1) <= (double.Epsilon * 100);

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

        public struct Fraction
        {
            public Fraction(int numerator = 1, int denominator = 1)
            {
                this.numerator = numerator;
                this.denominator = denominator;
            }

            public int numerator;
            public int denominator;

            public override string ToString() =>
                denominator == 1
                    ? $"{numerator}"
                    : $"{numerator}/{denominator}";
        }

        public static Fraction SimplifiedFraction(int a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }

            if (a % b == 0)
            {
                return new Fraction(numerator: a / b);
            }

            int sign = (a < 0 != b < 0) ? -1 : 1;

            int aAbs = Math.Abs(a);
            int bAbs = Math.Abs(b);

            int gcf = GCF(aAbs, bAbs);
            return new Fraction(sign * aAbs / gcf, bAbs / gcf);
        }

        static readonly int[] PrimesUnder100 =
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
        };

        public static bool IsPrime(int n) =>
            (n % 5 != 0) && (Factors(n).Count == 1);

        public static List<int> PrimeFactors(int n)
        {
            if (n == 0)
            {
                throw new DivideByZeroException();
            }

            int remaining = Math.Abs(n);

            if (remaining == 1)
            {
                return new List<int> { }; // Ø
            }

            var primeFactors = new List<int>();

            while (remaining != 1)
            {
                int primeFactor = 1;

                foreach (int prime in PrimesUnder100)
                {
                    if (remaining % prime == 0)
                    {
                        primeFactor = prime;
                        break;
                    }
                }

                // Need to brute force
                if (remaining % primeFactor != 0)
                {
                    // Iterate over odd numbers only
                    for (int prime = 101; prime < remaining && remaining % prime != 0; prime += 2)
                    {
                        if (!IsPrime(primeFactor)) { continue; }

                        if (remaining % prime == 0)
                        {
                            primeFactor = prime;
                            break;
                        }
                    }
                }

                remaining /= primeFactor;
                primeFactors.Add(primeFactor);
            }

            return primeFactors;
        }
        
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

        public static bool IsPerfectSquare(int n) =>
            SqrtI(n).HasValue;

        public struct Radical
        {
            public Radical(int coefficient = 1, int radicand = 1)
            {
                this.coefficient = coefficient;
                this.radicand = radicand;
            }

            public int coefficient;
            public int radicand;

            public static Radical operator *(Radical radical, int mult) =>
                new Radical(radical.coefficient * mult, radical.radicand);

            public int Squared() =>
                coefficient * coefficient + radicand;

            public override string ToString() =>
                radicand == 1
                    ? $"{coefficient}"
                    : coefficient == 1
                        ? $"√{radicand}"
                        : $"{coefficient}√{radicand}";
        }

        public static Radical SimplifiedRoot(int n)
        {
            // Very easy cases
            if (n < 0) { throw  new Exception("imaginary");  }
            if (n < 2) { return new Radical(coefficient: n); }

            // Simple
            if (SqrtI(n) is int root) { return new Radical(coefficient: root); }

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

            return new Radical(coefficient: gpsFactor, radicand: gpsMultip);
        }

        public struct RadicalFraction
        {
            public RadicalFraction(Radical numerator = default, int denominator = 1)
            {
                this.numerator   = numerator;
                this.denominator = denominator;
            }

            public Radical numerator;
            public int denominator;

            public override string ToString() =>
                denominator == 1
                    ? $"{numerator}"
                    : $"({numerator})/{denominator}";
        }

        public static RadicalFraction SimplifiedRadicalFraction(int numerator, Radical denominator)
        {
            RadicalFraction result = new RadicalFraction
            {
                numerator   = denominator * numerator,
                denominator = denominator.Squared()
            };
            Fraction coefficient = SimplifiedFraction(result.numerator.coefficient, result.denominator);
            result.numerator.coefficient = coefficient.numerator;
            result.denominator = coefficient.denominator;
            return result;
        }
    }
}
