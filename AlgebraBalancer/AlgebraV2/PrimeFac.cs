namespace AlgebraBalancer.AlgebraV2;

public struct PrimeExponent
{
    public ulong prime;
    public ulong exponent;
}

public struct Natural
{
    public PrimeExponent[] factors;
}

public struct Integer
{
    public bool isNegative;
    public PrimeExponent[] factors;
}

public class PositiveRational
{
    public Natural numerator;
    public Natural denominator;
}

public class Rational
{
    public bool isNegative;
    public Natural numerator;
    public Natural denominator;
}

public class Exponent
{
    public bool isNegative;
    public bool isImaginary;
    public Natural coefficient;
    //public Fraction number;
    //public Fraction power;
}

public class ExponentSum
{
    public Exponent[] terms;
}
