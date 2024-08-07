﻿using System;
using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public class RadicalFraction : IAlgebraicExpression
{
    public RadicalFraction() { }

    public RadicalFraction(Radical numerator, int denominator) =>
        (this.numerator, this.denominator) = (numerator, denominator);

    public RadicalFraction(int numerator, Radical denominator) =>
        (this.numerator, this.denominator) = (new Radical(denominator.coefficient * numerator, denominator.radicand), denominator.Squared());

    public Radical numerator = default;
    public int denominator = 1;

    public bool IsInoperable => false;

    // Does not simplify
    public override string ToString()
    {
        return $"({numerator})/{denominator}";
    }

    public string AsEquality(string lhs) => $"{lhs}={ToString()}";

    public IAlgebraicNotation Simplified()
    {
        //tex:$$d = 0 \implies \frac{n}{0} \therefore \nexists$$
        if (denominator == 0)
        {
            return Bald.UNDEFINED;
        }
        //tex:$$|d|=1 \implies \frac{n}{1} \therefore n \text{ or } \frac{n}{-1} = \frac{-n}{1} \therefore -n$$
        else if (denominator is 1 or -1)
        {
            // Regular radical
            return new Radical(denominator * numerator.coefficient, numerator.radicand).Simplified();
        }

        //tex:$$\frac{c\sqrt{r}}{d} = \frac{n}{d}$$
        var simplifiedNumerator = numerator.Simplified();

        //tex:$$n \in \mathbb{Z}$$
        if (simplifiedNumerator is Number num)
        {
            // Regular fraction
            return new Fraction(num, denominator).Simplified();
        }
        //tex:$$ni \mid n \in \mathbb{Z}$$
        else if (simplifiedNumerator is Imaginary imag)
        {
            //tex:$$\frac{ni}{d} = mi$$
            var newCoef = new Fraction(imag.coef, denominator).Simplified();

            //tex:$$m \in \mathbb{Z}$$
            if (newCoef is Number whole)
            {
                return new Imaginary(whole);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        //tex:$$n \notin \mathbb{Q}$$
        else if (simplifiedNumerator is Radical rad)
        {
            //tex:$$\frac{c\sqrt{r}}{d} = \frac{c}{d}\sqrt{r} = m\sqrt{r}$$
            var newCoef = new Fraction(rad.coefficient, denominator).Simplified();

            //tex:$$m \in \mathbb{Z} \therefore m\sqrt{r}$$
            if (newCoef is Number newCoefNum)
            {
                return new Radical(newCoefNum, rad.radicand).Simplified();
            }
            //tex:$$m \in \mathbb{Q}\setminus\mathbb{Z} \therefore \frac{c'\sqrt{r}}{d'}$$
            else if (newCoef is Fraction newCoefFrac)
            {
                return new RadicalFraction(
                    new Radical(newCoefFrac.numerator, rad.radicand),
                    newCoefFrac.denominator);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public static RadicalFraction operator -(RadicalFraction rhs) => new(-rhs.numerator, rhs.denominator);
}
