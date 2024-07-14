using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Algebra;

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
    public class LongDivisionTests
    {
        [TestMethod]
        public void Test1()
        {
            var numer = Polynomial.Parse("3x^3 - 13x^2 + 28x - 12");
            var denom = Polynomial.Parse("3x - 1");
            var (actualQuotient, actualRemainder) = numer / denom;
            actualQuotient = actualQuotient.SimplifiedToPolynomial();
            var (expectQuotient, expectRemainder) = (Polynomial.Parse("x^2 - 4x + 8"), -4);
            Assert.AreEqual(expectQuotient, actualQuotient);
            Assert.AreEqual(expectRemainder, actualRemainder);
        }

        [TestMethod]
        public void Test2()
        {
            var numer = Polynomial.Parse("8x^2 + 23");
            var denom = Polynomial.Parse("x + 2");
            var (actualQuotient, actualRemainder) = numer / denom;
            actualQuotient = actualQuotient.SimplifiedToPolynomial();
            var (expectQuotient, expectRemainder) = (Polynomial.Parse("8x - 16"), 55);
            Assert.AreEqual(expectQuotient, actualQuotient);
            Assert.AreEqual(expectRemainder, actualRemainder);
        }
    }
}
