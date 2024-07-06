using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Substitute;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using static AlgebraBalancerUnitTests.TestHelper;

namespace AlgebraBalancerUnitTests;

[TestClass]
public class SubstitutionTests
{
    [TestClass]
    public class SubstitutableStringTests
    {
        [TestMethod]
        public void TestBasic()
        {
            Assert.AreEqual(
                "629",
                new SubstitutableString(
                    "xyz",
                    "x", "y", "z").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "x", "6" },
                        { "y", "2" },
                        { "z", "9" },
                    }));
        }

        [TestMethod]
        public void TestMultipleInstancesOfSameParameter()
        {
            Assert.AreEqual(
                "5 5 5",
                new SubstitutableString(
                    "x x x",
                    "x").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "x", "5" },
                    }));
        }

        [TestMethod]
        public void TestReplacementInsideExpression()
        {
            Assert.AreEqual(
                "2(3-4)+76",
                new SubstitutableString(
                    "2(x-4)+76",
                    "x").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "x", "3" },
                    }));
        }
    }

    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var x = Variable.Define("x", "5");
            Assert.AreEqual("(5)", x.GetReplacement("x"));
        }
    }

    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var x = Formula.Define("x", "5");
            Assert.AreEqual("(5)", x.GetReplacement("x"));
        }
    }

    [TestClass]
    public class ParseTests
    {
        private static List<ISubstitutible> ParseAtLine(string document, int lineNumber)
        {
            var (lineStart, lineEnd) = GetLineRange(document, lineNumber);
            return SubstitutionParser.Parse(document, lineStart, lineEnd);
        }

        //[TestMethod]
        //public void TestParseBasic()
        //{
        //    CollectionAssert.AreEquivalent(
        //        (ISubstitutible[])[new Variable("x", "2")],
        //        ParseAtLine(
        //            "let x = 2\r" +
        //            "x",
        //            1));
        //}
    }

    //[TestClass]
    //public class CleanParensTests
    //{
    //    [TestMethod]
    //    public void TestFullExprParens()
    //    {
    //        Assert.AreEqual(
    //            "a + b",
    //            Relationship.CleanParentheses("(a + b)"));
    //    }

    //    [TestMethod]
    //    public void TestNotFullParens()
    //    {
    //        Assert.AreEqual(
    //            "(a + b)(a + b)",
    //            Relationship.CleanParentheses("(a + b)(a + b)"));
    //    }

    //    [TestMethod]
    //    public void TestDouble()
    //    {
    //        Assert.AreEqual(
    //            "a + b",
    //            Relationship.CleanParentheses("((a + b))"));
    //    }

    //    [TestMethod]
    //    public void TestManyRedundant()
    //    {
    //        Assert.AreEqual(
    //            "a + b",
    //            Relationship.CleanParentheses("(((((a + b)))))"));
    //    }

    //    [TestMethod]
    //    public void TestFullWithSubParens()
    //    {
    //        Assert.AreEqual(
    //            "6(2343) + 3(454)",
    //            Relationship.CleanParentheses("(6(2343) + 3(454))"));
    //    }

    //    [TestMethod]
    //    public void TestRedundantInnerParens()
    //    {
    //        Assert.AreEqual(
    //            "6(2343) + 3(454)",
    //            Relationship.CleanParentheses("6((2343)) + 3((454))"));
    //    }

    //    [TestMethod]
    //    public void TestRedundantInnerParensContainingParens()
    //    {
    //        Assert.AreEqual(
    //            "2(3-3/2)+2",
    //            Relationship.CleanParentheses("2((3-(3)/2))+2"));
    //    }

    //    [TestMethod]
    //    public void TestDontTouchVectors()
    //    {
    //        Assert.AreEqual(
    //            "(5, 7)",
    //            Relationship.CleanParentheses("(5, 7)"));
    //    }

    //    [TestMethod]
    //    public void TestStillCleanAroundVectors()
    //    {
    //        Assert.AreEqual(
    //            "(5, 7)",
    //            Relationship.CleanParentheses("((((5, 7))))"));
    //    }

    //    [TestMethod]
    //    public void TestDontTouchSubVectors()
    //    {
    //        Assert.AreEqual(
    //            "(5, 7) + (5, 7)",
    //            Relationship.CleanParentheses("((5, 7) + (5, 7))"));
    //    }

    //    [TestMethod]
    //    public void TestStillCleanAroundSubVectors()
    //    {
    //        Assert.AreEqual(
    //            "(5, 7) + (5, 7)",
    //            Relationship.CleanParentheses("(((5, 7)) + ((5, 7)))"));
    //    }

    //    [TestMethod]
    //    public void TestVectorWithParentheticalComponents()
    //    {
    //        Assert.AreEqual(
    //            "(5, 3(5))",
    //            Relationship.CleanParentheses("((5, 3(5)))"));
    //    }
    //}

    //[TestClass]
    //public class SubstitutionTests
    //{
    //    [TestMethod]
    //    public void TestSubstituteX()
    //    {
    //        Assert.AreEqual(
    //            "(4-(4)^2) + 5 = 2(4) - 3",
    //            Relationship.Substitute(
    //                "(x-x^2) + 5 = 2x - 3",
    //                "x=4")
    //        );
    //    }

    //    [TestMethod]
    //    public void TestSubstituteXY()
    //    {
    //        Assert.AreEqual(
    //            "(6-(6)^2) + 5 = 2(4) - 3",
    //            Relationship.Substitute(
    //                "(y-y^2) + 5 = 2x - 3",
    //                "x=4;y=6")
    //        );
    //    }

    //    [TestMethod]
    //    public void TestSubstituteXYX()
    //    {
    //        // Should stop after one iteration
    //        Assert.AreEqual(
    //            "(2x-(2x)^2) + 5 = 2(4) - 3",
    //            Relationship.Substitute(
    //                "(y-y^2) + 5 = 2x - 3",
    //                "x=4;y=2x")
    //        );
    //    }

    //    [TestMethod]
    //    public void TestSubstituteYXX()
    //    {
    //        // Order matters
    //        Assert.AreEqual(
    //            "((2(4))-(2(4))^2) + 5 = 2(4) - 3",
    //            Relationship.Substitute(
    //                "(y-y^2) + 5 = 2x - 3",
    //                "y=2x;x=4")
    //        );
    //    }

    //    [TestMethod]
    //    public void TestSubstituteRecursive()
    //    {
    //        Assert.AreEqual(
    //            "(2x-(2x)^2) + 5 = 2(2(2x)) - 3",
    //            Relationship.Substitute(
    //                "(y-y^2) + 5 = 2x - 3",
    //                "x=2y;y=2x")
    //        );
    //    }

    //    [TestMethod]
    //    public void TestSubstituteRecursionNotInfinite()
    //    {
    //        try
    //        {
    //            string result = Relationship.Substitute("f(x)", "f(x) = f(f(x))");
    //            Assert.IsTrue(new Regex(@"(?:f\()+x\)+").IsMatch(result));
    //        }
    //        catch (System.Exception err)
    //        {
    //            Assert.Fail($"Expected:no exception. Actual:<{err.Message}>.");
    //        }
    //    }

    //    [TestMethod]
    //    public void TestOperatorIsNotName()
    //    {
    //        Assert.AreEqual(
    //            "4+4-4*4/4=4%(4)^(4)&(4)|4",
    //            Relationship.Substitute(
    //                "x+x-x*x/x=x%x^x&x|x",
    //                "x=4")
    //        );
    //    }

    //    [TestClass]
    //    public class FunctionTests
    //    {
    //        [TestMethod]
    //        public void TestF()
    //        {
    //            Assert.AreEqual(
    //                "2(5)",
    //                Relationship.Substitute(
    //                    "f(5)",
    //                    "f(x)=2x")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestFuncInsideParens()
    //        {
    //            Assert.AreEqual(
    //                "2(5)",
    //                Relationship.Substitute(
    //                    "(f(5))",
    //                    "f(x)=2x")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestFuncInsideSubexpression()
    //        {
    //            Assert.AreEqual(
    //                "7(2(5)) + 4",
    //                Relationship.Substitute(
    //                    "(7f(5) + 4)",
    //                    "f(x)=2x")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestFOfG()
    //        {
    //            Assert.AreEqual(
    //                "2(3-3/2)+2",
    //                Relationship.Substitute(
    //                    "f(g(3))",
    //                    "f(x)=2x+2;g(x)=3-x/2")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestOrderDoesntMatter()
    //        {
    //            Assert.AreEqual(
    //                Relationship.Substitute(
    //                    "f(g(3))",
    //                    "f(x)=2x+2;g(x)=3-x/2"),
    //                Relationship.Substitute(
    //                    "f(g(3))",
    //                    "g(x)=3-x/2;f(x)=2x+2")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestMultipleParameters()
    //        {
    //            Assert.AreEqual(
    //                "2(2)+3(7)",
    //                Relationship.Substitute(
    //                    "f(2,7)",
    //                    "f(a,b)=2a+3b")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestVectorParameter()
    //        {
    //            Assert.AreEqual(
    //                "(3,6)/(5,2)",
    //                Relationship.Substitute(
    //                    "f((3,6), (5,2))",
    //                    "f(a,b)=a/b")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestParameterInMultiplePlaces()
    //        {
    //            Assert.AreEqual(
    //                "6+2(6)",
    //                Relationship.Substitute(
    //                    "f(6)",
    //                    "f(x)=x+2x")
    //            );
    //        }

    //        [TestMethod]
    //        public void TestOperatorIsNotName()
    //        {
    //            Assert.AreEqual(
    //                "(2(x))+(2(x))-(2(x))*(2(x))/(2(x))=(2(x))%(2(x))^(2(x))&(2(x))|(2(x))",
    //                Relationship.Substitute(
    //                    "f(x)+f(x)-f(x)*f(x)/f(x)=f(x)%f(x)^f(x)&f(x)|f(x)",
    //                    "f(x)=2x")
    //            );
    //        }

    //        [TestClass]
    //        public class MappedFunctionTests
    //        {
    //            [TestMethod]
    //            public void TestSingle()
    //            {
    //                Assert.AreEqual(
    //                    "5",
    //                    Relationship.Substitute(
    //                        "f(3)",
    //                        "f(x)={3->5}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestTwo()
    //            {
    //                Assert.AreEqual(
    //                    "(5) (2)",
    //                    Relationship.Substitute(
    //                        "f(3) f(7)",
    //                        "f(x)={3->5,7->2}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestMany()
    //            {
    //                Assert.AreEqual(
    //                    "(5) (2) (6) (2)",
    //                    Relationship.Substitute(
    //                        "f(3) f(7) f(4) f(9)",
    //                        "f(x)={3->5,7->2,4->6,9->2}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestVectorOutput()
    //            {
    //                Assert.AreEqual(
    //                    "(3,5)",
    //                    Relationship.Substitute(
    //                        "f(2)",
    //                        "f(x)={2->(3,5)}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestParentheticMapping()
    //            {
    //                Assert.AreEqual(
    //                    "7",
    //                    Relationship.Substitute(
    //                        "f((((9))))",
    //                        "f(x)={9->7}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestParameterExpression()
    //            {
    //                Assert.AreEqual(
    //                    "2",
    //                    Relationship.Substitute(
    //                        "f(2*2+(10/5)-1)",
    //                        "f(x)={5->2}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestParenthesesMultiplicationInParameterExpression()
    //            {
    //                Assert.AreEqual(
    //                    "43",
    //                    Relationship.Substitute(
    //                        "f(2(3))",
    //                        "f(x)={6->43}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestNestedMapping()
    //            {
    //                Assert.AreEqual(
    //                    "8",
    //                    Relationship.Substitute(
    //                        "f(3)",
    //                        "f(x)=f'(x);f'(x)={3->8}")
    //                );
    //            }

    //            [TestMethod]
    //            public void TestExpandBeforePassing()
    //            {
    //                Assert.AreEqual(
    //                    "345",
    //                    Relationship.Substitute(
    //                        "f(n)",
    //                        "f(x)=f'(2x);f'(x)={20->345};n=10")
    //                );
    //            }
    //        }

    //        [TestClass]
    //        public class AnonymousCallsTests
    //        {
    //            [TestMethod]
    //            public void TestBasic()
    //            {
    //                Assert.AreEqual(
    //                    "2(4)",
    //                    Relationship.AnonymousCalls("(x=>2x)(4)"));
    //            }

    //            [TestMethod]
    //            public void TestTwoArgs()
    //            {
    //                Assert.AreEqual(
    //                    "5+7",
    //                    Relationship.AnonymousCalls("(x y=>x+y)(5,7)"));
    //            }

    //            [TestMethod]
    //            public void TestWithinGreaterExpression()
    //            {
    //                Assert.AreEqual(
    //                    "2(3/2)+6",
    //                    Relationship.AnonymousCalls("2(x=>x/2)(3)+6"));
    //            }

    //            [TestMethod]
    //            public void TestSubstitutionWithLambdaParamter()
    //            {
    //                Assert.AreEqual(
    //                    "2(3(5))",
    //                    Relationship.Substitute(
    //                        "f(x=>3x)",
    //                        "f(x)=2x(5)"));
    //            }

    //            [TestMethod]
    //            public void TestSubstitutionWithMultiArgumentLambdaParamter()
    //            {
    //                Assert.AreEqual(
    //                    "2(1)+n",
    //                    Relationship.Substitute(
    //                        "f(a b=>2a+b)",
    //                        "f(x)=x(1, n)"));
    //            }

    //            [TestMethod]
    //            public void TestSubstitutionWithMultipleLambdaParamters()
    //            {
    //                Assert.AreEqual(
    //                    "2(3(9))",
    //                    Relationship.Substitute(
    //                        "f(x=>2x,y=>3y)",
    //                        "f(a,b)=a(b(9))"));
    //            }

    //            [TestMethod]
    //            public void TestSubstitutionWithMultipleLambdaParamtersSharingArgNames()
    //            {
    //                Assert.AreEqual(
    //                    "2(3(9))",
    //                    Relationship.Substitute(
    //                        "f(x=>2x,x=>3x)",
    //                        "f(a,b)=a(b(9))"));
    //            }

    //            [TestMethod]
    //            public void TestSubstitutionWithMultipleOutOfOrderLambdaParamtersSharingArgNames()
    //            {
    //                Assert.AreEqual(
    //                    "3(2(9))",
    //                    Relationship.Substitute(
    //                        "f(x=>2x,x=>3x)",
    //                        "f(a,b)=b(a(9))"));
    //            }
    //        }
    //    }
    //}
}
