using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Notation;
using AlgebraBalancer.Algebra;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class AlgebraicTests
{
    [TestMethod]
    public void Test1Add2()
    {
        Algebraic a = 1;
        Algebraic b = 2;
        var comb = a + b;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(3), test);
    }

    [TestMethod]
    public void Test2Mul2()
    {
        Algebraic a = 2;
        var comb = a * a;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(4), test);
    }

    [TestMethod]
    public void Test1Div2()
    {
        Algebraic a = 1;
        Algebraic b = 2;
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new Fraction(1, 2), test);
    }

    [TestMethod]
    public void Test4Div2()
    {
        Algebraic a = 4;
        Algebraic b = 2;
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(2), test);
    }

    [TestMethod]
    public void TestSqrt2Over2()
    {
        Algebraic a = new Radical(2);
        Algebraic b = 2;
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new RadicalFraction(new Radical(2), 2), test);
    }

    [TestMethod]
    public void Test2Sqrt2Over4()
    {
        Algebraic a = new Radical(2, 2);
        Algebraic b = 4;
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new RadicalFraction(new Radical(2), 2), test);
    }

    private static (IAlgebraicNotation, IAlgebraicNotation) QuadraticHelper(int a, int b, int c)
    {
        Algebraic lhs = new Fraction(-b, 2 * a);
        Algebraic rhs = new Radical(b * b - 4 * a * c) / (2 * a);
        var solution1 = (lhs + rhs).Simplified();
        var solution2 = (lhs - rhs).Simplified();
        return (solution1, solution2);
    }

    [TestMethod]
    public void TestQuadratic1()
    {
        var (solution1, solution2) = QuadraticHelper(2, -4, -6);
        Assert.AreEqual(new Number(3), solution1);
        Assert.AreEqual(new Number(-1), solution2);
    }

    [TestMethod]
    public void TestQuadratic2()
    {
        var (solution1, solution2) = QuadraticHelper(3, 2, 5);

        var expect1 = (new Radical(-1, 1) + new Radical(-14)) / 3;
        Assert.AreEqual(expect1, solution1);

        var expect2 = (new Radical(-1, 1) + new Radical(-1, -14)) / 3;
        Assert.AreEqual(expect2, solution2);
    }

    [TestMethod]
    public void TestDenominatorRationalization()
    {
        Assert.AreEqual(
            (new Radical(2, 3) + new Radical(3, 2) + new Radical(30)) / 12,
            (1 / (new Radical(2) + new Radical(3) - new Radical(5))).Simplified()
        );
    }
}
