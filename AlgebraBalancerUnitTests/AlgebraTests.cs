
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Notation;
using AlgebraBalancer.Algebra;
using static AlgebraBalancer.ExactMath;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class AlgebraTests
{
    [TestClass]
    public class PowerTests
    {
        [TestMethod]
        public void TestPowerSimple()
        {
            Assert.AreEqual(new Number(16), Power(4, 2));
        }

        [TestMethod]
        public void TestPowerOne()
        {
            Assert.AreEqual(new Number(4), Power(4, 1));
        }

        [TestMethod]
        public void TestPowerZero()
        {
            Assert.AreEqual(new Number(1), Power(4, 0));
        }

        [TestMethod]
        public void TestPowerZeroZero()
        {
            Assert.AreEqual(new Number(1), Power(0, 0));
        }

        [TestMethod]
        public void TestPowerNegative()
        {
            Assert.AreEqual(new Fraction(1, 16), Power(4, -2));
        }

        [TestMethod]
        public void TestPowerHuge()
        {
            Assert.IsInstanceOfType(Power(int.MaxValue, 2), typeof(Huge));
        }

        [TestMethod]
        public void TestPowerTiny()
        {
            Assert.IsInstanceOfType(Power(int.MaxValue, -2), typeof(Tiny));
        }
    }

    [TestClass]
    public class RadicalTests
    {
        [TestMethod]
        public void TestRadicalSimplifyToInt()
        {
            Assert.AreEqual(new Number(1), new Radical(1).Simplified());
        }

        [TestMethod]
        public void TestRadicalSimplifyToRadical()
        {
            Assert.AreEqual(new Radical(2), new Radical(2).Simplified());
        }

        [TestMethod]
        public void TestRadicalSimplifyToRadicalWithCoefficient()
        {
            Assert.AreEqual(new Radical(2, 2), new Radical(8).Simplified());
        }

        [TestMethod]
        public void TestRadicalImaginary()
        {
            Assert.AreEqual(new Imaginary(2), new Radical(-4).Simplified());
        }
    }

    [TestClass]
    public class FractionTests
    {

        [TestMethod]
        public void TestFractionSimplest()
        {
            Assert.AreEqual(new Fraction(2, 3), new Fraction(2, 3).Simplified());
        }

        [TestMethod]
        public void TestFractionSimplifyToNumber()
        {
            Assert.AreEqual(new Number(1), new Fraction(2, 2).Simplified());
        }

        [TestMethod]
        public void TestFractionDivByZero()
        {
            Assert.IsInstanceOfType(new Fraction(5, 0).Simplified(), typeof(Undefined));
        }

        [TestMethod]
        public void TestFractionSimplifyToFraction()
        {
            Assert.AreEqual(new Fraction(1, 3), new Fraction(3, 9).Simplified());
        }
    }

    [TestClass]
    public class ImaginaryTests
    {
        [TestMethod]
        public void TestImaginaryTimesImaginary()
        {
            Assert.AreEqual(new Number(-6), new Imaginary(2) * new Imaginary(3));
        }

        [TestMethod]
        public void TestImaginaryTimesNumber()
        {
            Assert.AreEqual(new Imaginary(6), new Imaginary(2) * new Number(3));
        }

        [TestMethod]
        public void TestImaginaryPlusImaginary()
        {
            Assert.AreEqual(new Imaginary(5), new Imaginary(2) + new Imaginary(3));
        }

        [TestMethod]
        public void TestImaginaryPlusNumber()
        {
            Assert.AreEqual(new Complex(2, 3), new Number(2) + new Imaginary(3));
        }
    }

    [TestClass]
    public class ComplexTests
    {
        [TestMethod]
        public void TestComplexTimesComplex()
        {
            Assert.AreEqual(new Complex(-7, 22), new Complex(2, 3) * new Complex(4, 5));
        }

        [TestMethod]
        public void TestComplexTimesNumber()
        {
            Assert.AreEqual(new Complex(6, 9), new Complex(2, 3) * new Number(3));
        }

        [TestMethod]
        public void TestComplexTimesImaginary()
        {
            Assert.AreEqual(new Complex(-12, 8), new Complex(2, 3) * new Imaginary(4));
        }

        [TestMethod]
        public void TestComplexPlusComplex()
        {
            Assert.AreEqual(new Complex(5, 7), new Complex(2, 3) + new Complex(3, 4));
        }

        [TestMethod]
        public void TestComplexPlusNumber()
        {
            Assert.AreEqual(new Complex(5, 3), new Complex(2, 3) + new Number(3));
        }

        [TestMethod]
        public void TestComplexPlusImaginary()
        {
            Assert.AreEqual(new Complex(2, 5), new Complex(2, 3) + new Imaginary(2));
        }
    }
}
