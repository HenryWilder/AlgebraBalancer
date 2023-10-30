
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static AlgebraBalancer.Algebra;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class AlgebraTests
{
    [TestMethod]
    public void PowerTest1() =>
        Assert.IsInstanceOfType(Power(2, 2), typeof(Number));

    [TestMethod]
    public void PowerTest2() =>
        Assert.IsInstanceOfType(Power(int.MaxValue, 2), typeof(Huge));

    [TestMethod]
    public void PowerTest3() =>
        Assert.IsInstanceOfType(Power(int.MaxValue, -2), typeof(Epsilon));

    [TestMethod]
    public void RadicalTest1() =>
        Assert.IsInstanceOfType(new Radical(1).Simplified(), typeof(Number));

    [TestMethod]
    public void RadicalTest2() =>
        Assert.IsInstanceOfType(new Radical(2).Simplified(), typeof(Radical));

    [TestMethod]
    public void RadicalTest3() =>
        Assert.IsInstanceOfType(new Radical(-1).Simplified(), typeof(Complex));

    [TestMethod]
    public void FractionTest1() =>
        Assert.IsInstanceOfType(new Fraction(1, 2).Simplified(), typeof(Fraction));

    [TestMethod]
    public void FractionTest2() =>
        Assert.IsInstanceOfType(new Fraction(2, 2).Simplified(), typeof(Number));

    [TestMethod]
    public void FractionTest3() =>
        Assert.IsInstanceOfType(new Fraction(1, 0).Simplified(), typeof(Undefined));
}
