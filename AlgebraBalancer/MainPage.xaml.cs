﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static AlgebraBalancer.Algebra;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Core;
using Windows.System;
using System.Data;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Shapes;

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

    private static string Calculations(int x)
    {
        string result = "";

        // Categorization
        {
            string posOrNeg = x > 0 ? "⁺" : x < 0 ? "⁻" : "";
            if (x > 0 && IsPrime(x))
            {
                result += $"{x}  ∈ ℙ";
            }
            else
            {
                string oddOrEven = IsOdd(x) ? "+1" : "";
                result += $"{x}  ∈ 2ℤ{posOrNeg}{oddOrEven}";
            }
        }

        // Prime Factors
        result += $"\n{x}  = ";
        var pfac = PrimeFactors(x);
        result += string.Join(" × ", pfac
            .Select(p =>
                p.prime.ToString() +
                (p.exponent == 1 ? "" : LatexUnicode.ToSuperscript(p.exponent.ToString()))
            )
        );

        // Square
        result += $"\n{x}² = {x * x}";

        // Root
        result += $"\n√{x} = ";
        var root = new Radical(x).Simplified();
        if (root is Radical radical && radical.coefficient != 1 && radical.radicand != 1)
        {
            result += $"(√{radical.coefficient * radical.coefficient})√{radical.radicand} = ";
        }
        result += root;

        // Factors
        result += "\nFactors:";
        List<(string a, string b, string sum, string diff)>
            factorStrings = Factors(x)
                .Select((f) => ($"{f.common}", $"{f.associated}",
                    $"{f.common + f.associated}", $"{f.associated - f.common}"))
                .ToList();

        int aPad = factorStrings.Max((f) => f.a.Length);
        int bPad = factorStrings.Max((f) => f.b.Length);
        //int sumPad = factorStrings.Max((f) => f.sum.Length);
        //int diffPad = factorStrings.Max((f) => f.diff.Length);

        foreach (var (a, b, sum, diff) in factorStrings)
        {
            string aPadded    =    a.PadLeft(   aPad);
            string bPadded    =    b.PadLeft(   bPad);
            //string sumPadded  =  sum.PadLeft( sumPad);
            //string diffPadded = diff.PadLeft(diffPad);

            result += $"\n{aPadded} × {bPadded}";// "; Σ={sumPadded}; Δ={diffPadded}";
        }

        return result;
    }

    private static string Calculations(int a, int b)
    {
        string result = "";

        {
            string inequality = a > b ? ">" : a < b ? "<" : "=";
            result += $"{a} {inequality} {b}";
        }

        result += $"\n{a} + {b} = {a + b}";
        result += $"\n{a} - {b} = {a - b}";
        result += $"\n{a} × {b} = ";
        try { checked { result += a * b; } }
        catch (OverflowException) { result += huge; }
        result += $"\n{a} ÷ {b} = {new Fraction(a, b).Simplified()}";
        result += $"\n{a} % {b} = " + (b != 0 ? new Number(a % b) : undefined);
        result += $"\n{a} ^ {b} = {Power(a, b)}";


        result += $"\nGCF({a}, {b}) = {GCF(a, b)}";
        result += $"\nLCM({a}, {b}) = {LCM(a, b)}";

        // Common factors
        result += "\nCommon Factors:";
        List<(string common, string a, string b)>
            factorStrings = CommonFactors(a, b)
                .Select((f) => ($"{f.common}", $"{f.associated[0]}", $"{f.associated[1]}"))
                .ToList();

        int commonPad = factorStrings.Max((f) => f.common.Length);
        int aPad = factorStrings.Max((f) => f.a.Length);
        int bPad = factorStrings.Max((f) => f.b.Length);

        foreach (var (cFacStr, aFacStr, bFacStr) in factorStrings)
        {
            string commonPadded = cFacStr.PadLeft(commonPad);
            string aPadded = aFacStr.PadLeft(aPad);
            string bPadded = bFacStr.PadLeft(bPad);

            result += $"\n{commonPadded} × ({aPadded}, {bPadded})";
        }

        return result;
    }

    private static string Calculations(int a, int b, int c)
    {
        string result = "";

        var a2 = Power(a, 2);
        var b2 = Power(b, 2);
        var c2 = Power(c, 2);
        if (a2 is Number a2Num && b2 is Number b2Num && c2 is Number c2Num)
        {
            var magnitude = new Radical(a2Num + b2Num + c2Num).Simplified();
            result += $"|A| = {magnitude}";
            {
                IAlgebraicNotation aPart, bPart, cPart;
                if (magnitude is Radical radMag)
                {
                    aPart = new RadicalFraction(a, radMag).Simplified();
                    bPart = new RadicalFraction(b, radMag).Simplified();
                    cPart = new RadicalFraction(c, radMag).Simplified();
                }
                else if (magnitude is Number numMag)
                {
                    aPart = new Fraction(a, numMag).Simplified();
                    bPart = new Fraction(b, numMag).Simplified();
                    cPart = new Fraction(c, numMag).Simplified();
                }
                else if (magnitude is IAlgebraicAtomic atomMag)
                {
                    aPart = new Fraction(new Number(a), atomMag).Simplified();
                    bPart = new Fraction(new Number(b), atomMag).Simplified();
                    cPart = new Fraction(new Number(c), atomMag).Simplified();
                }
                else
                {
                    throw new NotImplementedException();
                }
                result += $"\nÂ = ({aPart}, {bPart}, {cPart})";
            }
        }
        else
        {
            result += $"|A| = ?\nÂ = ({a}, {b}, {c})(?⁻¹)";
        }
        result += $"\nΣ({a}, {b}, {c}) = {a + b + c}";
        result += $"\n∏({a}, {b}, {c}) = {a * b * c}";

        result += $"\nGCF: {GCF(a, b, c)}";

        result += $"\nLCM: {LCM(a, b, c)}";

        // Quadratic
        {
            result += $"\n{a}𝑥² + {b}𝑥 + {c} = 0 => 𝑥 = ";
            result += $"\n  (-({b})±√(({b})²-4({a})({c})))/2({a})";

            var formula = new RadicalFraction(-b, new Radical(b * b - 4 * a * c), 2 * a);

            string unsimplified = $"\n  {formula}";
            result += unsimplified;

            string simplified = $"\n  {formula.Simplified()}";
            if (simplified != unsimplified) { result += simplified; }
        }

        return result;
    }

    private static string Calculations(List<int> parameters) =>
        parameters.Count switch
        {
            1 => Calculations(parameters[0]),
            2 => Calculations(parameters[0], parameters[1]),
            3 => Calculations(parameters[0], parameters[1], parameters[2]),
            _ => "...",
        };

    private void UpdateCalculations(object sender, RoutedEventArgs args)
    {
        UpdateAsync();
    }

    private async void UpdateAsync()
    {
        CalcBtn.IsEnabled = false;
        Output.Text = string.Empty;
        (OutputProgress.IsActive, OutputProgress.Visibility) = (true, Visibility.Visible);

        string calculations = string.Empty;

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
            var task = Task.Run(() => calculations = Calculations(parameters));
            if (await Task.WhenAny(task, Task.Delay(5000)) != task)
            {
                calculations = "Calculation timed out";
            }
        }
        else
        {
            calculations = "...";
        }

        (OutputProgress.IsActive, OutputProgress.Visibility) = (false, Visibility.Collapsed);
        Output.Text = calculations;
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

    private readonly static DataTable dt = new();

    private readonly static Regex rxImpliedMul = new(@"(?<=\))\s*(?=[0-9\(])|(?<=[0-9\)])\s*(?=\()");
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
            int selectionStart = Notes.SelectionStart;
            string notesText = Notes.Text;
            int maxIndex = Math.Max(notesText.Length - 1, 0);

            switch (e.Key)
            {
                case VirtualKey.Left:
                {
                    int selectionIndex = Math.Max(selectionStart - 1, 0);

                    int startOfLine = notesText.LastIndexOf('\r', selectionIndex);
                    if (startOfLine == -1) startOfLine = 0;

                    int prevCol = notesText.LastIndexOf('&', selectionIndex);
                    if (prevCol == -1) prevCol = 0;

                    Notes.SelectionStart = Math.Max(prevCol, startOfLine);
                }
                    break;

                case VirtualKey.Right:
                {
                    int selectionIndex = Math.Min(selectionStart + 1, maxIndex);

                    int endOfLine = notesText.IndexOf('\r', selectionIndex);
                    if (endOfLine == -1) endOfLine = notesText.Length;

                    int nextCol = notesText.IndexOf('&', selectionIndex);
                    if (nextCol == -1) nextCol = notesText.Length;

                    Notes.SelectionStart = Math.Min(nextCol, endOfLine);
                }
                    break;
            }
            Notes.SelectionLength = 0; // Todo: shift-select
            e.Handled = true;
        }
        // Duplicate line
        else if (!string.IsNullOrWhiteSpace(Notes.Text) && ((e.Key is VirtualKey.Down or VirtualKey.Up) && isShifting && isAlting
            || (e.Key == VirtualKey.Enter && (isShifting || isAlting))))
        {
            int selectionStart = Notes.SelectionStart;
            string notesText = Notes.Text;
            int selectionIndex = Math.Min(selectionStart, notesText.Length - 1);

            int startOfLine = notesText.LastIndexOf('\r', selectionIndex - 1);
            if (startOfLine == -1) startOfLine = 0;

            int endOfLine = notesText.IndexOf('\r', selectionIndex);
            if (endOfLine == -1) endOfLine = notesText.Length;

            string lineText = notesText.Substring(startOfLine, endOfLine - startOfLine);
            if (lineText[0] != '\r')
            {
                lineText = lineText.Insert(0, "\r");
            }
            int cursorOffset = e.Key == VirtualKey.Up ? 0 : lineText.Length;

            Notes.Text = notesText.Insert(endOfLine, lineText);
            Notes.SelectionStart = selectionStart + cursorOffset;
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
            if (Notes.SelectionLength > 0)
            {
                string addText;
                try
                {
                    string expr = Notes.SelectedText;
                    expr = rxImpliedMul.Replace(expr, "*");
                    expr = rxSquare.Replace(expr, (Match match) => {
                        string baseValue = match.Groups[1].Value;
                        int expValue = match.Groups[2].Value switch {
                            "²" => 2,
                            "³" => 3,
                            "⁴" => 4,
                            _ => throw new Exception("Not implemented"),
                        };
                        // Weird workaround for Math.pow() not working with DataTable
                        string subexpr = "(" + baseValue;
                        for (int i = 1; i < expValue; ++i)
                        {
                            subexpr += "*" + baseValue;
                        }
                        subexpr += ")";
                        return subexpr;
                    });
                    double value = Convert.ToDouble(dt.Compute(expr, ""));
                    addText = $" = {value}";
                }
                catch (Exception err)
                {
                    addText = $" = <{err.Message}>";
                }
                int insertAt = Notes.SelectionStart + Notes.SelectionLength;
                int insertEnd = insertAt + addText.Length;
                Notes.Text = Notes.Text.Insert(insertAt, addText);
                Notes.SelectionStart = insertEnd;
            }
            e.Handled = true;
        }
        // Insert 4 spaces when tab is pressed
        else if (e.Key == VirtualKey.Tab)
        {
            int selectionStart = Notes.SelectionStart;
            int selectionLength = Notes.SelectionLength;
            if (isShifting)
            {
                int lineStart = Math.Max(Notes.Text.Substring(0, selectionStart).LastIndexOf('\r'), 0);
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
            string search = UnicodeLookup.Text;

            UnicodeButtonSymbols.Clear();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var filteredItems = LatexUnicode.unicodeReplacements
                    .Where((kvp) => kvp.Key.Contains(search))
                    .Select((kvp) => kvp.Value);

                if (filteredItems.Count() == 0)
                {
                    UnicodeButtonSymbols.Add(NO_RESULTS_MSG);
                }
                else if (filteredItems.Count() > 64)
                {
                    UnicodeButtonSymbols.Add(TOO_MANY_RESULTS_MSG);
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
            string insertion = symbol == ":P" ? "👬" : symbol;
            int position = Notes.SelectionStart;
            Notes.Text = Notes.Text.Insert(position, insertion);
            _ = Notes.Focus(FocusState.Programmatic);
            Notes.SelectionStart = position + insertion.Length;
        }
    }
}
