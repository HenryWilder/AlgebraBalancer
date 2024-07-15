using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Core;
using Windows.System;
using System.Collections.ObjectModel;
using AlgebraBalancer.Algebra.Balancer;
using AlgebraBalancer.Substitute;
using AlgebraBalancer.Algebra;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer;

public sealed partial class MainPage : Page
{
    private readonly ObservableCollection<string> UnicodeButtonSymbols = [];

    public MainPage()
    {
        InitializeComponent();

        // Math buttons
        string[] headers = ["A", "B", "C"];
        foreach (string header in headers)
        {
            var item = new AlgebraInput(header);
            Inputs.Children.Add(item);
            item.PreviewKeyDown += (object sender, KeyRoutedEventArgs e) =>
            {
                if (e.Key == VirtualKey.Enter)
                {
                    UpdateAsync();
                    e.Handled = true;
                }
            };
        }

        UnicodeButtons.ItemsSource = UnicodeButtonSymbols;
    }

    private void UpdateCalculations(object sender, RoutedEventArgs args)
    {
        UpdateAsync();
    }

    private async void UpdateAsync()
    {
        CalcBtn.IsEnabled = false;
        Output.Blocks.Clear();
        (OutputProgress.IsActive, OutputProgress.Visibility) = (true, Visibility.Visible);

        List<(string name, string value)> calculations = [];

        List<int> parameters;
        try
        {
            parameters = Inputs.Children
                .Select((input) => (input as AlgebraInput).GetValue())
                .Where((int? value) => value.HasValue)
                .Select((int? value) => value.Value)
                .ToList();
        }
        catch
        {
            parameters = null;
        }

        if (parameters is not null)
        {
            var task = Task.Run(() => calculations = ExactCalculations.Calculations(parameters));
            if (await Task.WhenAny(task, Task.Delay(5000)) != task)
            {
                calculations = [(null, "Calculation timed out")];
            }
        }
        else
        {
            calculations = [(null, "...")];
        }

        (OutputProgress.IsActive, OutputProgress.Visibility) = (false, Visibility.Collapsed);

        foreach ((string name, string value) in calculations)
        {
            var para = new Paragraph();
            if (!string.IsNullOrEmpty(name)) para.Inlines.Add(new Run { Text = name, FontWeight = FontWeights.Bold });
            para.Inlines.Add(new Run { Text = value });
            para.Margin = new Thickness(5);
            Output.Blocks.Add(para);
        }

        CalcBtn.IsEnabled = true;
    }

    private void TogglePane_Click(object sender, RoutedEventArgs e)
    {
        bool isBecomingInline = CalculationsPane.DisplayMode == SplitViewDisplayMode.Overlay;
        CalculationsPane.DisplayMode  = isBecomingInline ? SplitViewDisplayMode.Inline : SplitViewDisplayMode.Overlay;
        CalculatorPane_PinIcon.Symbol = isBecomingInline ? Symbol.UnPin : Symbol.Pin;
    }

    private void ShowPane_Click(object sender, RoutedEventArgs e)
    {
        CalculationsPane.IsPaneOpen = !CalculationsPane.IsPaneOpen;
    }

    private string UnicodeReplacements(string str)
    {
        string macroPass = LatexUnicode.ApplyUnicodeReplacements(str);
        string remapPass = LatexUnicode.ApplyRemapPatterns(macroPass);
        string alignPass = MathAlign.AlignMath(remapPass);
        return alignPass;
    }

    private void Notes_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        int selectionStart = Notes.SelectionStart;
        string updated = Notes.Text.Insert(selectionStart, MathAlign.CURSOR_SAVER);
        updated = UnicodeReplacements(updated);
        int cursorPos = updated.IndexOf(MathAlign.CURSOR_SAVER);
        Notes.Text = updated.Replace(MathAlign.CURSOR_SAVER, "");
        Notes.SelectionStart = cursorPos;
    }

    private static readonly Regex rxWS = new(@"[ \t\n\r]+");
    private readonly static Regex rxSquare = new(@"(\d+\.?\d*)([²³⁴])");

    private static readonly string HELP_TEXT = @"
