using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Notation;
using AlgebraBalancer.Algebra;
using static AlgebraBalancer.ExactMath;
using AlgebraBalancer.Algebra.Balancer;

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

    [TestClass]
    public class RelationshipTests
    {
        private static void AssertRelationshipsAreEqual(Relationship expect, Relationship actual)
        {
            Assert.AreEqual(expect.items.Length, actual.items.Length);
            for (int i = 0; i < expect.items.Length; ++i)
            {
                var expectItem = expect.items[i];
                var actualItem = actual.items[i];
                if (i % 2 == 0)
                {
                    Assert.IsInstanceOfType(expectItem, typeof(RelSide), $"Malformed test: Cannot expect item {i} not to be RelSide");
                    Assert.IsInstanceOfType(actualItem, typeof(RelSide));
                    string expectSide = (string)(RelSide)expectItem;
                    string actualSide = (string)(RelSide)actualItem;
                    Assert.AreEqual(expectSide, actualSide);
                }
                else
                {
                    Assert.IsInstanceOfType(expectItem, typeof(RelComp), $"Malformed test: Cannot expect item {i} not to be RelComp");
                    Assert.IsInstanceOfType(actualItem, typeof(RelComp));
                    var expectCmp = (Comparator)(RelComp)expectItem;
                    var actualCmp = (Comparator)(RelComp)actualItem;
                    Assert.AreEqual(expectCmp, actualCmp);
                }
            }
        }

        [TestClass]
        public class ParseTests
        {
            [TestMethod]
            public void TestRelationshipParse()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.EQ, "10 - 2"),
                    Relationship.Parse("5 + 3 = 10 - 2"));
            }

            [TestMethod]
            public void TestRelationshipMultiCharCmpParse()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.LE, "10 - 2"),
                    Relationship.Parse("5 + 3 <= 10 - 2"));
            }

            [TestMethod]
            public void TestRelationshipParseAligned()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.LE, "10 - 2"),
                    Relationship.Parse("5 + 3 &<= 10 - 2"));
            }

            [TestMethod]
            public void TestRelationshipParseAlignedWithSpace()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.GE, "10 - 2"),
                    Relationship.Parse("5 + 3 & >= 10 - 2"));
            }

            [TestMethod]
            public void TestRelationshipParseMatAligned()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.LE, "10 - 2"),
                    Relationship.Parse("5 + 3 &<=& 10 - 2"));
            }

            [TestMethod]
            public void TestRelationshipParseMatAlignedWithSpace()
            {
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.GE, "10 - 2"),
                    Relationship.Parse("5 + 3 & >= & 10 - 2"));
            }
        }

        [TestClass]
        public class OperationTests
        {
            [TestMethod]
            public void TestOperationAdd()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "10 - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "10"), test);
            }

            [TestMethod]
            public void TestOperationAddMultipleSides()
            {
                var test = new Relationship("5 + 3 - 2", Comparator.EQ, "8 - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3", Comparator.EQ, "8"), test);
            }

            [TestMethod]
            public void TestOperationAddMultipleSameSide()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "12 - 2 - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "12 - 2"), test);
            }

            [TestMethod]
            public void TestOperationAddAmbiguous()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "10 - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "10"), test);
            }

            [TestMethod]
            public void TestOperationAddParentheticalRight()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "2(7 - 2) - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "2(7 - 2)"), test);
            }

            [TestMethod]
            public void TestOperationAddParentheticalLeft()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2 + 2(7 - 2)");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "2(7 - 2)"), test);
            }

            [TestMethod]
            public void TestOperationAddSameValueDifferentOperation()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2(7 - 2) - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "-2(7 - 2)"), test);
            }

            [TestMethod]
            public void TestOperationAddSameValueDifferentOperationLeft()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2 - 2(7 - 2)");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "-2(7 - 2)"), test);
            }
        }

        [TestClass]
        public class RefactorTests
        {

        }
    }
}
