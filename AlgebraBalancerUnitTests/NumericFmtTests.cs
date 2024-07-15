using AlgebraBalancer;

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
        [TestMethod]
        public void TestSpaceRemoval()
        {
            Assert.AreEqual(
                "2+2-1/5+3-2",
                ParserFormat("2 + 2   -    1/5   +3     - 2"));
        }

        [TestClass]
        public class ImpliedMultiplicationTests
        {
            [TestMethod, TestCategory("Subexpression"), TestCategory("Number")]
            public void TestNumberTimesSubexpr()
            {
                Assert.AreEqual(
                    "2*(2+3)",
                    ParserFormat("2(2+3)"));
            }

            [TestMethod, TestCategory("Subexpression"), TestCategory("Number")]
            public void TestSubexprTimesNumber()
            {
                Assert.AreEqual(
                    "(2+3)*2",
                    ParserFormat("(2+3)2"));
            }

            [TestMethod, TestCategory("Subexpression")]
            public void TestSubexprTimesSubexpr()
            {
                Assert.AreEqual(
                    "(2+3)*(2+5)",
                    ParserFormat("(2+3)(2+5)"));
            }

            [TestMethod, TestCategory("Variable"), TestCategory("Number")]
            public void TestNumberTimesVariable()
            {
                Assert.AreEqual(
                    "2*x",
                    ParserFormat("2x"));
            }

            [TestMethod, TestCategory("Variable"), TestCategory("Number")]
            public void TestVariableTimesNumber()
            {
                Assert.AreEqual(
                    "x*2",
                    ParserFormat("x2"));
            }

            [TestMethod, TestCategory("Variable"), TestCategory("Subexpr")]
            public void TestSubexprTimesVariable()
            {
                Assert.AreEqual(
                    "(2+2)*x",
                    ParserFormat("(2+2)x"));
            }

            [TestMethod, TestCategory("Variable"), TestCategory("Subexpr")]
            public void TestVariableTimesSubexpr()
            {
                Assert.AreEqual(
                    "x*(2+2)",
                    ParserFormat("x(2+2)"));
            }

            [TestMethod, TestCategory("Variable")]
            public void TestVariableTimesVariable()
            {
                Assert.AreEqual(
                    "x*y",
                    ParserFormat("xy"));
            }

            [TestMethod, TestCategory("Number")]
            public void TestNumberNearNumber()
            {
                Assert.AreNotEqual(
                    "23*65",
                    ParserFormat("23 65"));
            }

            [TestMethod, TestCategory("Number")]
            public void TestSubtractionNotMultiplication()
            {
                Assert.AreNotEqual(
                    "23*-65",
                    ParserFormat("23 -65"));
            }
        }

        [TestClass]
        public class ImaginaryTests
        {
            [TestMethod]
            public void TestImaginaryItalicToAscii()
            {
                Assert.AreEqual(
                    "3+6*i-4",
                    ParserFormat("3+6𝑖-4"));
            }

            [TestMethod]
            public void TestImaginaryComplexToAscii()
            {
                Assert.AreEqual(
                    "3+6*i-4",
                    ParserFormat("3+6ⅈ-4"));
            }
        }

        [TestClass]
        public class MulTests
        {
            [TestMethod]
            public void TestMulCDotToAscii()
            {
                Assert.AreEqual(
                    "3+6*4",
                    ParserFormat("3+6⋅4"));
            }

            [TestMethod]
            public void TestMulTimesToAscii()
            {
                Assert.AreEqual(
                    "3+6*4",
                    ParserFormat("3+6×4"));
            }
        }

        [TestClass]
        public class DivTests
        {
            [TestMethod]
            public void TestDivDivToSlash()
            {
                Assert.AreEqual(
                    "3+6/4",
                    ParserFormat("3+6÷4"));
            }
        }
    }

    ////////////////////
    // Display format //
    ////////////////////

    [TestClass]
    public class DisplayFormatTests
    {
        [TestMethod]
        public void TestDefault()
        {
            Assert.AreEqual(
                "3+6/4=23i+7³",
                DisplayFormat(
                    "3+6/4=23*i+7^3"
                ));
        }

        [TestMethod]
        public void TestSpacesAroundBinary()
        {
            Assert.AreEqual(
                "3 + 6 / 4=23i + 7³",
                DisplayFormat(
                    "3+6/4=23*i+7^3",
                    FormatOptions.SpaceAroundBinaryOperators
                ));
        }

        [TestMethod]
        public void TestSpacesAroundRelational()
        {
            Assert.AreEqual(
                "3+6/4 = 23i+7³",
                DisplayFormat(
                    "3+6/4=23*i+7^3",
                    FormatOptions.SpaceAroundRelationalOperators
                ));
        }

        [TestMethod]
        public void TestSpacesAroundBinaryAndRelational()
        {
            Assert.AreEqual(
                "3 + 6 / 4 = 23i + 7³",
                DisplayFormat(
                    "3+6/4=23*i+7^3",
                    FormatOptions.SpaceAroundBinaryOperators | FormatOptions.SpaceAroundRelationalOperators
                ));
        }

        [TestMethod]
        public void TestAsciiImaginary()
        {
            Assert.AreEqual(
                "2i",
                DisplayFormat(
                    "2i",
                    FormatOptions.AsciiImaginary
                ));
        }

        [TestMethod]
        public void TestItalicImaginary()
        {
            Assert.AreEqual(
                "2𝑖",
                DisplayFormat(
                    "2i",
                    FormatOptions.ItalicImaginary
                ));
        }

        [TestMethod]
        public void TestComplexImaginary()
        {
            Assert.AreEqual(
                "2ⅈ",
                DisplayFormat(
                    "2i",
                    FormatOptions.ComplexImaginary
                ));
        }

        [TestMethod]
        public void TestSlashDiv()
        {
            Assert.AreEqual(
                "6/4",
                DisplayFormat(
                    "6/4",
                    FormatOptions.SlashDiv
                ));
        }

        [TestMethod]
        public void TestDivDiv()
        {
            Assert.AreEqual(
                "6÷4",
                DisplayFormat(
                    "6/4",
                    FormatOptions.DivDiv
                ));
        }

        [TestMethod]
        public void TestAsciiMul()
        {
            Assert.AreEqual(
                "3*3",
                DisplayFormat(
                    "3*3",
                    FormatOptions.AsciiMul
                ));
        }

        [TestMethod]
        public void TestTimesMul()
        {
            Assert.AreEqual(
                "3×3",
                DisplayFormat(
                    "3*3",
                    FormatOptions.TimesMul
                ));
        }

        [TestMethod]
        public void TestCDotMul()
        {
            Assert.AreEqual(
                "3⋅3",
                DisplayFormat(
                    "3*3",
                    FormatOptions.CDotMul
                ));
        }

        [TestMethod]
        public void TestAddNegative()
        {
            Assert.AreEqual(
                "3-3",
                DisplayFormat(
                    "3+-3"
                ));
        }

        [TestMethod]
        public void TestSubNegative()
        {
            Assert.AreEqual(
                "3+3",
                DisplayFormat(
                    "3--3"
                ));
        }

        [TestMethod]
        public void TestAddSubChainA()
        {
            Assert.AreEqual(
                "3-3",
                DisplayFormat(
                    "3+--+-+---+-+++-+-3"
                ));
        }

        [TestMethod]
        public void TestAddSubChainB()
        {
            Assert.AreEqual(
                "3+3",
                DisplayFormat(
                    "3+--+-+---+-+++-+--3"
                ));
        }

        [TestMethod]
        public void TestIdentityProduct()
        {
            Assert.AreEqual(
                "x",
                DisplayFormat(
                    "1*x*1"
                ));
        }

        [TestMethod]
        public void TestNegativeIdentityProduct()
        {
            Assert.AreEqual(
                "-x",
                DisplayFormat(
                    "-1*x"
                ));
        }

        [TestMethod]
        public void TestIdentityQuotient()
        {
            Assert.AreEqual(
                "x",
                DisplayFormat(
                    "x/1"
                ));
        }

        [TestMethod]
        public void TestIdentitySum()
        {
            Assert.AreEqual(
                "x",
                DisplayFormat(
                    "0+x+0"
                ));
        }

        [TestMethod]
        public void TestIdentityDifference()
        {
            Assert.AreEqual(
                "x",
                DisplayFormat(
                    "0-x-0"
                ));
        }

        [TestMethod]
        public void TestChainedIdentitySumDifference()
        {
            Assert.AreEqual(
                "x",
                DisplayFormat(
                    "0+-+-+-+0+-+-+-+-+x+-+-+-+-+0+-+-+-+0"
                ));
        }

        [TestMethod]
        public void TestRadical()
        {
            Assert.AreEqual(
                "2i√6",
                DisplayFormat(
                    "2*i*√6"
                ));
        }

        [TestMethod]
        public void TestRadicalExactCalculations()
        {
            Assert.AreEqual(
                "2𝑖√6",
                DisplayFormat(
                    "2*i*√6",
                    MainPage.EXACT_CALC_FMT
                ));
        }
    }
}