!! The commands have been modified so that they display without executing.
   Make sure to type the commands yourself instead of copying and pasting them from this, or else they won't work.

# Notes
  [alt] + [enter]              => Toggle calculator pane
  [ctrl] + [alt] + [left]      => Jump left one column
  [ctrl] + [alt] + [right]     => Jump right one column
  ([alt] or [shift]) + [enter] => Duplicate line down
  [alt] + [shift] + [down]     => Duplicate line down
  [alt] + [shift] + [up]       => Duplicate line up
  [ctrl] + (1, 2, or 3)        => Insert selection into the corresponding calculator input
  [ctrl] + [enter]             => Clear calculator inputs and replace 1st with selection
  [ctrl] + [space]             => Calculate approximate value of the selection inline

  \SomeLaTeXCommand\ => Unicode equivalent of corresponding LaTeX command

  \matrix~<rows>x<cols> => Create a matrix with <rows> rows and <cols> columns (between 1 and 9)
    Example: \matrix~3x2
\matrix3x2

  \det~<rows>x<cols> => Create a determinant with <rows> rows and <cols> columns (between 1 and 9)
    Example: \det~3x2
\det3x2

  \cases~<cases> => Create a piecewise with <cases> cases (between 1 and 9)
    Example: \cases~3
\cases3

  \rcases~<cases> => Create a reverse piecewise with <cases> cases (between 1 and 9)
    Example: \rcases~3
\rcases3

  ^... or ^{...} => Superscript (^{0-9})
  _... or _{...} => Subscript (_{0-9})

  $...  => Blackboard bold
  \...\ => Math variable

  @~@  => Circ (""@@"")
  @~0  => Degrees (""@0"")
  @~*  => Times (""@*"")
  @~.  => Cdot (""@."")
  @~/  => Div (""@/"")
  @~-  => Intersection (""@-"")
  @~+  => Union (""@+"")
  @~2  => Square root (""@2"")
  √~³  => Cube root (""@2^3"")
  √~⁴  => Cube root (""@2^4"")
  @~8  => Infinity (""@8"")
  @~6  => Partial derivative (""@6"")
  @~A  => Forall (""@A"")
  @~E  => Exists (""@E"")
  @~v0 => Varnothing (""@v0"")
  @~I  => Integral (""@I"")

  (), [], and {} are created in pairs and surround the selection.
  If the brackets are empty, backspacing the opening bracket deletes the closing bracket.
  ), ], and } can overtype each other (to make interval notation easier).

  Use ""~&"" to separate columns.
  Columns are aligned in an alternating pattern of right, left, right, left, etc.
    --> ~& <-- ~& --> ~& <--

  Use ""~&~&"" to keep the same alignment direction in the new column.
    --> ~&~& --> ~&~& --> ~&~& --> ~& <-- ~&~& <-- ~&~& <-- ~&~& <--

  Alignments are localized to the current ""chunk"". Chunks are separated by lines that have no ""~&""s.
    apple ~& banana ~& orange ~& mango
      000 ~& 000    ~&    000 ~& 000
    blah blah blah blah
    0000 ~& 0000 ~&~& 0000
       0 ~& 0    ~&~& 0

  \~& => Non-aligning ampersand (""\&"")

# Calculator
  Press enter to calculate.

# Symbol lookup
  Press enter to search.
  Press escape to clear the search.
  Click a symbol to insert it in the notes section.
