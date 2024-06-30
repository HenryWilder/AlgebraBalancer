using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using static AlgebraBalancer.Algebra;
using System.Xml.Linq;
using Windows.Web.Syndication;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Documents;
using System.Collections;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    //private void SetVisibleMacro(int index)
    //{
    //    foreach (var child in Macros.Children)
    //    {
    //        child.Visibility = Visibility.Collapsed;
    //    }
    //    if (index == -1) { return; }
    //    Macros.Children[index].Visibility = Visibility.Visible;
    //}

    public MainPage()
    {
        InitializeComponent();
        string[] headers = { "A", "B", "C" };
        foreach (string header in headers)
        {
            Inputs.Children.Add(new AlgebraInput(header));
        }
        foreach (var item in Inputs.Children)
        {
            item.PreviewKeyDown += (object sender, KeyRoutedEventArgs e) => {
                if (e.Key == VirtualKey.Enter)
                {
                    Update(null, new());
                    e.Handled = true;
                }
            };
        }
        //SetVisibleMacro(-1);
    }

    private static string Calculations(int x)
    {
        string result = "";

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

        result += $"\n{a}𝑥² + {b}𝑥 + {c} =";
        result += $"\n  (-({b})±√(({b})²-4({a})({c})))/2({a})";

        var formula = new RadicalFraction(-b, new Radical(b * b - 4 * a * c), 2 * a);

        string unsimplified = $"\n  {formula}";
        result += unsimplified;

        string simplified   = $"\n  {formula.Simplified()}";
        if (simplified != unsimplified) { result += simplified; }

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

    private async void Update(object sender, RoutedEventArgs args)
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

    //private void MathMacroButton_Click(object sender, RoutedEventArgs e)
    //{
    //    switch (MathMacroSelector.SelectedIndex)
    //    {
    //        case 0: // FOIL
    //        {
    //            var (a, b, c, d) = (Macro_FOIL_A.Text, Macro_FOIL_B.Text, Macro_FOIL_C.Text, Macro_FOIL_D.Text);

    //            Notes.Text += $"({a} + {b})({c} + {d})\n";

    //            int abPad = Math.Max(a.Length, b.Length);
    //            a = a.PadLeft(abPad);
    //            b = b.PadLeft(abPad);

    //            int cdPad = Math.Max(a.Length, b.Length);
    //            c = c.PadLeft(cdPad);
    //            d = d.PadLeft(cdPad);

    //            Notes.Text += 
    //                $"{a} × {c} = ?₁\n" +
    //                $"{a} × {d} = ?₂\n" +
    //                $"{b} × {c} = ?₃\n" +
    //                $"{b} × {d} = ?₄\n" +
    //                $"?₁ + ?₂ + ?₃ + ?₄\n";
    //        }
    //            break;

    //        case 1: // Factor
    //        {
    //            var (aStr, bStr, cStr) = (Macro_Factor_A.Text, Macro_Factor_B.Text, Macro_Factor_C.Text);

    //            bool isANum = int.TryParse(aStr, out int a);
    //            bool isCNum = int.TryParse(cStr, out int c);
    //            string acStr = isANum && isCNum ? $"{a * c}" : $"({aStr} × {bStr})";

    //            Notes.Text +=
    //                $"{aStr}𝑥² + {bStr}𝑥 + {cStr}\n" +
    //                $"?₁ + ?₂ = {bStr.PadLeft(acStr.Length)}\n" +
    //                $"?₁ × ?₂ = {acStr.PadLeft(bStr.Length)}\n" +
    //                $"(𝑥 + ?₁)(𝑥 + ?₂)\n";
    //        }
    //            break;

    //        case 2: // Matrix
    //        {
    //            var (aColStr, aRowStr) = (Macro_Matrix_ACols.Text, Macro_Matrix_ARows.Text);
    //            var (bColStr, bRowStr) = (Macro_Matrix_BCols.Text, Macro_Matrix_BRows.Text);

    //            if (
    //                int.TryParse(aColStr, out int aCol) && aCol > 0 &&
    //                int.TryParse(aRowStr, out int aRow) && aRow > 0 &&
    //                int.TryParse(bColStr, out int bCol) && bCol > 0 &&
    //                int.TryParse(bRowStr, out int bRow) && bRow > 0 &&
    //                aCol == bRow
    //            )
    //            {
    //                for (int tblRow = 0; tblRow <= aRow; ++tblRow) {
    //                    for (int tblCol = 0; tblCol < bCol; ++tblCol)
    //                    {
    //                        Notes.Text += "( ";
    //                        for (int cellComp = 1; cellComp < bRow; ++cellComp)
    //                        {
    //                            Notes.Text += tblRow == 0 ? ", " : "+ ";
    //                        }
    //                        Notes.Text += ") ";
    //                    }
    //                    if (tblRow > 0)
    //                    {
    //                        Notes.Text += "( ";
    //                        for (int cellComp = 1; cellComp < bRow; ++cellComp)
    //                        {
    //                            Notes.Text += ", ";
    //                        }
    //                        Notes.Text += ")";
    //                    }
    //                    Notes.Text += "\n";
    //                }
    //            }
    //        }
    //            break;
    //    }
    //}

    //private void MathMacroSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    SetVisibleMacro(MathMacroSelector.SelectedIndex);
    //}

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

    private static readonly string CURSOR_SAVER = "\f";

    private string AlignMath(string str)
    {
        static string MinimizePadding(string part)
        {
            part = Regex.Replace(part, $@"^ *{CURSOR_SAVER} *$", CURSOR_SAVER);
            part = Regex.Replace(part, @" +", " ");
            part = Regex.Replace(part, @"^ +$", "");
            return part;
        }

        var lines = str.Split("\r").Select((line) => Regex.Replace(line, @" +&", " &")).ToList();
        var alignLines = lines.Select((line) => line.Split('&'));
        var alignments = new List<int>();
        foreach (string[] line in alignLines)
        {
            while (alignments.Count() < line.Count() - 1)
            {
                alignments.Add(-1);
            }
            for (int i = 0; i < line.Count() - 1; ++i)
            {
                string part = MinimizePadding(line[i]).Replace(CURSOR_SAVER, "");
                if (i == 0) part = Regex.Replace(part, @"^ +", "");
                int partLen = part.Length;
                if (partLen > alignments[i]) alignments[i] = partLen;
            }
        }
        return string.Join("\r",
            alignLines.Select((line, i) => string.Join('&',
                line.Select((part, j) => {
                    if (j < line.Count() - 1 && j < alignments.Count())
                    {
                        int accountForCursor = part.Contains(CURSOR_SAVER) ? CURSOR_SAVER.Length : 0;
                        int partWidth = alignments[j] + accountForCursor;
                        part = MinimizePadding(part);
                        if (j == 0) part = Regex.Replace(part, @"^ +", "");
                        part = (j & 1) == 0 ? part.PadLeft(partWidth) : part.PadRight(partWidth);
                    }
                    return part;
                })
            ))
        );
    }

    private string UnicodeReplacements(string str)
    {
        string macroPass = LatexUnicode.ApplyUnicodeReplacements(str);

        string remapPass = LatexUnicode.ApplyRemapPatterns(macroPass);

        string alignPass = !Regex.IsMatch(remapPass, @"^\\noalign\b")
            ? AlignMath(remapPass)
            : remapPass;

        return alignPass;
    }

    //private static readonly string rxSource = $@"@{{{{((?:(?!}}}}).)*?{CURSOR_SAVER}.*?)}}}}";
    //private static readonly string rxCopy = $@"@{{{{((?:(?!{CURSOR_SAVER}).)*?)}}}}";

    //private string ApplyDuplications(string text)
    //{
    //    var sourceMatch = Regex.Match(text, rxSource);
    //    if (sourceMatch.Success)
    //    {
    //        string replacement = sourceMatch.Captures[0].Value.Replace(CURSOR_SAVER, "");
    //        text = Regex.Replace(text, rxCopy, replacement);
    //    }
    //    return text;
    //}

    //private static readonly string rxEndDuplicationFront = $@"@(?:{{{CURSOR_SAVER}|{CURSOR_SAVER}{{)(.*?)}}}}";
    //private static readonly string rxEndDuplicationBack = $@"@{{{{((?:(?!}}}}).)*?)@{CURSOR_SAVER}}}}}";

    //private string InlineMacros(string text)
    //{
    //    // Remove all duplicators

    //    if (Regex.IsMatch(text, rxEndDuplicationFront + "|" + rxEndDuplicationBack))
    //    {
    //        text = Regex.Replace(text, rxEndDuplicationFront, $"{CURSOR_SAVER}@{{{{$1}}}}");
    //        text = Regex.Replace(text, rxEndDuplicationBack, $"@{{{{$1}}}}{CURSOR_SAVER}");
    //        text = Regex.Replace(text, @"@{{?(.*?)@?}}", "@{{$1}}");
    //        text = Regex.Replace(text, @"@{{(.*?)}}", "$1");
    //    }

    //    // Add duplicator
    //    text = Regex.Replace(text, $@"@{CURSOR_SAVER}(?!{{{{)", $"@{{{{{CURSOR_SAVER}}}}}");
    //    //text = Regex.Replace(text, @"@(?!{{)", "@{{}}");

    //    return text;
    //}

    private void Notes_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        int selectionStart = Notes.SelectionStart;
        string updated = Notes.Text.Insert(selectionStart, CURSOR_SAVER);
        //updated = ApplyDuplications(updated);
        //updated = InlineMacros(updated);
        updated = UnicodeReplacements(updated);
        int cursorPos = updated.IndexOf(CURSOR_SAVER);
        Notes.Text = updated.Replace(CURSOR_SAVER, "");
        Notes.SelectionStart = cursorPos;
    }

    private string StringFOIL(string str)
    {
        str = Regex.Replace(str, @" *", "");

        Match match;

        if ((match = Regex.Match(str, @"^\((-?[0-9a-z]+)([+-][0-9a-z]+)\)\^2$")).Success)
        {
            string aStr = match.Groups[1].Value;
            string bStr = match.Groups[2].Value;
            
            bool isAInt = int.TryParse(aStr, out int a);
            bool isBInt = int.TryParse(bStr, out int b);

            if (isAInt && isBInt)
            {
                return $"{a*a+2*a*b+b*b}";
            }
            else if (isAInt)
            {
                return $"{a*a} + {2*a}({bStr}) + ({bStr})^2";
            }
            else if (isBInt)
            {
                return $"({aStr})^2 + {2*b}({aStr}) + {b*b}";
            }
            else
            {
                return $"({aStr})^2 + 2({aStr})({bStr}) + ({bStr})^2";
            }
        }

        if ((match = Regex.Match(str, @"^\((-?[0-9a-z]+)([+-][0-9a-z]+)\)\((-?[0-9a-z]+)([+-][0-9a-z]+)\)$")).Success)
        {
            string aStr = match.Groups[1].Value;
            string bStr = match.Groups[2].Value;
            string cStr = match.Groups[3].Value;
            string dStr = match.Groups[4].Value;

            bool isAInt = int.TryParse(aStr, out int a);
            bool isBInt = int.TryParse(bStr, out int b);
            bool isCInt = int.TryParse(cStr, out int c);
            bool isDInt = int.TryParse(dStr, out int d);

            if (isAInt && isBInt && isCInt && isDInt)
            {
                return $"{a * c + b * c + a * d + b * d}";
            }
            if (isAInt && isBInt && isCInt && !isDInt)
            {
                return $"{a + b}({dStr}) + {a * c + b * c} ";
            }
            if (isAInt && isBInt && !isCInt && isDInt)
            {
                return $"{a + b}({cStr}) + {a * d + b * d} ";
            }
            if (isAInt && !isBInt && isCInt && isDInt)
            {
                return $"{c + d}({bStr}) + {c * a + d * a} ";
            }
            if (!isAInt && isBInt && isCInt && isDInt)
            {
                return $"{c + d}({aStr}) + {b * a + b * a} ";
            }
            if (!isAInt && isBInt && !isCInt && isDInt)
            {
                return $"({aStr})({cStr}) + {b}({cStr}) + {d}({aStr}) + {b * d}";
            }
            if (isAInt && !isBInt && isCInt && !isDInt)
            {
                return $"({bStr})({dStr}) + {a}({dStr}) + {c}({bStr}) + {a * c}";
            }
            if (isAInt && isBInt && !isCInt && !isDInt)
            {
                return $"{a + b}({cStr}) + {a + b}({dStr})";
            }
            if (!isAInt && !isBInt && isCInt && isDInt)
            {
                return $"{c + d}({aStr}) + {c + d}({bStr})";
            }
            if (!isAInt && !isBInt && !isCInt && !isDInt)
            {
                return $"({aStr})({cStr}) + ({bStr})({cStr}) + ({dStr})({aStr}) + ({bStr})({dStr})";
            }
        }

        return str;
    }

    private string StringFactor(string str)
    {
        str = Regex.Replace(str, @" *", "");
        Match match;
        if ((match = Regex.Match(str, @"^\((-?[0-9a-z]+)([+-][0-9a-z]+)\)\^2$")).Success)
        {
            string aStr = match.Groups[1].Value;
            string bStr = match.Groups[2].Value;

            bool isAInt = int.TryParse(aStr, out int a);
            bool isBInt = int.TryParse(bStr, out int b);

            if (isAInt && isBInt)
            {
                return $"{a * a + 2 * a * b + b * b}";
            }
            else if (isAInt)
            {
                return $"{a * a} + {2 * a}{bStr} + {bStr}^2";
            }
            else if (isBInt)
            {
                return $"{aStr}^2 + {2 * b}{aStr} + {b * b}";
            }
            else
            {
                return $"{aStr}^2 + 2{aStr}{bStr} + {bStr}^2";
            }
        }
        return str;
    }

    private void Notes_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
        bool isShifting = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
        bool isCtrling = (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        var altState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);
        bool isAlting = (altState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

        // Duplicate line
        if ((e.Key is VirtualKey.Down or VirtualKey.Up) && isShifting && isAlting)
        {
            int selectionStart = Notes.SelectionStart;
            string notesText = Notes.Text;
            int selectionIndex = Math.Min(selectionStart, notesText.Length - 1);

            int startOfLine = notesText.LastIndexOf('\n', selectionIndex - 1);
            if (startOfLine == -1) startOfLine = notesText.LastIndexOf('\r', selectionIndex - 1);
            if (startOfLine == -1) startOfLine = 0;

            int endOfLine = notesText.IndexOf('\r', selectionIndex);
            if (endOfLine == -1) endOfLine = notesText.IndexOf('\r', selectionIndex);
            if (endOfLine == -1) endOfLine = notesText.Length;

            string lineText = notesText.Substring(startOfLine, endOfLine - startOfLine);
            if (lineText[0] is not '\n' and not '\r')
            {
                lineText = lineText.Insert(0, "\n");
            }
            int cursorOffset = e.Key == VirtualKey.Down ? lineText.Length : 0;

            Notes.Text = notesText.Insert(endOfLine, lineText);
            Notes.SelectionStart = selectionStart + cursorOffset;
            e.Handled = true;
        }
        // Put selection in math input
        else if (isCtrling && (e.Key is VirtualKey.Number1 or VirtualKey.Number2 or VirtualKey.Number3))
        {
            int index = e.Key - VirtualKey.Number1;
            (Inputs.Children[index] as AlgebraInput).SetValue(Notes.SelectedText);
            Update(null, new());
            e.Handled = true;
        }
        // FOIL selection
        //else if (e.Key == VirtualKey.F && Notes.SelectionLength > 0)
        //{
        //    int selectionStart = Notes.SelectionStart;
        //    int selectionLength = Notes.SelectionLength;
        //    string polytext = Notes.SelectedText;

        //    // FOIL
        //    if (isShifting)
        //    {
        //        polytext = StringFOIL(polytext);
        //    }

        //    // Factor
        //    else
        //    {
        //        polytext = StringFactor(polytext);
        //    }

        //    Notes.Text = Notes.Text.Remove(selectionStart, selectionLength).Insert(selectionStart, polytext);
        //    Notes.SelectionStart = selectionStart;
        //    Notes.SelectionLength = polytext.Length;
        //    e.Handled = true;
        //}
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
}
