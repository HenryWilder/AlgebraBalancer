using Microsoft.VisualStudio.TestTools.UnitTesting;

using static AlgebraBalancer.Algebra.NumericFmt;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class NumericFmtTests
{
    //////////////////////////
    // Identify preferences //
    //////////////////////////

    [TestClass]
    public class IdentifyPreferenceTests
    {
        [TestClass]
        public class SpaceAroundOperatorTests
        {
            [TestClass]
            public class BinaryTests
            {
                [TestMethod]
                public void TestSpaceAroundBinaryOperators()
                {
                    Assert.IsTrue(
                        IdentifyPreference("1 + 1")
                        .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                        "Should detect space");
                }

                [TestMethod]
                public void TestNoSpaceAroundBinaryOperators()
                {
                    Assert.IsFalse(
                        IdentifyPreference("1+1")
                        .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                        "Should refuse lack of space");
                }

                [TestMethod]
                public void TestMixedSpaceAroundBinaryOperators()
                {
                    Assert.IsTrue(
                        IdentifyPreference("1+1 - 2")
                        .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                        "Should prioritize spaces over lack of spaces");
                }

                [TestClass]
                public class UnaryMinusTests
                {
                    [TestMethod]
                    public void TestSpaceAfterUnaryMinus()
                    {
                        Assert.IsFalse(
                            IdentifyPreference("- 1+4")
                            .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                            "Should ignore spaces involving unary minus");
                    }

                    [TestMethod]
                    public void TestSpaceAroundUnaryMinus()
                    {
                        Assert.IsFalse(
                            IdentifyPreference(" - 1+4")
                            .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                            "Should ignore spaces involving unary minus");
                    }

                    [TestMethod]
                    public void TestSpaceAfterUnaryMinusSubexpression()
                    {
                        Assert.IsFalse(
                            IdentifyPreference("3+(- 1+4)")
                            .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                            "Should ignore spaces involving unary minus");
                    }

                    [TestMethod]
                    public void TestSpaceAroundUnaryMinusSubexpression()
                    {
                        Assert.IsFalse(
                            IdentifyPreference("3+( - 1+4)")
                            .HasFlag(FormatOptions.SpaceAroundBinaryOperators),
                            "Should ignore spaces involving unary minus");
                    }
                }
            }

            [TestClass]
            public class RelationalTests
            {
                [TestMethod]
                public void TestSpaceAroundRelationalOperators()
                {
                    Assert.IsTrue(
                        IdentifyPreference("1 = 1")
                        .HasFlag(FormatOptions.SpaceAroundRelationalOperators),
                        "Should detect space");
                }

                [TestMethod]
                public void TestNoSpaceAroundRelationalOperators()
                {
                    Assert.IsFalse(
                        IdentifyPreference("1=1")
                        .HasFlag(FormatOptions.SpaceAroundRelationalOperators),
                        "Should refuse lack of space");
                }

                [TestMethod]
                public void TestMixedSpaceAroundRelationalOperators()
                {
                    Assert.IsTrue(
                        IdentifyPreference("1=1 = 1")
                        .HasFlag(FormatOptions.SpaceAroundRelationalOperators),
                        "Should prioritize spaces over lack of spaces");
                }
            }
        }

        [TestClass]
        public class ImaginaryTests
        {
            private const FormatOptions IMAGINARY_OPTION =
                FormatOptions.AsciiImaginary | FormatOptions.ItalicImaginary | FormatOptions.ComplexImaginary;

            [TestMethod, TestCategory("ASCII Imaginary")]
            public void TestImaginaryAsciiExplicit()
            {
                Assert.AreEqual(
                    FormatOptions.AsciiImaginary,
                    IdentifyPreference("2+6i+7") & IMAGINARY_OPTION,
                    "Should detect ascii imaginary");
            }

            [TestMethod, TestCategory("ASCII Imaginary")]
            public void TestImaginaryAsciiDefault()
            {
                Assert.AreEqual(
                    FormatOptions.AsciiImaginary,
                    IdentifyPreference("23+7") & IMAGINARY_OPTION,
                    "Should default to ascii imaginary");
            }

            [TestMethod, TestCategory("Italic Imaginary")]
            public void TestImaginaryItalic()
            {
                Assert.AreEqual(
                    FormatOptions.ItalicImaginary,
                    IdentifyPreference("2+6𝑖+7") & IMAGINARY_OPTION,
                    "Should detct italic imaginary");
            }

            [TestMethod, TestCategory("Italic Imaginary")]
            public void TestImaginaryItalicPriority()
            {
                Assert.AreEqual(
                    FormatOptions.ItalicImaginary,
                    IdentifyPreference("2+6𝑖+7i") & IMAGINARY_OPTION,
                    "Should prioratize italic imaginary over ascii");
            }

            [TestMethod, TestCategory("Complex Imaginary")]
            public void TestImaginaryComplex()
            {
                Assert.AreEqual(
                    FormatOptions.ComplexImaginary,
                    IdentifyPreference("2+6ⅈ+7") & IMAGINARY_OPTION,
                    "Should detct complex imaginary");
            }

            [TestMethod, TestCategory("Complex Imaginary")]
            public void TestImaginaryComplexPriority()
            {
                Assert.AreEqual(
                    FormatOptions.ComplexImaginary,
                    IdentifyPreference("2+6ⅈ+7i") & IMAGINARY_OPTION,
                    "Should prioratize complex imaginary over ascii");
            }

            [TestMethod, TestCategory("Complex Imaginary")]
            public void TestImaginaryComplexItalicPriority()
            {
                Assert.AreEqual(
                    FormatOptions.ComplexImaginary,
                    IdentifyPreference("2+6ⅈ+7𝑖") & IMAGINARY_OPTION,
                    "Should prioratize complex imaginary over italic");
            }
        }

        [TestClass]
        public class MulTests
        {
            private const FormatOptions TIMES_OPTION =
                FormatOptions.AsciiMul | FormatOptions.TimesMul | FormatOptions.CDotMul;

            [TestMethod, TestCategory("ASCII Multiply")]
            public void TestMulAsciiExplicit()
            {
                Assert.AreEqual(
                    FormatOptions.AsciiMul,
                    IdentifyPreference("4*4") & TIMES_OPTION,
                    "Should detect ascii multiplication");
            }

            [TestMethod, TestCategory("ASCII Multiply")]
            public void TestMulAsciiDefault()
            {
                Assert.AreEqual(
                    FormatOptions.AsciiMul,
                    IdentifyPreference("4+4") & TIMES_OPTION,
                    "Should default to ascii multiplication");
            }

            [TestMethod, TestCategory("Times Multiply")]
            public void TestMulTimes()
            {
                Assert.AreEqual(
                    FormatOptions.TimesMul,
                    IdentifyPreference("4×4") & TIMES_OPTION,
                    "Should detect times multiplication");
            }

            [TestMethod, TestCategory("Times Multiply")]
            public void TestMulTimesPriority()
            {
                Assert.AreEqual(
                    FormatOptions.TimesMul,
                    IdentifyPreference("2*4×4") & TIMES_OPTION,
                    "Should prioritize times multiplication over ascii");
            }

            [TestMethod, TestCategory("CDot Multiply")]
            public void TestMulCDot()
            {
                Assert.AreEqual(
                    FormatOptions.CDotMul,
                    IdentifyPreference("4⋅4") & TIMES_OPTION,
                    "Should detect cdot multiplication");
            }

            [TestMethod, TestCategory("CDot Multiply")]
            public void TestMulCDotPriority()
            {
                Assert.AreEqual(
                    FormatOptions.CDotMul,
                    IdentifyPreference("2*4⋅4") & TIMES_OPTION,
                    "Should prioritize cdot multiplication over ascii");
            }

            [TestMethod, TestCategory("CDot Multiply")]
            public void TestMulCDotTimesPriority()
            {
                Assert.AreEqual(
                    FormatOptions.CDotMul,
                    IdentifyPreference("2×4⋅4") & TIMES_OPTION,
                    "Should prioritize cdot multiplication over ascii");
            }
        }

        [TestClass]
        public class DivTests
        {
            private const FormatOptions DIV_OPTION =
                FormatOptions.SlashDiv | FormatOptions.DivDiv;

            [TestMethod, TestCategory("Slash Division")]
            public void TestDivSlashExplicit()
            {
                Assert.AreEqual(
                    FormatOptions.SlashDiv,
                    IdentifyPreference("4/4") & DIV_OPTION,
                    "Should detect slash division");
            }

            [TestMethod, TestCategory("Slash Division")]
            public void TestDivSlashDefault()
            {
                Assert.AreEqual(
                    FormatOptions.SlashDiv,
                    IdentifyPreference("4+4") & DIV_OPTION,
                    "Should default to slash division");
            }

            [TestMethod, TestCategory("Div Division")]
            public void TestDivDiv()
            {
                Assert.AreEqual(
                    FormatOptions.DivDiv,
                    IdentifyPreference("4÷4") & DIV_OPTION,
                    "Should default to div division");
            }

            [TestMethod, TestCategory("Div Division")]
            public void TestDivDivPriority()
            {
                Assert.AreEqual(
                    FormatOptions.DivDiv,
                    IdentifyPreference("2/4÷4") & DIV_OPTION,
                    "Should prioritize div division over slash");
            }
        }
    }

    ///////////////////
    // Parser format //
    ///////////////////

    [TestClass]
    public class ParserFormatTests
    {

    }

    ////////////////////
    // Display format //
    ////////////////////

    [TestClass]
    public class DisplayFormatTests
    {

    }
}
