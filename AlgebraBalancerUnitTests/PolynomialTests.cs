using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Algebra;
using System;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class PolynomialTests
{
    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        public void TestParseMonomial()
        {
            var expect = new Polynomial(
                new PolynomialTerm(3, new TermMultiplicand("x", 2))
            );
            var actual = Polynomial.Parse("3x^2");
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void TestParseBinomial()
        {
            var expect = new Polynomial(
                new PolynomialTerm(5, new TermMultiplicand("x", 2)),
                new PolynomialTerm(7, new TermMultiplicand("x"))
            );
            var actual = Polynomial.Parse("5x^2 + 7x");
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void TestParseTrinomial()
        {
            var expect = new Polynomial(
                new PolynomialTerm(2, new TermMultiplicand("x", 2)),
                new PolynomialTerm(9, new TermMultiplicand("x")),
                new PolynomialTerm(-8)
            );
            var actual = Polynomial.Parse("2x^2 + 9x - 8");
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void TestImpliedCoefficientOf1()
        {
            var expect = new Polynomial(
                new PolynomialTerm(new TermMultiplicand("x", 2))
            );
            var actual = Polynomial.Parse("x^2");
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void TestImpliedCoefficientOfNegative1()
        {
            var expect = new Polynomial(
                new PolynomialTerm(-1, new TermMultiplicand("x", 2))
            );
            var actual = Polynomial.Parse("-x^2");
            Assert.AreEqual(expect, actual);
        }
    }

    [TestClass]
    public class DivisionTests
    {
        private static readonly Polynomial numerA = Polynomial.Parse("3x^3 - 13x^2 + 28x - 12");
        private static readonly Polynomial denomA = Polynomial.Parse("3x - 1");
        private static readonly (Polynomial quotient, int remainder) expectA = (Polynomial.Parse("x^2 - 4x + 8"), -4);

        [TestMethod]
        public void TestLongDivisionA()
        {
            var (actualQuotient, actualRemainder) = Polynomial.LongDivision(numerA, denomA);
            Assert.AreEqual(expectA, (actualQuotient.SimplifiedToPolynomial(), actualRemainder));
        }

        [TestMethod]
        public void TestSyntheticDivisionA()
        {
            _ = Assert.ThrowsException<ArgumentException>(() => _ = Polynomial.SyntheticDivision(numerA, denomA));
        }

        private static readonly Polynomial numerB = Polynomial.Parse("8x^2 + 23");
        private static readonly Polynomial denomB = Polynomial.Parse("x + 2");
        private static readonly (Polynomial quotient, int remainder) expectB = (Polynomial.Parse("8x - 16"), 55);

        [TestMethod]
        public void TestLongDivisionB()
        {
            var (actualQuotient, actualRemainder) = Polynomial.LongDivision(numerB, denomB);
            Assert.AreEqual(expectB, (actualQuotient.SimplifiedToPolynomial(), actualRemainder));
        }

        [TestMethod]
        public void TestSyntheticDivisionB()
        {
            var (actualQuotient, actualRemainder) = Polynomial.SyntheticDivision(numerB, denomB);
            Assert.AreEqual(expectB, (actualQuotient.SimplifiedToPolynomial(), actualRemainder));
        }
    }
}
