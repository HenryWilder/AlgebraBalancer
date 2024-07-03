using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.UI.Xaml.Shapes;

namespace AlgebraBalancer;
internal static class MathAlign
{
    public const string CURSOR_SAVER = "\f";
    
    private static readonly Regex rxColumnSplit = new(@"(?<!\u200B)&");
    //private static readonly Regex rxNoalign = new(@"^\\noalign\b");
    private static readonly Regex rxAlignAroundCursor = new($@"^ *{CURSOR_SAVER} *$");
    private static readonly Regex rxMultipleSpaces = new(@" +");
    private static readonly Regex rxSpacesBeforeAmp = new(@" +&");
    private static readonly Regex rxOnlySpaces = new(@"^ +$");
    private static readonly Regex rxLeadingSpaces = new(@"^ +"); // TODO: Check if this can just be TrimLeft or somethin

    private static string MinimizePadding(string part)
    {
        part = rxAlignAroundCursor.Replace(part, CURSOR_SAVER);
        part = rxMultipleSpaces.Replace(part, " ");
        part = rxOnlySpaces.Replace(part, "");
        return part;
    }

    private class AlignChunk(bool needsAlignment)
    {
        public List<string> rows = [];
        public bool needsAlignment = needsAlignment;

        public void Align()
        {
            if (!needsAlignment) return;

            // Split each row into columns
            var table = rows.Select((row) => rxColumnSplit.Split(row));

            // Find the widest each column ever gets
            var alignments = new List<int>();
            foreach (string[] row in table)
            {
                int numColumns = row.Count();
                while (alignments.Count() < numColumns - 1)
                {
                    alignments.Add(-1);
                }

                for (int i = 0; i < numColumns - 1; ++i)
                {
                    string column = MinimizePadding(row[i]).Replace(CURSOR_SAVER, "");

                    if (i == 0) column = rxLeadingSpaces.Replace(column, "");

                    int colWidth = column.Length;
                    alignments[i] = System.Math.Max(alignments[i], colWidth);
                }
            }

            // Apply the appropriate padding
            rows = table.Select((line, i) =>
                string.Join('&', line.Select((part, j) => {
                    if (j < line.Count() - 1 && j < alignments.Count())
                    {
                        int accountForCursor = part.Contains(CURSOR_SAVER) ? CURSOR_SAVER.Length : 0;
                        int partWidth = alignments[j] + accountForCursor;
                        part = MinimizePadding(part);

                        if (j == 0)
                        {
                            part = rxLeadingSpaces.Replace(part, "");
                        }

                        part = (j & 1) == 0
                            ? part.PadLeft(partWidth)
                            : part.PadRight(partWidth);
                    }
                    return part;
                }))
            ).ToList();
        }
    }

    private static List<AlignChunk> GetChunks(List<string> lines)
    {
        List<AlignChunk> chunks = [];

        foreach (string line in lines)
        {
            bool isAlignedLine = line.Contains('&');
            if (chunks.Count == 0 || isAlignedLine != chunks.Last().needsAlignment)
            {
                chunks.Add(new AlignChunk(isAlignedLine));
            }

            chunks.Last().rows.Add(line);
        }

        return chunks;
    }

    public static string AlignMath(string str)
    {
        var lines = str.Split("\r")
            .Select((line) => rxSpacesBeforeAmp.Replace(line, " &"))
            .ToList();

        var chunks = GetChunks(lines);
        foreach (var chunk in chunks) chunk.Align();

        return string.Join("\r", chunks.SelectMany((chunk) => chunk.rows));
    }
}
