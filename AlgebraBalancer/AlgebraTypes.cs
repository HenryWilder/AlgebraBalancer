using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AlgebraBalancer.AlgebraTypes;

namespace AlgebraBalancer;

internal static class AlgebraTypes
{
    // Eq to Number
    private static bool CheckEquivalent(Number num1, Number num2)
    {
        if (num1.tag != num2.tag)
        {
            return false;
        }

        return num1.tag switch
        {
            Number.Tag.Rounded or Number.Tag.Huge or Number.Tag.Epsilon or Number.Tag.Imaginary =>
                false, // todo: insufficient certainty

            Number.Tag.Integral  => num1.value      == num2.value,
            Number.Tag.Infinite  => num1.IsPositive == num2.IsPositive,
            Number.Tag.Undefined => true,
            _ => throw new NotImplementedException(),
        };
    }

    private static bool CheckEquivalent(Fraction frac, Number num) =>
        frac.IsIntegral && CheckEquivalent(frac.Evaluated, num);

    private static bool CheckEquivalent(Radical rad, Number num) =>
        rad.IsIntegral && CheckEquivalent(rad.Evaluated, num);

    private static bool CheckEquivalent(Exponent exp, Number num) =>
        exp.IsIntegral && CheckEquivalent(exp.Evaluated, num);

    // Eq to Fraction
    private static bool CheckEquivalent(Fraction frac1, Fraction frac2) =>
        CheckEquivalent(frac1.numerator,   frac2.numerator  ) &&
        CheckEquivalent(frac1.denominator, frac2.denominator);

    private static bool CheckEquivalent(Radical rad, Fraction frac) =>
        CheckEquivalent(frac.denominator, Number.One) &&
        CheckEquivalent(rad, frac.numerator);

    private static bool CheckEquivalent(Exponent exp, Fraction frac) =>
        CheckEquivalent(frac.numerator, Number.One) &&
        CheckEquivalent(new Exponent(exp.basePart, -exp.exponent).Evaluated, frac.denominator);


    // Eq to Radical
    private static bool CheckEquivalent(Radical rad1, Radical rad2) =>
        CheckEquivalent(rad1.coefficient, rad2.coefficient) &&
        CheckEquivalent(rad1.radicand,    rad2.radicand   );

    private static bool CheckEquivalent(Exponent exp, Radical rad) =>
        throw new NotImplementedException(); // todo


    // Eq to Exponent
    private static bool CheckEquivalent(Exponent exp1, Exponent exp2) =>
        CheckEquivalent(exp1.Evaluated, exp2.Evaluated);


    // Selector
    private static bool CheckEquivalent(IAlgebraExpression expr1, IAlgebraExpression expr2)
    {
        expr1 = expr1.Simplified;
        expr2 = expr2.Simplified;
        if (expr1 is Number num1)
        {
            if (expr2 is Number    num2) { return CheckEquivalent( num1, num2); }
            if (expr2 is Fraction frac2) { return CheckEquivalent(frac2, num1); }
            if (expr2 is Radical   rad2) { return CheckEquivalent( rad2, num1); }
            if (expr2 is Exponent  exp2) { return CheckEquivalent( exp2, num1); }
        }
        if (expr1 is Fraction frac1)
        {
            if (expr2 is Number    num2) { return CheckEquivalent(frac1,  num2); }
            if (expr2 is Fraction frac2) { return CheckEquivalent(frac1, frac2); }
            if (expr2 is Radical   rad2) { return CheckEquivalent( rad2, frac1); }
            if (expr2 is Exponent  exp2) { return CheckEquivalent( exp2, frac1); }
        }
        if (expr1 is Radical rad1)
        {
            if (expr2 is Number    num2) { return CheckEquivalent(rad1,  num2); }
            if (expr2 is Fraction frac2) { return CheckEquivalent(rad1, frac2); }
            if (expr2 is Radical   rad2) { return CheckEquivalent(rad1,  rad2); }
            if (expr2 is Exponent  exp2) { return CheckEquivalent(exp2,  rad1); }
        }
        if (expr1 is Exponent exp1)
        {
            if (expr2 is Number    num2) { return CheckEquivalent(exp1,  num2); }
            if (expr2 is Fraction frac2) { return CheckEquivalent(exp1, frac2); }
            if (expr2 is Radical   rad2) { return CheckEquivalent(exp1,  rad2); }
            if (expr2 is Exponent  exp2) { return CheckEquivalent(exp1,  exp2); }
        }

        throw new NotImplementedException();
    }


    public interface IAlgebraExpression
    {
        public abstract bool IsIntegral { get; }               // The expression can be simplified to an integer without losing precision
        public abstract IAlgebraExpression Simplified { get; } // Get the simplest form of the expression
        public abstract Number Evaluated { get; }              // Convert to Number, even if irrational
        public abstract IAlgebraExpression Negated { get; }    // Multiply by -1
    }

    public struct Number : IAlgebraExpression
    {
        public enum Tag
        {
            Integral,  // Value is exact
            Rounded,   // Approximation of a fraction
            Huge,      // Immeasurably large, but finite; uses value's sign
            Epsilon,   // Immeasurably small, but finite; uses value's sign
            Imaginary, // Square root of negative
            Infinite,  // Divergently recursive; uses value's sign
            Undefined, // Division by zero
        }

        public static Number Zero => new(0);
        public static Number One  => new(1);
        public static Number Huge => new() { value = int.MaxValue, tag = Tag.Huge };
        public static Number Epsilon => new() { value = 0, tag = Tag.Epsilon };
        public static Number Infinity => new() { value = int.MaxValue, tag = Tag.Infinite };
        public static Number operator -(Number num) => new() { value = -num.value, tag = num.tag };

