using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlgebraBalancer.Substitute;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using static AlgebraBalancerUnitTests.TestHelper;
using System.Collections;

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
            Assert.AreEqual("(5)", x.GetReplacement("x", new Substitutor("", 0, 0, out _)));
        }
    }

    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var f = Formula.TryDefine("f(x)", "3x");
            Assert.AreEqual("3(7)", f.GetReplacement("f(7)", new Substitutor("", 0, 0, out _)));
        }

        [TestMethod]
        public void TestMultipleParameters()
        {
            var f = Formula.TryDefine("f(a,b)", "3a+2b");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("f(2,6)", new Substitutor("", 0, 0, out _)));
        }
    }

    [TestClass]
    public class AnonymousFormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var f = AnonymousFormula.TryDefine("x=>3x");
            Assert.AreEqual("3(7)", f.GetReplacement("(7)", new Substitutor("", 0, 0, out _)));
        }

        [TestMethod]
        public void TestMultipleParameters()
        {
            var f = AnonymousFormula.TryDefine("a b=>3a+2b");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("(2,6)", new Substitutor("", 0, 0, out _)));
        }

        [TestMethod]
        public void TestWrappedParameters()
        {
            var f = AnonymousFormula.TryDefine("(a,b=>3a+2b)");
            Assert.AreEqual("3(2)+2(6)", f.GetReplacement("(2,6)", new Substitutor("", 0, 0, out _)));
        }

        [TestMethod]
        public void TestUnwrappedWrappedParameters()
        {
            var f = AnonymousFormula.TryDefine("a,b=>3a+2b");
            Assert.AreEqual("3a+2(2)", f.GetReplacement("(2,6)", new Substitutor("", 0, 0, out _))); // because `a` should be considered separate, and 6 gets dropped
        }
    }

    [TestClass]
    public class MappedFormulaTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var f = MappedFormula.TryDefine("f(x)", "{2->3,5->8}");
            Assert.AreEqual("(3)", f.GetReplacement("f(2)", new Substitutor("", 0, 0, out _)));
            Assert.AreEqual("(8)", f.GetReplacement("f(5)", new Substitutor("", 0, 0, out _)));
            Assert.AreEqual("(∄)", f.GetReplacement("f(8)", new Substitutor("", 0, 0, out _)));
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
        private static List<(string, string)> ParseDefines(string doc, int beg, int end) =>
            SubstitutionParser.ParseDefines(doc, beg, end, out _);

        [TestMethod]
        public void TestBasic()
        {
            CollectionAssert.AreEqual((List<(string, string)>)[
                ("a", "5")
            ], AtLine(ParseDefines,
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
            ], AtLine(ParseDefines,
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
            ], AtLine(ParseDefines, 
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
            ], AtLine(ParseDefines,
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
            ], AtLine(ParseDefines,
                "a with a = 3",
                0
            ));
        }

        [TestMethod]
        public void TestLetAndWith()
        {
            CollectionAssert.AreEqual(((string, string)[])[
                ("x", "9"),
                ("c", "1"),
                ("b", "5"),
                ("a", "3"),
            ], AtLine(ParseDefines,
                "let a = 3, b = 5, c = 1\r" +
                "x with x = 9",
                1
            ));
        }
    }

    [TestClass]
    public class ParseTests
    {
        private void AssertSubstitutiblesAreEqual(IEnumerable<ISubstitutible> expect, IEnumerable<ISubstitutible> actual)
        {
            Assert.AreEqual(expect.Count(), actual.Count(), "Number of elements");
            foreach (var (e, a) in expect.Zip(actual, Tuple.Create))
            {
                if (e is Variable ve && a is Variable va)
                {
                    Assert.AreEqual(ve.name, va.name, "Variable name");
                    Assert.AreEqual(ve.value, va.value, "Variable value");
                }
                else if (e is Formula fe && a is Formula fa)
                {
                    Assert.AreEqual(fe.name, fa.name, "Formula name");
                    CollectionAssert.AreEqual(fe.parameterNames, fa.parameterNames, "Formula parameter names");
                    Assert.AreEqual(fe.definition.str, fa.definition.str, "Formula definition string");
                }
                else if (e is MappedFormula me && a is MappedFormula ma)
                {
                    Assert.AreEqual(me.name, ma.name, "MappedFormula name");
                    CollectionAssert.AreEqual(me.mapping, ma.mapping, "MappedFormula mapping");
                }
                else if (e is AnonymousFormula ae && a is AnonymousFormula aa)
                {
                    Assert.AreEqual(ae.parameterNames, aa.parameterNames, "AnonymousFormula parameter names");
                    Assert.AreEqual(ae.definition.str, aa.definition.str, "AnonymousFormula definition string");
                }
                else
                {
                    Assert.Fail("Type mismatch");
                }
            }
        }

        private static List<ISubstitutible> Parse(string doc, int beg, int end) =>
            SubstitutionParser.Parse(doc, beg, end, out _);

        [TestMethod]
        public void TestVariable()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Variable.TryDefine("a", "3"),
            ], AtLine(Parse,
                "let a = 3\r",
                1
            ));
        }

        [TestMethod]
        public void TestFormula()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Formula.TryDefine("f(a,b)", "3a+b"),
            ], AtLine(Parse,
                "let f(a,b) = 3a+b\r",
                1
            ));
        }

        [TestMethod]
        public void TestMappedFormula()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                MappedFormula.TryDefine("f(x)", "{2->3,5->9}"),
            ], AtLine(Parse,
                "let f(x) = {2->3,5->9}\r",
                1
            ));
        }

        [TestMethod]
        public void TestCombined()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                MappedFormula.TryDefine("g(x)", "{2->3,5->9}"),
                Formula.TryDefine("f(a,b)", "3a+b"),
                Variable.TryDefine("a", "3"),
            ], AtLine(Parse,
                "let a = 3\r" +
                "let f(a,b) = 3a+b\r" +
                "let g(x) = {2->3,5->9}\r",
                3
            ));
        }

        [TestMethod]
        public void TestLetStatementShadowing()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Variable.TryDefine("a", "5"),
            ], AtLine(Parse,
                "let a = 3\r" +
                "let a = 5\r",
                2
            ));
        }

        [TestMethod]
        public void TestLetClauseShadowing()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Variable.TryDefine("a", "12"),
            ], AtLine(Parse,
                "let a = 3, a = 12\r",
                1
            ));
        }

        [TestMethod]
        public void TestWithStatementLetShadowing()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Variable.TryDefine("a", "7"),
            ], AtLine(Parse,
                "let a = 1\r" +
                "a with a = 7",
                1
            ));
        }

        [TestMethod]
        public void TestWithClauseShadowing()
        {
            AssertSubstitutiblesAreEqual((ISubstitutible[])[
                Variable.TryDefine("a", "9"),
            ], AtLine(Parse,
                "a with a = 7, a = 9",
                0
            ));
        }
    }

    [TestClass]
    public class SubstitutorTests
    {
        private static string Substitute(string doc, int beg, int end) => new Substitutor(doc, beg, end, out string expr).Substitute(expr);

        [TestMethod]
        public void TestVariable()
        {
            Assert.AreEqual(
                "3",
            AtLine(Substitute,
                "let a = 3\r" +
                "a",
                1
            ));
        }

        [TestMethod]
        public void TestFormula()
        {
            Assert.AreEqual(
                "3(6)+2(7)+5",
            AtLine(Substitute,
                "let f(a,b) = 3a+2b+5\r" +
                "f(6,7)",
                1
            ));
        }

        [TestMethod]
        public void TestFormulaRecursion()
        {
            Assert.AreEqual(
                "2(x+4)",
            AtLine(Substitute,
                "let f(x) = 2x, g(x) = x+4\r" +
                "f(g(x))",
                1
            ));
        }

        [TestMethod]
        public void TestFormulaRecursionNotInfinite()
        {
            Assert.IsTrue(
                new Regex(@"(?:f\()+x\)+").IsMatch(
            AtLine(Substitute,
                "let f(x) = f(f(x))\r" +
                "f(x)",
                1
            )));
        }

        [TestMethod]
        public void TestMappedFormula()
        {
            Assert.AreEqual(
                "(b) and (a)",
            AtLine(Substitute,
                "let f(x) = {5->a,6->b}\r" +
                "f(6) and f(5)",
                1
            ));
        }

        [TestMethod]
        public void TestMappedFormulaRecursion()
        {
            Assert.AreEqual(
                "9",
            AtLine(Substitute,
                "let f(x) = {1->7,7->9}\r" +
                "f(f(1))",
                1
            ));
        }
    }
}