".Replace("~", "\u200B");

    /// <summary>
    /// Looks within a multiline string and finds the "line" containing the provided position.
    /// A line is defined as the range between a combination of any two of the following, with the shortest length possible:
    /// <ul>
    ///   <li> The start of the string. </li>
    ///   <li> A '\r' character. </li>
    ///   <li> A second '\r' character. </li>
    ///   <li> The end of the string. </li>
    /// </ul>
    /// In other words: <code>@"(?&lt;=^|\r).*?(?=\r|$)"</code>
    /// The range returned will not contain the '\r' character(s) if any are used to find the boundaries of the line.
    /// </summary>
    /// <param name="text">The full text to search within.</param>
    /// <param name="index">The index to search for.</param>
    /// <returns>
    /// The range of the two '\r' characters (or start or end) closest to <paramref name="index"/> in either direction.
    /// (-1,-1) if <paramref name="index"/> is out of bounds.
    /// </returns>
    public static (int lineStart, int lineEnd) GetLineContainingPosition(in string text, int index)
    {
        const string LINE_SPLIT = "\r";
        string[] lines = text.Split(LINE_SPLIT);
        int lineStartIndex = 0;
        foreach (string line in lines)
        {
            int lineEndIndex = lineStartIndex + line.Length;
            if (index <= lineEndIndex)
            {
                return (lineStartIndex, lineEndIndex);
            }

            lineStartIndex = lineEndIndex + LINE_SPLIT.Length;
        }

        return (-1, -1);
    }

    public static void ColumnJump(
        bool isLeft,
        in string notesText,
        int selectionStart,
        out int selectionStartFinal,
        out int selectionLengthFinal
    )
    {
        var (startOfLine, endOfLine) = GetLineContainingPosition(notesText, selectionStart);

        if (isLeft && selectionStart != 0)
        {
            if (selectionStart == startOfLine)
            {
                selectionStartFinal = startOfLine - 1;
            }
            else
            {
                int prevCol = notesText.LastIndexOf('&', Math.Clamp(selectionStart - 1, 0, notesText.Length - 1));
                if (prevCol == -1) prevCol = 0;

                selectionStartFinal = Math.Max(prevCol, startOfLine);
            }
        }
        else if (!isLeft && selectionStart != notesText.Length)
        {
            if (selectionStart == endOfLine)
            {
                selectionStartFinal = endOfLine + 1;
            }
            else
            {
                int nextCol = notesText.IndexOf('&', Math.Clamp(selectionStart + 1, 0, notesText.Length - 1));
                if (nextCol == -1) nextCol = notesText.Length;

                selectionStartFinal = Math.Min(nextCol, endOfLine);
            }
        }
        else
        {
            selectionStartFinal = selectionStart;
        }

        selectionLengthFinal = 0; // Todo: shift-select
    }

    public static void DuplicateLine(
        bool isUpward,
        int selectionStart,
        in string notesText,
        out int selectionStartFinal,
        out string notesTextFinal
    )
    {
        var (startOfLine, endOfLine) = GetLineContainingPosition(notesText, selectionStart);

        string lineText = notesText.Substring(startOfLine, endOfLine - startOfLine).Insert(0, "\r");
        int cursorOffset = isUpward ? 0 : lineText.Length;

        notesTextFinal = notesText.Insert(endOfLine, lineText);
        selectionStartFinal = selectionStart + cursorOffset;
    }

    public static void CalculateInline(
        string expr,
        int selectionStart,
        int selectionLength,
        in string notesText,
        out int selectionStartFinal,
        out string notesTextFinal
    )
    {
        bool isApproximationError;
        string resultApproximate;
        try
        {
            isApproximationError = !Solver.TrySolveDouble(expr, out string result);
            resultApproximate = isApproximationError ? $"<{result}>" : result;
        }
        catch (Exception err)
        {
            isApproximationError = true;
            resultApproximate = $"<{err.Message}>";
        }

        bool isAlgebraicError;
        string resultAlgebraic;
        try
        {
            var letter = new Regex(@"\p{L}").Match(expr);
            if (letter.Success)
            {
                if (AlgSolver.TrySolvePolynomialDivision(expr, out _, out var denom, out var quotient, out var remainder))
                {
                    resultAlgebraic =
                        (quotient.ToString() +
                        ((remainder is Number n && n == 0) ? "" : $", {remainder}") +
                        $" => ({denom})({quotient})+{remainder}")
                        .Replace("+-", "-");
                }
                else if (AlgSolver.TryFOILPolynomials(expr, out var foiled))
                {
                    resultAlgebraic = foiled.ToString();
                }
                else if (AlgSolver.TrySimplifyPolynomial(expr, out var simplified))
                {
                    resultAlgebraic = simplified.ToString();
                }
                else
                {
                    throw new Exception($"Cannot use variable '{letter.Value}' except in polynomial");
                }
            }
            else
            {
                resultAlgebraic = AlgSolver.SolveAlgebraic(expr).Simplified().ToString();
                if (resultAlgebraic.Contains("𝑖"))
                {
                    if (expr.Contains("ⅈ"))
                    {
                        resultAlgebraic = resultAlgebraic.Replace("𝑖", "ⅈ");
                    }
                    else if (!expr.Contains("𝑖"))
                    {
                        resultAlgebraic = resultAlgebraic.Replace("𝑖", "i");
                    }
                }
            }
            isAlgebraicError = false;
        }
        catch (Exception err)
        {
            isAlgebraicError = true;
            resultAlgebraic = $"<{err.Message}>";
        }

        string addText;
        if (resultApproximate == resultAlgebraic)
        {
            addText = $" = {resultAlgebraic}";
        }
        else if (isApproximationError != isAlgebraicError)
        {
            addText = !isAlgebraicError
                ? $" = {resultAlgebraic}"
                : $" ≈ {resultApproximate}";
        }
        else
        {
            addText = $" = {resultAlgebraic} ≈ {resultApproximate}";
        }

        int insertAt = selectionStart + selectionLength;
        int insertEnd = insertAt + addText.Length;
        notesTextFinal = notesText.Insert(insertAt, addText);
        selectionStartFinal = insertEnd;
    }

    public static void SubstituteVars(
        int selectionStart,
        in string notesText,
        out int selectionStartFinal,
        out string notesTextFinal
    )
    {
        var (startOfLine, endOfLine) = GetLineContainingPosition(notesText, selectionStart);
        var substitutor = new Substitutor(notesText, startOfLine, endOfLine, out string expr);
        string newExpr = "\r" + substitutor.Substitute(expr);
        selectionStartFinal = endOfLine + newExpr.Length;
        notesTextFinal = notesText.Insert(endOfLine, newExpr);
    }

    private static readonly Regex rxOperator = new(@"^\s*([-+/*])\s*(.*)\s*$");

    // TODO: Currently broken
    public static void BalanceAlgebra(
        int selectionStart,
        in string notesText,
        out int selectionStartFinal,
        out string notesTextFinal
    )
    {
        var (startOfLine, endOfLine) = GetLineContainingPosition(notesText, selectionStart);
        string lineText = notesText.Substring(startOfLine, endOfLine - startOfLine);

        string[] args = lineText.Split(@"\\");
        var rel = Relationship.Parse(args[0]);
        var match = rxOperator.Match(args[1]);
        if (match.Success)
        {
            var op = match.Groups[1].Value switch
            {
                "+" => Operation.Add,
                "-" => Operation.Sub,
                "*" => Operation.Mul,
                "/" => Operation.Div,
                _ => throw new NotImplementedException(),
            };
            rel.ApplyOperation(op, match.Groups[2].Value);
            string refactor = rel.ToString();
            notesTextFinal = notesText
                .Remove(startOfLine, endOfLine - startOfLine)
                .Insert(startOfLine, refactor);
            selectionStartFinal = startOfLine + refactor.Length;
            return;
        }

        selectionStartFinal = selectionStart;
        notesTextFinal = notesText;
    }

    private void Notes_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
        bool isShifting = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
        bool isCtrling = (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        var altState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);
        bool isAlting = (altState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        // Help
        if (e.Key == VirtualKey.F1)
        {
            Notes.Text += HELP_TEXT;
            e.Handled = true;
        }
        // Toggle calculator
        else if (e.Key == VirtualKey.Enter && isAlting)
        {
            CalculationsPane.IsPaneOpen = !CalculationsPane.IsPaneOpen;
            e.Handled = true;
        }
        // Column jumping
        else if ((e.Key is VirtualKey.Left or VirtualKey.Right) && isAlting && isCtrling)
        {
            ColumnJump(
                e.Key == VirtualKey.Left,
                Notes.Text,
                Notes.SelectionStart,
                out int newSelectionStart,
                out int newSelectionLength
            );
            Notes.SelectionStart = newSelectionStart;
            Notes.SelectionLength = newSelectionLength;
            e.Handled = true;
        }
        // Duplicate line
        else if (!string.IsNullOrWhiteSpace(Notes.Text) && ((e.Key is VirtualKey.Down or VirtualKey.Up) && isShifting && isAlting
            || (e.Key == VirtualKey.Enter && isShifting)))
        {
            DuplicateLine(
                e.Key == VirtualKey.Up,
                Notes.SelectionStart,
                Notes.Text,
                out int newSelectionStart,
                out string newNotesText
            );
            Notes.Text = newNotesText;
            Notes.SelectionStart = newSelectionStart;
            e.Handled = true;
        }
        // Put selection in math input
        else if (isCtrling && (e.Key is VirtualKey.Number1 or VirtualKey.Number2 or VirtualKey.Number3))
        {
            int index = e.Key - VirtualKey.Number1;
            (Inputs.Children[index] as AlgebraInput).SetValue(Notes.SelectedText);
            CalculationsPane.IsPaneOpen = true;
            UpdateAsync();
            e.Handled = true;
        }
        // Calculate just the selection
        else if (isCtrling && e.Key == VirtualKey.Enter)
        {
            (Inputs.Children[0] as AlgebraInput).SetValue(rxWS.Replace(Notes.SelectedText, ""));
            (Inputs.Children[1] as AlgebraInput).SetValue("");
            (Inputs.Children[2] as AlgebraInput).SetValue("");
            CalculationsPane.IsPaneOpen = true;
            UpdateAsync();
            e.Handled = true;
        }
        // Calculate the selection and insert the result on the right side of an equals sign
        else if (isCtrling && e.Key == VirtualKey.Space)
        {
            int newSelectionStart;
            string newNotesText;
            // Calculate approximate value
            if (Notes.SelectionLength > 0)
            {
                CalculateInline(
                    Notes.SelectedText,
                    Notes.SelectionStart,
                    Notes.SelectionLength,
                    Notes.Text,
                    out newSelectionStart,
                    out newNotesText
                );
            }
            // Substitution
            else
            {
                SubstituteVars(
                    Notes.SelectionStart,
                    Notes.Text,
                    out newSelectionStart,
                    out newNotesText
                );
            }
            Notes.Text = newNotesText;
            Notes.SelectionStart = newSelectionStart;
            e.Handled = true;
        }
        // Insert 4 spaces when tab is pressed
        else if (e.Key == VirtualKey.Tab)
        {
            int selectionStart = Notes.SelectionStart;
            int selectionLength = Notes.SelectionLength;
            if (isShifting)
            {
                int lineStart = 
                    Math.Max(Notes.Text.Substring(0, selectionStart).LastIndexOf('\r'), 0);
                int erasing = 0;
                for (; erasing < 4
                    && lineStart + erasing < Notes.Text.Length
                    && Notes.Text[lineStart + erasing] == ' ';
                    ++erasing) { }
                if (erasing > 0)
                {
                    Notes.Text = Notes.Text.Remove(lineStart, erasing);
                    Notes.SelectionStart = selectionStart - erasing;
                    Notes.SelectionLength = selectionLength;
                }
            }
            else
            {
                Notes.Text = Notes.Text.Insert(selectionStart, "    ");
                Notes.SelectionStart = selectionStart + 4;
                Notes.SelectionLength = selectionLength;
            }
            e.Handled = true;
        }
        // Create brackets in pairs and surround the selection with them
        else if ((e.Key == VirtualKey.Number9 && isShifting) || e.Key == ((VirtualKey)0xDB))
        {
            string open, close;
            if (e.Key == VirtualKey.Number9 && isShifting) (open, close) = ("(", ")");
            else if (e.Key == ((VirtualKey)0xDB)) (open, close) = isShifting ? ("{", "}") : ("[", "]");
            else throw new Exception("Unreachable");
            string text = Notes.Text;
            text = text
                .Insert(Notes.SelectionStart + Notes.SelectionLength, close)
                .Insert(Notes.SelectionStart, open);
            int start = Notes.SelectionStart + 1;
            int length = Notes.SelectionLength;
            Notes.Text = text;
            Notes.SelectionStart = start;
            Notes.SelectionLength = length;
            e.Handled = true;
        }
        // Overwrite bracket pair closer (overwriting paren with brack, etc. IS intended)
        else if ((e.Key == VirtualKey.Number0 && isShifting) || e.Key == ((VirtualKey)0xDD))
        {
            int selectionPosition = Notes.SelectionStart;
            if (selectionPosition < Notes.Text.Length && Notes.Text[selectionPosition] is ')' or ']' or '}')
            {
                string replacement;
                if (e.Key == VirtualKey.Number0 && isShifting) replacement = ")";
                else if (e.Key == ((VirtualKey)0xDD)) replacement = isShifting ? "}" : "]";
                else throw new Exception("Unreachable");
                Notes.Text = Notes.Text.Remove(selectionPosition, 1).Insert(selectionPosition, replacement);
                Notes.SelectionStart = selectionPosition + 1;
                e.Handled = true;
            }
        }
        // Delete bracket pair together
        else if (e.Key == VirtualKey.Back)
        {
            int newSelectPosition = Math.Max(Notes.SelectionStart - 1, 0);
            if (Notes.SelectionLength == 0 && Notes.Text.Length >= newSelectPosition + 2 &&
                Notes.Text.Substring(newSelectPosition, 2) is "()" or "[]" or "{}" or "<>" or "[)" or "(]")
            {
                Notes.Text = Notes.Text.Remove(newSelectPosition, 2);
                Notes.SelectionStart = newSelectPosition;
                e.Handled = true;
            }
        }
    }

    private const string NO_RESULTS_MSG = "No results";
    private const string TOO_MANY_RESULTS_MSG = "Too many results to display";

    private void UnicodeLookup_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            string searchStr = UnicodeLookup.Text;
            UnicodeLookup.Text = "";

            UnicodeButtonSymbols.Clear();
            if (!string.IsNullOrWhiteSpace(searchStr))
            {
                Regex search = new(
                    string.Join("(?:", searchStr.ToCharArray()) + string.Concat(searchStr.Skip(1).Select(_ => ")?")),
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var filteredItems = LatexUnicode.unicodeReplacements
                    .Select((kvp) => (kvp, search.Match(kvp.Key)))
                    .OrderByDescending(x => x.Item2.Length)
                    .Take(128)
                    .Select((x) => x.kvp.Key/* + "\n" + x.kvp.Value*/);

                if (filteredItems.Count() == 0)
                {
                    UnicodeButtonSymbols.Add(NO_RESULTS_MSG);
                }
                else
                {
                    foreach (string item in filteredItems)
                    {
                        UnicodeButtonSymbols.Add(item);
                    }
                }
            }
        }
        else if (e.Key == VirtualKey.Escape)
        {
            UnicodeLookup.Text = "";
            UnicodeButtonSymbols.Clear();
        }
    }

    private void UnicodeButtons_ItemClick(object sender, ItemClickEventArgs e)
    {
        string symbol = e.ClickedItem as string;
        if (symbol is TOO_MANY_RESULTS_MSG or NO_RESULTS_MSG)
        {
            UnicodeButtonSymbols.Clear();
            UnicodeButtonSymbols.Add(":P");
        }
        else
        {
            string insertion = symbol == ":P" ? "👬" : symbol/*.Split('\n')[1]*/;
            int position = Notes.SelectionStart;
            Notes.Text = Notes.Text.Insert(position, insertion);
            _ = Notes.Focus(FocusState.Programmatic);
            Notes.SelectionStart = position + insertion.Length;
        }
    }
}