        public Number(int value)
        {
            this.value = value;
            tag = Tag.Integral;
        }
        public Number(double value)
        {
            bool isMeasurable = value is >= int.MinValue and <= int.MaxValue;

            if (isMeasurable)
            {
                bool isIntegral = value % 1 == 0;

                if (isIntegral)
                {
                    this.value = (int)Math.Floor(value);
                    tag = Tag.Integral;
                }
                else // Fractional
                {
                    bool isPositive = value >= 0;
                    this.value = isPositive ? 1 : -1;

                    bool isTiny = value is > -0.0001 and < 0.0001 and not 0;
                    tag = isTiny ? Tag.Epsilon : Tag.Rounded;
                }
            }
            else // Immeasurable
            {
                this.value = value > 0 ? int.MaxValue : int.MinValue;
                bool isFinite = !(double.IsInfinity(value) || double.IsNegativeInfinity(value));
                tag = isFinite ? Tag.Huge : Tag.Infinite;
            }
        }

        public int value;
        public Tag tag;

        public readonly bool IsReal       => tag is not (Tag.Imaginary or Tag.Undefined);
        public readonly bool IsFinite     => tag != Tag.Infinite;
        public readonly bool IsMeasurable => tag is Tag.Integral or Tag.Rounded;
        public readonly bool IsExact      => tag == Tag.Integral;
        public readonly bool IsPositive   => IsReal && value >= 0;
        public readonly bool IsNegative   => IsReal && value < 0;
        private readonly string Sign      => IsNegative ? "-" : string.Empty;

        public override readonly string ToString() =>
            tag switch
            {
                Tag.Integral  => value.ToString(),
                Tag.Rounded   => $"{{{value}<•<{value+1}}}",
                Tag.Huge      => Sign + "𝓗",
                Tag.Epsilon   => Sign + "ε",
                Tag.Imaginary => "{?𝑖}",
                Tag.Infinite  => Sign + "∞",
                Tag.Undefined => "∅",
                _ => throw new NotImplementedException()
            };

        public readonly bool IsIntegral => IsExact;

        public readonly IAlgebraExpression Simplified => this;
        public readonly Number Evaluated => this;
        public readonly IAlgebraExpression Negated => new Number() { value = -value, tag = tag };
    }

    public struct Fraction : IAlgebraExpression
    {
        public IAlgebraExpression numerator;
        public IAlgebraExpression denominator;

        public override readonly string ToString() =>
            IsIntegral ? $"{numerator}" : $"{numerator}/{denominator}";

        public readonly bool IsIntegral => CheckEquivalent(denominator, Number.One);

        public readonly IAlgebraExpression Simplified => throw new NotImplementedException(); // todo
        public readonly Number Evaluated => throw new NotImplementedException(); // todo

        public readonly bool IsEquivalentTo(IAlgebraExpression expression)
        {
            if (expression is Number num)
            {
                return IsIntegral && num.IsEquivalentTo(numerator);
            }
            if (expression is Fraction frac)
            {
                return numerator.IsEquivalentTo(frac.numerator) &&
                    denominator.IsEquivalentTo(frac.denominator);
            }
            if (expression is Radical rad)
            {
                return rad.IsEquivalentTo(this);
            }
            if (expression is Exponent exp)
            {
                return exp.IsEquivalentTo(this);
            }

            throw new NotImplementedException();
        }
    }


    public struct Radical : IAlgebraExpression
    {
        public IAlgebraExpression coefficient;
        public IAlgebraExpression radicand;

        public static Radical operator *(Radical radical, IAlgebraExpression mult) =>
            new() { coefficient = radical.coefficient * mult, radicand = radical.radicand };

        public readonly IAlgebraExpression Squared() =>
            coefficient * coefficient + radicand;

        public readonly bool IsImaginary() =>
            radicand < 0;

        public override readonly string ToString()
        {
            if (IsImaginary())
            {
                return "𝑖";
            }
            if (radicand == 1)
            {
                return $"{coefficient}";
            }
            else if (coefficient == 1)
            {
                return $"√{radicand}";
            }
            else
            {
                return $"{coefficient}√{radicand}";
            }
        }

        public readonly bool IsIntegral =>
            radicand.IsEquivalentTo(Number.One);

        public readonly IAlgebraExpression Simplified => throw new NotImplementedException(); // todo
        public readonly Number Evaluated => throw new NotImplementedException(); // todo

        public readonly bool IsEquivalentTo(IAlgebraExpression expression)
        {
            if (expression is Number num)
            {
                return IsIntegral && num.IsEquivalentTo(numerator);
            }
            if (expression is Fraction frac)
            {
                return numerator.IsEquivalentTo(frac.numerator) &&
                    denominator.IsEquivalentTo(frac.denominator);
            }
            if (expression is Radical rad)
            {
                return rad.IsEquivalentTo(this);
            }
            if (expression is Exponent exp)
            {
                return exp.IsEquivalentTo(this);
            }

            throw new NotImplementedException();
        }
    }


    public struct Exponent : IAlgebraExpression
    {
        public IAlgebraExpression basePart;
        public IAlgebraExpression exponent;

        // Converts exponent to number
        public readonly Number? Expanded => ;

        // Assumes simplified
        public readonly bool IsIntegral
        {
            get
            {
                if (exponent is Number num)
                {
                    return num.IsExact && num.IsPositive;
                }
                return false;
            }
        }

        public readonly IAlgebraExpression Simplified => throw new NotImplementedException(); // todo
        public readonly Number Evaluated => throw new NotImplementedException(); // todo

    }
}
