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
                "6",
                new SubstitutableString(
                    "x",
                    "x").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "x", "6" },
                    }));
        }

        [TestMethod]
        public void TestMultipleParameters()
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

        [TestMethod]
        public void TestMultiLetterArgNames()
        {
            Assert.AreEqual(
                "276",
                new SubstitutableString(
                    "2appleorange",
                    "apple", "orange").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "apple", "7" },
                        { "orange", "6" },
                    }));
        }

        [TestMethod]
        public void TestGreedyNames()
        {
            Assert.AreEqual(
                "29+3",
                new SubstitutableString(
                    "2xx+x",
                    "x", "xx").GetSubstituted(
                    new Dictionary<string, string>() {
                        { "x", "3" },
                        { "xx", "9" },
                    }));
        }
    }

    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var x = Variable.TryDefine("x", "5");
            Assert.AreEqual("(5)", x.GetReplacement("x"));
        }
    }

    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var f = Formula.TryDefine("f(x)", "3x");
            Assert.AreEqual("3(7)", f.GetReplacement("f(7)"));
        }

        [TestMethod]
        public void TestMultipleParameters()
        {
            var f = Formula.TryDefine("f(a,b)", "3a+2b");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("f(2,6)"));
        }
    }

    [TestClass]
    public class AnonymousFormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var f = AnonymousFormula.TryDefine("x=>3x");
            Assert.AreEqual("3(7)", f.GetReplacement("(7)"));
        }

        [TestMethod]
        public void TestMultipleParameters()
        {
            var f = AnonymousFormula.TryDefine("a b=>3a+2b");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("(2,6)"));
        }

        [TestMethod]
        public void TestWrappedParameters()
        {
            var f = AnonymousFormula.TryDefine("(a,b=>3a+2b)");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("(2,6)"));
        }

        [TestMethod]
        public void TestUnwrappedWrappedParameters()
        {
            var f = AnonymousFormula.TryDefine("a,b=>3a+2b");
            Assert.AreEqual("3a+2(2)", f.GetReplacement("(2,6)")); // because `a` should be considered separate, and 6 gets dropped
        }
    }

    [TestClass]
    public class GetStatementClausesTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestOneClause()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "b")
            ], SubstitutionParser.GetStatementClauses(
                "a = b"
            ));
        }

        [TestMethod]
        public void TestTwoClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "b"),
                ("b", "c")
            ], SubstitutionParser.GetStatementClauses(
                "a = b and b = c"
            ));
        }

        [TestMethod]
        public void TestNoClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
            ], SubstitutionParser.GetStatementClauses(
                ""
            ));
        }

        [TestMethod]
        public void TestCommaSeparatedClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "b"),
                ("b", "c")
            ], SubstitutionParser.GetStatementClauses(
                "a = b, b = c"
            ));
        }

        [TestMethod]
        public void TestCommaSeparatedVectorClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "(3, 4)"),
                ("b", "(5, 6)"),
                ("c", "(1, 67)")
            ], SubstitutionParser.GetStatementClauses(
                "a = (3, 4), b = (5, 6), c = (1, 67)"
            ));
        }
    }

    [TestClass]
    public class ParseDefinesTests
    {
        [TestMethod]
        public void TestBasic()
        {
            CollectionAssert.AreEqual((List<(string, string)>)[
                ("a", "5")
            ], AtLine(SubstitutionParser.ParseDefines,
                "let a = 5\r",
                1
            ));
        }

        [TestMethod]
        public void TestMultipleClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "3"),
                ("b", "5"),
                ("c", "1"),
            ], AtLine(SubstitutionParser.ParseDefines,
                "let a = 3, b = 5, c = 1\r",
                1
            ));
        }

        [TestMethod]
        public void TestMultipleVectorClauses()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "(3, 4)"),
                ("b", "(5, 6)"),
                ("c", "(1, 67)"),
            ], AtLine(SubstitutionParser.ParseDefines, 
                "let a = (3, 4), b = (5, 6), c = (1, 67)\r",
                1
            ));
        }

        [TestMethod]
        public void TestMultipleLets()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("c", "1"),
                ("b", "5"),
                ("a", "3"),
            ], AtLine(SubstitutionParser.ParseDefines,
                "let a = 3\r" +
                "let b = 5\r" +
                "let c = 1\r",
                3
            ));
        }

        [TestMethod]
        public void TestWith()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "3"),
            ], AtLine(SubstitutionParser.ParseDefines,
                "a with a = 3",
                0
            ));
        }

        [TestMethod]
        public void TestLetAndWith()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("a", "3"),
                ("b", "5"),
                ("c", "1"),
                ("x", "9"),
            ], AtLine(SubstitutionParser.ParseDefines,
                "let a = 3, b = 5, c = 1\r" +
                "x with x = 9",
                1
            ));
        }
    }
}
