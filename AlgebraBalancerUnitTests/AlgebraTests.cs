﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Notation;
using AlgebraBalancer.Algebra;
using static AlgebraBalancer.ExactMath;
using static AlgebraBalancer.MainPage;
using AlgebraBalancer.Algebra.Balancer;
using AlgebraBalancer;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using static AlgebraBalancerUnitTests.TestHelper;

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

        [TestClass]
        public class ToStringTests
        {
            [TestMethod]
            public void TestRadicalWithCoefficientToString()
            {
                Assert.AreEqual("2√2", new Radical(2, 2).ToString());
            }

            [TestMethod]
            public void TestRadicalNoCoefficientToString()
            {
                Assert.AreEqual("√2", new Radical(2).ToString());
            }

            [TestMethod]
            public void TestRadicalNoRadicandToString()
            {
                Assert.AreEqual("2", new Radical(2, 1).ToString());
            }

            [TestMethod]
            public void TestRadicalNoCoefficientOrRadicandToString()
            {
                Assert.AreEqual("1", new Radical().ToString());
            }

            [TestMethod]
            public void TestRadicalImaginaryToString()
            {
                Assert.AreEqual("2𝑖√2", new Radical(2, -2).ToString());
            }

            [TestMethod]
            public void TestRadicalImaginaryNoRadicandToString()
            {
                Assert.AreEqual("2𝑖", new Radical(2, -1).ToString());
            }

            [TestMethod]
            public void TestRadicalImaginaryNoCoefficientToString()
            {
                Assert.AreEqual("𝑖√2", new Radical(-2).ToString());
            }

            [TestMethod]
            public void TestRadicalImaginaryNoCoefficientOrRadicandToString()
            {
                Assert.AreEqual("𝑖", new Radical(-1).ToString());
            }
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
    public class RadicalFractionTests
    {
        [TestMethod]
        public void TestBasic()
        {
            Assert.AreEqual(new Number(1), new RadicalFraction(new Radical(1, 1), 1).Simplified());
        }

        [TestClass]
        public class ToStringTests
        {

        }
    }

    [TestClass]
    public class QuadraticTests
    {
        [TestMethod]
        public void Test1()
        {
            var solutions = Quadratic(1, 5, 6).solutions;
            Assert.AreEqual(new Number(-2), solutions[0]);
            Assert.AreEqual(new Number(-3), solutions[1]);
        }

        [TestMethod]
        public void Test2()
        {
            var solutions = Quadratic(1, 3, -10).solutions;
            Assert.AreEqual(new Number(2), solutions[0]);
            Assert.AreEqual(new Number(-5), solutions[1]);
        }

        [TestMethod]
        public void Test3()
        {
            var solutions = Quadratic(9, -6, 1).solutions;
            Assert.AreEqual(new Fraction(1, 3), solutions[0]);
            Assert.AreEqual(new Fraction(1, 3), solutions[1]);
        }
    }

    [TestClass]
    public class VertexFormTests
    {
        [TestMethod]
        public void Test1()
        {

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

        //[TestClass]
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
            public void TestOperationAddVar()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "10 - x");
                test.ApplyOperation(Operation.Add, "x");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + x", Comparator.EQ, "10"), test);
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
            public void TestOperationAddParentheticalRightContinued()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "2(7 - 2 + 3) - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "2(7 - 2 + 3)"), test);
            }

            [TestMethod]
            public void TestOperationAddParentheticalLeftContinued()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2 + 2(7 - 2 + 3)");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "2(7 - 2 + 3)"), test);
            }

            [TestMethod]
            public void TestOperationAddSameValueDifferentOperationLeft()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2 - 2(7 - 2)");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "-2(7 - 2)"), test);
            }

            [TestMethod]
            public void TestOperationAddSameValueDifferentOperationRight()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "-2(7 - 2) - 2");
                test.ApplyOperation(Operation.Add, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 + 2", Comparator.EQ, "-2(7 - 2)"), test);
            }

            [TestMethod]
            public void TestOperationSub()
            {
                var test = new Relationship("5 + 3", Comparator.EQ, "8 + 2");
                test.ApplyOperation(Operation.Sub, "2");
                AssertRelationshipsAreEqual(
                    new Relationship("5 + 3 - 2", Comparator.EQ, "8"), test);
            }
        }

        //[TestClass]
        public class FactorTests
        {
            [TestMethod]
            public void TestFactorVariable()
            {
                Assert.AreEqual("(a + b)x", Relationship.Factor("ax + bx"));
            }

            [TestMethod]
            public void TestFactorVariableMoreTerms()
            {
                Assert.AreEqual("(a + b + c + d)x", Relationship.Factor("ax + bx + cx + dx"));
            }

            [TestMethod]
            public void TestFactorVariablePower()
            {
                Assert.AreEqual("(x + 1)x", Relationship.Factor("x^2 + x"));
            }

            [TestMethod]
            public void TestFactorVariablePower2()
            {
                Assert.AreEqual("(x + 1)x^2", Relationship.Factor("x^3 + x^2"));
            }

            [TestMethod]
            public void TestFactorConstant()
            {
                Assert.AreEqual("(2a + b)2", Relationship.Factor("4a + 2b"));
            }
        }
    }
}

