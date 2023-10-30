
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class AlgebraTests
{
    [TestMethod]
    public void HugeTest1()
    {
        Algebra.IAlgebraicNotation huge = new Algebra.Huge();
        Assert.IsInstanceOfType(huge, typeof(Algebra.Huge));
    }

    [TestMethod]
    public void HugeTest2()
    {
        Algebra.IAlgebraicNotation number = new Algebra.Number();
        Assert.IsNotInstanceOfType(number, typeof(Algebra.Huge));
    }

    [TestMethod]
    public void HugeTest3()
    {
        Algebra.IAlgebraicNotation number = Algebra.Power(2, 2);
        Assert.IsNotInstanceOfType(number, typeof(Algebra.Huge));
    }

    [TestMethod]
    public void HugeTest4()
    {
        Algebra.IAlgebraicNotation huge = Algebra.Power(int.MaxValue, 2);
        Assert.IsInstanceOfType(huge, typeof(Algebra.Huge));
    }
}
