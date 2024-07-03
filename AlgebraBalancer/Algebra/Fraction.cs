using System;

using AlgebraBalancer.Notation;
using static AlgebraBalancer.Notation.Bald;

namespace AlgebraBalancer.Algebra;
public struct Fraction : IAlgebraicExpression
{
    public Fraction() { }

    public Fraction(int numerator, int denominator) =>
        (this.numerator, this.denominator) = (new Number(numerator), new Number(denominator));

    public Fraction(IAlgebraicAtomic numerator, IAlgebraicAtomic denominator) =>
        (this.numerator, this.denominator) = (numerator, denominator);

    public IAlgebraicAtomic numerator = new Number(1);
    public IAlgebraicAtomic denominator = new Number(1);

    public override readonly string ToString() =>
        $"{numerator}/{denominator}";

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Simplified()
    {
        if (this.numerator is null || this.denominator is null)
        {
            throw new NullReferenceException("fractions should not contain null");
        }
        if (this.numerator is Undefined || this.denominator is Undefined)
        {
            return UNDEFINED;
        }
        if (this.numerator is not Number || this.denominator is not Number)
        {
            return this;
        }

        var numerator = (Number)this.numerator;
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

        int numeratorAbs = Math.Abs(numerator);
        int denominatorAbs = Math.Abs(denominator);

        int gcf = ExactMath.GCF(numeratorAbs, denominatorAbs);
        return new Fraction(sign * numeratorAbs / gcf, denominatorAbs / gcf);
    }
}