[TestClass]
public class MainPageTests
{
    [TestClass]
    public class GetLineContainingPositionTests
    {
        [TestMethod]
        public void TestBetweenNewlines()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"{SEL_BEG_EX}oran{SEL_BEG_IN}ge{SEL_END_EX}\r" +
                $"banana\r" +
                $"mango");
        }

        [TestMethod]
        public void TestBetweenNewlinesButNotFirst()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"orange\r" +
                $"{SEL_BEG_EX}ban{SEL_BEG_IN}ana{SEL_END_EX}\r" +
                $"mango");
        }

        [TestMethod]
        public void TestFirstLine()
        {
            AssertPredictedLineRange(
                $"{SEL_BEG_EX}ap{SEL_BEG_IN}ple{SEL_END_EX}\r" +
                $"orange\r" +
                $"banana\r" +
                $"mango");
        }

        [TestMethod]
        public void TestLastLine()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"orange\r" +
                $"banana\r" +
                $"{SEL_BEG_EX}man{SEL_BEG_IN}go{SEL_END_EX}");
        }

        [TestMethod]
        public void TestStart()
        {
            AssertPredictedLineRange(
                $"{SEL_BEG_EX}{SEL_BEG_IN}apple{SEL_END_EX}\r" +
                $"orange\r" +
                $"banana\r" +
                $"mango");
        }

        [TestMethod]
        public void TestEnd()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"orange\r" +
                $"banana\r" +
                $"{SEL_BEG_EX}mango{SEL_BEG_IN}{SEL_END_EX}");
        }

        [TestMethod]
        public void TestEmptyString()
        {
            AssertPredictedLineRange(
                $"{SEL_BEG_EX}{SEL_BEG_IN}{SEL_END_EX}");
        }

        [TestMethod]
        public void TestSingleLineString()
        {
            AssertPredictedLineRange(
                $"{SEL_BEG_EX}app{SEL_BEG_IN}le{SEL_END_EX}");
        }

        [TestMethod]
        public void TestEmptyLine()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"{SEL_BEG_EX}{SEL_BEG_IN}{SEL_END_EX}\r" +
                $"banana");
        }

        [TestMethod]
        public void TestStartOfLine()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"{SEL_BEG_EX}{SEL_BEG_IN}orange{SEL_END_EX}\r" +
                $"banana");
        }

        [TestMethod]
        public void TestEndOfLine()
        {
            AssertPredictedLineRange(
                $"apple\r" +
                $"{SEL_BEG_EX}orange{SEL_BEG_IN}{SEL_END_EX}\r" +
                $"banana");
        }
    }

    [TestClass]
    public class ColumnJumpTests
    {
        private const bool JUMP_LEFT = true;
        private const bool JUMP_RIGHT = false;

        [TestMethod]
        public void TestJumpRightToAmp()
        {
            string notesText = $"app{SEL_BEG_IN}le {SEL_BEG_EX}& orange";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToAmp()
        {
            string notesText = $"apple {SEL_BEG_EX}& ora{SEL_BEG_IN}nge";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToAmpFromStart()
        {
            string notesText = $"{SEL_BEG_IN}apple {SEL_BEG_EX}& orange";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToAmpFromEnd()
        {
            string notesText = $"apple {SEL_BEG_EX}& orange{SEL_BEG_IN}";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToEnd()
        {
            string notesText = $"apple & ora{SEL_BEG_IN}nge{SEL_BEG_EX}";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToStart()
        {
            string notesText = $"{SEL_BEG_EX}app{SEL_BEG_IN}le & orange";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToEndFromAmp()
        {
            string notesText = $"apple {SEL_BEG_IN}& orange{SEL_BEG_EX}";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToStartFromAmp()
        {
            string notesText = $"{SEL_BEG_EX}apple {SEL_BEG_IN}& orange";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToNewline()
        {
            string notesText = $"apple & or{SEL_BEG_IN}ange{SEL_BEG_EX}\rbanana & mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToNewline()
        {
            string notesText = $"apple & orange\r{SEL_BEG_EX}banan{SEL_BEG_IN}a & mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToNextlineFromBeforeNewline()
        {
            string notesText = $"apple & orange{SEL_BEG_IN}\r{SEL_BEG_EX}banana & mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToAmpFromBeforeNewline()
        {
            string notesText = $"apple {SEL_BEG_EX}& orange{SEL_BEG_IN}\rbanana & mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpRightToAmpFromAfterNewline()
        {
            string notesText = $"apple & orange\r{SEL_BEG_IN}banana {SEL_BEG_EX}& mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_RIGHT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }

        [TestMethod]
        public void TestJumpLeftToPrevlineFromAfterNewline()
        {
            string notesText = $"apple & orange{SEL_BEG_EX}\r{SEL_BEG_IN}banana & mango";
            GetTextSelectionPositions(ref notesText, out int initStart, out int expectStart, out _, out _);
            ColumnJump(JUMP_LEFT, notesText, initStart, out int newSelectionStart, out int _);
            Assert.AreEqual(expectStart, newSelectionStart);
        }
    }

    [TestClass]
    public class DuplicateLineTests
    {
        private const bool UPWARD = true;
        private const bool DOWNWARD = false;

        [TestMethod]
        public void TestDupeLineDown()
        {
            string notesText       = $"apple orange\rbanana mango{SEL_BEG_IN}\rstrawberry kiwi";
            string expectNotesText = $"apple orange\rbanana mango\rbanana mango{SEL_BEG_EX}\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(DOWNWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineUp()
        {
            string notesText       = $"apple orange\rbanana mango{SEL_BEG_IN}\rstrawberry kiwi";
            string expectNotesText = $"apple orange\rbanana mango{SEL_BEG_EX}\rbanana mango\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(UPWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineDownFromWithinLine()
        {
            string notesText = $"apple orange\rbanana m{SEL_BEG_IN}ango\rstrawberry kiwi";
            string expectNotesText = $"apple orange\rbanana mango\rbanana m{SEL_BEG_EX}ango\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(DOWNWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineUpFromWithinLine()
        {
            string notesText = $"apple orange\rbanana m{SEL_BEG_IN}ango\rstrawberry kiwi";
            string expectNotesText = $"apple orange\rbanana m{SEL_BEG_EX}ango\rbanana mango\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(UPWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineDownFromStartLine()
        {
            string notesText = $"apple orange{SEL_BEG_IN}\rbanana mango\rstrawberry kiwi";
            string expectNotesText = $"apple orange\rapple orange{SEL_BEG_EX}\rbanana mango\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(DOWNWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineUpFromStartLine()
        {
            string notesText = $"apple orange{SEL_BEG_IN}\rbanana mango\rstrawberry kiwi";
            string expectNotesText = $"apple orange{SEL_BEG_EX}\rapple orange\rbanana mango\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(UPWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineDownFromEndLine()
        {
            string notesText = $"apple orange\rbanana mango\rstrawberry kiwi{SEL_BEG_IN}";
            string expectNotesText = $"apple orange\rbanana mango\rstrawberry kiwi\rstrawberry kiwi{SEL_BEG_EX}";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(DOWNWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestDupeLineUpFromEndLine()
        {
            string notesText = $"apple orange\rbanana mango\rstrawberry kiwi{SEL_BEG_IN}";
            string expectNotesText = $"apple orange\rbanana mango\rstrawberry kiwi{SEL_BEG_EX}\rstrawberry kiwi";
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            DuplicateLine(UPWARD, selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        // Empty notes are handled by a check outside of the DuplicateLine function. Duplicating a line when the notes are empty is undefined.
    }

    [TestClass]
    public class CalculateInlineTests
    {
        [TestMethod]
        public void TestBasic()
        {
            string notesText       = $"{SEL_BEG_IN}2 + 2{SEL_END_IN}";
            string expectNotesText = $"2 + 2 = 4{SEL_BEG_EX}";
            GetModifiedTextSelectionPositions(
                ref notesText, ref expectNotesText,
                out int selectionStart,  out int expectStart,
                out int selectionLength, out _);
            string expr = notesText.Substring(selectionStart, selectionLength);
            CalculateInline(expr, selectionStart, selectionLength, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestParenMulLeft()
        {
            string notesText = $"{SEL_BEG_IN}(2 + 2)3{SEL_END_IN}";
            string expectNotesText = $"(2 + 2)3 = 12{SEL_BEG_EX}";
            GetModifiedTextSelectionPositions(
                ref notesText, ref expectNotesText,
                out int selectionStart, out int expectStart,
                out int selectionLength, out _);
            string expr = notesText.Substring(selectionStart, selectionLength);
            CalculateInline(expr, selectionStart, selectionLength, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestParenMulRight()
        {
            string notesText = $"{SEL_BEG_IN}3(2 + 2){SEL_END_IN}";
            string expectNotesText = $"3(2 + 2) = 12{SEL_BEG_EX}";
            GetModifiedTextSelectionPositions(
                ref notesText, ref expectNotesText,
                out int selectionStart, out int expectStart,
                out int selectionLength, out _);
            string expr = notesText.Substring(selectionStart, selectionLength);
            CalculateInline(expr, selectionStart, selectionLength, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestParenParenMul()
        {
            string notesText = $"{SEL_BEG_IN}(1 + 2)(2 + 2){SEL_END_IN}";
            string expectNotesText = $"(1 + 2)(2 + 2) = 12{SEL_BEG_EX}";
            GetModifiedTextSelectionPositions(
                ref notesText, ref expectNotesText,
                out int selectionStart, out int expectStart,
                out int selectionLength, out _);
            string expr = notesText.Substring(selectionStart, selectionLength);
            CalculateInline(expr, selectionStart, selectionLength, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }
        
        [TestMethod]
        public void TestInnerSelection()
        {
            string notesText = $"2 + ({SEL_BEG_IN}2 * 8{SEL_END_IN}) - 3";
            string expectNotesText = $"2 + (2 * 8 = 16{SEL_BEG_EX}) - 3";
            GetModifiedTextSelectionPositions(
                ref notesText, ref expectNotesText,
                out int selectionStart, out int expectStart,
                out int selectionLength, out _);
            string expr = notesText.Substring(selectionStart, selectionLength);
            CalculateInline(expr, selectionStart, selectionLength, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }
    }

    /// <summary>
    /// These tests are specifically for confirming that <see cref="SubstituteVars"/> is handling the parsing corectly.
    /// Other tests related to substitution should be handled by <see cref="SubstitutionTests">.
    /// </summary>
    [TestClass]
    public class SubstituteVarsTests
    {
        private static void AssertSubstitutionMatchesPrediction(string expectNotesText, string notesText)
        {
            GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
            SubstituteVars(selectionStart, notesText, out int newSelectionStart, out string newNotesText);
            Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
            Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
        }

        [TestMethod]
        public void TestBasic()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3 with x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"2x + 3 with x=3{SEL_BEG_IN}");
        }

        [TestMethod]
        public void TestWhitespaceBefore()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3      with x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"2x + 3      with x=3{SEL_BEG_IN}");
        }

        [TestMethod]
        public void TestWhitespaceBetween()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3 with      x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"2x + 3 with      x=3{SEL_BEG_IN}");
        }

        [TestMethod]
        public void TestWhitespaceAfter()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3 with x=3     \r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"2x + 3 with x=3     {SEL_BEG_IN}");
        }

        [TestMethod]
        public void TestLineBefore()
        {
            AssertSubstitutionMatchesPrediction(
                $"3x-5\r" +
                $"2x + 3 with x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"3x-5\r" +
                $"2x + 3 with x=3{SEL_BEG_IN}");
        }

        [TestMethod]
        public void TestLineAfter()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3 with x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}\r" +
                $"3x-5",

                $"2x + 3 with x=3{SEL_BEG_IN}\r" +
                $"3x-5");
        }

        [TestMethod]
        public void TestSelectingWithin()
        {
            AssertSubstitutionMatchesPrediction(
                $"2x + 3 with x=3\r" +
                $"2(3) + 3{SEL_BEG_EX}",

                $"2x +{SEL_BEG_IN} 3 with x=3");
        }

        [TestMethod]
        public void TestDontAppendLets()
        {
            AssertSubstitutionMatchesPrediction(
                $"let y=x\r" +
                $"a with a=2\r" +
                $"2{SEL_BEG_EX}",

                $"let y=x\r" +
                $"a with a=2{SEL_BEG_IN}");
        }
    }

    //[TestClass]
    //public class BalanceAlgebraTests
    //{
    //    [TestMethod]
    //    public void TestBasic()
    //    {
    //        string notesText       = @$"4 - 2 = 3  with +2{SEL_BEG_IN}";
    //        string expectNotesText = @$"4 = 3 + 2{SEL_BEG_EX}";
    //        GetModifiedTextSelectionPositions(ref notesText, ref expectNotesText, out int selectionStart, out int expectStart, out _, out _);
    //        BalanceAlgebra(selectionStart, notesText, out int newSelectionStart, out string newNotesText);
    //        Assert.AreEqual(expectNotesText, newNotesText, "Notes Text");
    //        Assert.AreEqual(expectStart, newSelectionStart, "Selection Start");
    //    }
    //}
}