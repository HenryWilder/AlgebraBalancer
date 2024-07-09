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
        var a = new Algebraic(1);
        var b = new Algebraic(2);
        var comb = a + b;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(3), test);
    }

    [TestMethod]
    public void Test2Mul2()
    {
        var a = new Algebraic(2);
        var comb = a * a;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(4), test);
    }

    [TestMethod]
    public void Test1Div2()
    {
        var a = new Algebraic(1);
        var b = new Algebraic(2);
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new Fraction(1, 2), test);
    }

    [TestMethod]
    public void Test4Div2()
    {
        var a = new Algebraic(4);
        var b = new Algebraic(2);
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new Number(2), test);
    }

    [TestMethod]
    public void TestSqrt2Over2()
    {
        var a = new Algebraic(new Radical(2));
        var b = new Algebraic(2);
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new RadicalFraction(new Radical(2), 2), test);
    }

    [TestMethod]
    public void Test2Sqrt2Over4()
    {
        var a = new Algebraic(new Radical(2, 2));
        var b = new Algebraic(4);
        var comb = a / b;
        var test = comb.Simplified();
        Assert.AreEqual(new RadicalFraction(new Radical(2), 2), test);
    }

    private static (IAlgebraicNotation, IAlgebraicNotation) QuadraticHelper(int a, int b, int c)
    {
        var lhs = new Algebraic(new Fraction(-b, 2 * a));
        var rhs = new Algebraic([new Radical(b * b - 4 * a * c)], 2 * a);
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
        Assert.AreEqual(new Algebraic([new Radical(-1, 1), new Radical(-14)], 3), solution1);
        Assert.AreEqual(new Algebraic([new Radical(-1, 1), new Radical(-1, -14)], 3), solution2);
    }
}
