using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlgebraBalancerUnitTests;
internal static class TestHelper
{
    /// <summary>
    /// String constant designating
    /// <see cref="Windows.UI.Xaml.Controls.TextBox.SelectionStart"/>
    /// position in test string before operation
    /// </summary>
    public const string SEL_BEG_IN = "[[SELECTION_BEGIN_INITIAL]]";

    /// <summary>
    /// String constant designating
    /// (<see cref="Windows.UI.Xaml.Controls.TextBox.SelectionStart"/> +
    /// <see cref="Windows.UI.Xaml.Controls.TextBox.SelectionLength"/>)
    /// position in test string before operation
    /// </summary>
    public const string SEL_END_IN = "[[SELECTION_END_INITIAL]]";

    /// <summary>
    /// String constant designating
    /// <see cref="Windows.UI.Xaml.Controls.TextBox.SelectionStart"/>
    /// position in test string after operation
    /// </summary>
    public const string SEL_BEG_EX = "[[SELECTION_BEGIN_EXPECT]]";

    /// <summary>
    /// String constant designating
    /// (<see cref="Windows.UI.Xaml.Controls.TextBox.SelectionStart"/> +
    /// <see cref="Windows.UI.Xaml.Controls.TextBox.SelectionLength"/>)
    /// position in test string after operation
    /// </summary>
    public const string SEL_END_EX = "[[SELECTION_END_EXPECT]]";

    public static void GetTextSelectionPositions(
        ref string text,
        out int selectionStart,
        out int expectStart,
        out int selectionLength,
        out int expectLength
    )
    {
        selectionStart =
            text.Replace(SEL_END_IN, "")
                .Replace(SEL_BEG_EX, "")
                .Replace(SEL_END_EX, "")
                .IndexOf(SEL_BEG_IN);

        selectionLength = Math.Max(-1,
            text.Replace(SEL_BEG_IN, "")
                .Replace(SEL_BEG_EX, "")
                .Replace(SEL_END_EX, "")
                .IndexOf(SEL_END_IN) - selectionStart);

        expectStart =
            text.Replace(SEL_BEG_IN, "")
                .Replace(SEL_END_IN, "")
                .Replace(SEL_END_EX, "")
                .IndexOf(SEL_BEG_EX);

        expectLength = Math.Max(-1,
            text.Replace(SEL_BEG_IN, "")
                .Replace(SEL_END_IN, "")
                .Replace(SEL_BEG_EX, "")
                .IndexOf(SEL_END_EX) - expectStart);

        text =
            text.Replace(SEL_BEG_IN, "")
                .Replace(SEL_END_IN, "")
                .Replace(SEL_BEG_EX, "")
                .Replace(SEL_END_EX, "");
    }

    public static void GetModifiedTextSelectionPositions(
        ref string notesText,
        ref string expectNotesText,
        out int selectionStart,
        out int expectStart,
        out int selectionLength,
        out int expectLength
    )
    {
        GetTextSelectionPositions(ref notesText, out int actBeg, out _, out int actLen, out _);
        selectionStart = actBeg;
        selectionLength = actLen;
        GetTextSelectionPositions(ref expectNotesText, out _, out int expBeg, out _, out int expLen);
        expectStart = expBeg;
        expectLength = expLen;
    }

    /// <summary>
    /// Asserts that <see cref="SEL_BEG_EX"/> and <see cref="SEL_END_EX"/> are where they should be,
    /// based on the position of <see cref="SEL_BEG_IN"/>.
    /// </summary>
    public static void AssertPredictedLineRange(string notesText)
    {
        // Does not need to be exhaustive
        static string Sanatize(string str)
        {
            return "\"" + str
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\"", @"\""")
            + "\"";
        }

        GetTextSelectionPositions(ref notesText, out int selectionStart, out int expectStart, out _, out int expectLength);
        var (lineStart, lineEnd) = AlgebraBalancer.MainPage.GetLineContainingPosition(notesText, selectionStart);
        Assert.AreEqual(
            (
                Sanatize(notesText.Substring(expectStart, expectLength)),
                $"{expectStart}:{expectStart + expectLength}"
            ),
            (
                Sanatize(notesText.Substring(lineStart, lineEnd - lineStart)),
                $"{lineStart}:{lineEnd}"
            ));
    }

    public static (int lineStart, int lineEnd) GetLineRange(in string document, int lineNumber)
    {
        const string LINE_SPLIT = "\r";
        string[] lines = document.Split(LINE_SPLIT);
        int lineStart = 0;
        int lineEnd = lines[0].Length;
        for (int i = 0; i < lineNumber; ++i)
        {
            lineStart = lineEnd + LINE_SPLIT.Length;
            lineEnd = lineStart + lines[i].Length;
        }
        return (lineStart, lineEnd);
    }
}
