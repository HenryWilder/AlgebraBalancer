using System;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public struct RadicalFraction : IAlgebraicExpression
{
    public RadicalFraction() { }

    public RadicalFraction(Radical numerator, int denominator) =>
        (this.numerator, this.denominator) = (numerator, denominator);

    public RadicalFraction(int numerator, Radical denominator) =>
        (this.numerator, this.denominator) = (denominator * numerator, denominator.Squared());

    public RadicalFraction(int leadingTerm, Radical numerator, int denominator) =>
        (this.leadingTerm, this.numerator, this.denominator) = (leadingTerm, numerator, denominator);

    public int leadingTerm = 0;
    public Radical numerator = default;
    public int denominator = 1;

    public override readonly string ToString()
    {
        bool isQuadratic = leadingTerm != 0;
        bool isFractional = Math.Abs(denominator) != 1;

        string extendedNumerator = isQuadratic
            ? $"{leadingTerm}±{numerator}"
            : $"{numerator}";

        return isFractional
            ? $"({extendedNumerator})/{denominator}"
            : extendedNumerator;
    }

    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public readonly IAlgebraicNotation Simplified()
    {
        //tex:$$\frac{-b\pm\sqrt{b^2-4ac}}{2a} = \boxed{\frac{-b}{2a}}\pm\frac{\sqrt{b^2-4ac}}{2a}$$
        var simplifiedLeadingTerm = new Fraction(leadingTerm, denominator).Simplified();

        //tex:$$\frac{-b\pm\boxed{\sqrt{b^2-4ac}}}{2a}$$
        var simplifiedNumerator = numerator.Simplified();
        
        //tex:$$\sqrt{b^2-4ac} \in \mathbb{R}\setminus\mathbb{Q}$$
        if (simplifiedNumerator is Radical simplifiedRadicalNumerator)
        {
            //tex:$$\left.\frac{m}{2a} \;\middle|\; m\sqrt{\dots} = \sqrt{b^2-4ac}\right.$$
            var simplifiedRadicalCoefficient = new Fraction(simplifiedRadicalNumerator.coefficient, denominator).Simplified();

            //tex:$$\frac{m}{2a} \in \mathbb{Q}\setminus\mathbb{Z}$$
            if (simplifiedRadicalCoefficient is Fraction fracCoefficient)
            {
                var radNumerator = new Radical((Number)fracCoefficient.numerator, numerator.radicand);
                return new RadicalFraction(radNumerator, (Number)fracCoefficient.denominator);
            }
            //tex:$$\frac{m}{2a} \in \mathbb{Z}$$
            else if (simplifiedRadicalCoefficient is Number numCoefficient)
            {
                return new Radical(numCoefficient, numerator.radicand);
            }
            //tex:$$\frac{m}{2a} \notin \mathbb{Q}$$
            else
            {
                throw new NotImplementedException();
            }
        }
        //tex:$$\sqrt{b^2-4ac} \in \mathbb{Z}$$
        else if (simplifiedNumerator is Number simplifiedIntegerNumerator)
        {
            return new MultipleSolutions([
                simplifiedLeadingTerm.Add(simplifiedIntegerNumerator),
                simplifiedLeadingTerm.Sub(simplifiedIntegerNumerator),
            ]);
        }
        //tex:$$\sqrt{b^2-4ac} \in (\mathbb{C}\setminus\mathbb{R}) \cup ($$
        else
        {
            throw new NotImplementedException();
        }
    }
}
