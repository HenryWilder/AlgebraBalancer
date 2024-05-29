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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private void SetVisibleMacro(int index)
    {
        foreach (var child in Macros.Children)
        {
            child.Visibility = Visibility.Collapsed;
        }
        if (index == -1) { return; }
        Macros.Children[index].Visibility = Visibility.Visible;
    }

    public MainPage()
    {
        InitializeComponent();
        string[] headers = { "A", "B", "C" };
        foreach (string header in headers)
        {
            Inputs.Children.Add(new AlgebraInput(header));
        }
        //SetVisibleMacro(-1);
    }

    private static string Calculations(int x)
    {
        string result = "";

        {
            string oddOrEven = IsOdd(x) ? "odd" : "even";
            string primeOrComposite = IsPrime(x) ? "prime" : "composite";
            result += $"{x} is an {oddOrEven} {primeOrComposite}";
        }

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
        int sumPad = factorStrings.Max((f) => f.sum.Length);
        int diffPad = factorStrings.Max((f) => f.diff.Length);

        foreach (var (a, b, sum, diff) in factorStrings)
        {
            string aPadded    =    a.PadLeft(   aPad);
            string bPadded    =    b.PadLeft(   bPad);
            string sumPadded  =  sum.PadLeft( sumPad);
            string diffPadded = diff.PadLeft(diffPad);

            result += $"\n{aPadded} × {bPadded}; Σ={sumPadded}; Δ={diffPadded}";
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


        result += $"\nGCF: {GCF(a, b)}";
        result += $"\nLCM: {LCM(a, b)}";

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

    private void MathMacroButton_Click(object sender, RoutedEventArgs e)
    {
        switch (MathMacroSelector.SelectedIndex)
        {
            case 0: // FOIL
            {
                var (a, b, c, d) = (Macro_FOIL_A.Text, Macro_FOIL_B.Text, Macro_FOIL_C.Text, Macro_FOIL_D.Text);

                Notes.Text += $"({a} + {b})({c} + {d})\n";

                int abPad = Math.Max(a.Length, b.Length);
                a = a.PadLeft(abPad);
                b = b.PadLeft(abPad);

                int cdPad = Math.Max(a.Length, b.Length);
                c = c.PadLeft(cdPad);
                d = d.PadLeft(cdPad);

                Notes.Text += 
                    $"{a} × {c} = ?₁\n" +
                    $"{a} × {d} = ?₂\n" +
                    $"{b} × {c} = ?₃\n" +
                    $"{b} × {d} = ?₄\n" +
                    $"?₁ + ?₂ + ?₃ + ?₄\n";
            }
                break;

            case 1: // Factor
            {
                var (aStr, bStr, cStr) = (Macro_Factor_A.Text, Macro_Factor_B.Text, Macro_Factor_C.Text);

                bool isANum = int.TryParse(aStr, out int a);
                bool isCNum = int.TryParse(cStr, out int c);
                string acStr = isANum && isCNum ? $"{a * c}" : $"({aStr} × {bStr})";

                Notes.Text +=
                    $"{aStr}𝑥² + {bStr}𝑥 + {cStr}\n" +
                    $"?₁ + ?₂ = {bStr.PadLeft(acStr.Length)}\n" +
                    $"?₁ × ?₂ = {acStr.PadLeft(bStr.Length)}\n" +
                    $"(𝑥 + ?₁)(𝑥 + ?₂)\n";
            }
                break;

            case 2: // Matrix
            {
                var (aColStr, aRowStr) = (Macro_Matrix_ACols.Text, Macro_Matrix_ARows.Text);
                var (bColStr, bRowStr) = (Macro_Matrix_BCols.Text, Macro_Matrix_BRows.Text);

                if (
                    int.TryParse(aColStr, out int aCol) && aCol > 0 &&
                    int.TryParse(aRowStr, out int aRow) && aRow > 0 &&
                    int.TryParse(bColStr, out int bCol) && bCol > 0 &&
                    int.TryParse(bRowStr, out int bRow) && bRow > 0 &&
                    aCol == bRow
                )
                {
                    for (int tblRow = 0; tblRow <= aRow; ++tblRow) {
                        for (int tblCol = 0; tblCol < bCol; ++tblCol)
                        {
                            Notes.Text += "( ";
                            for (int cellComp = 1; cellComp < bRow; ++cellComp)
                            {
                                Notes.Text += tblRow == 0 ? ", " : "+ ";
                            }
                            Notes.Text += ") ";
                        }
                        if (tblRow > 0)
                        {
                            Notes.Text += "( ";
                            for (int cellComp = 1; cellComp < bRow; ++cellComp)
                            {
                                Notes.Text += ", ";
                            }
                            Notes.Text += ")";
                        }
                        Notes.Text += "\n";
                    }
                }
            }
                break;
        }
    }

    private void MathMacroSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetVisibleMacro(MathMacroSelector.SelectedIndex);
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

    private static readonly string CURSOR_SAVER = "\f";

    private static readonly Dictionary<string, string> unicodeReplacements = new Dictionary<string, string>{
        { @"\implies", "⇒" },
        { @"¬\implies", "⇏" },
        { @"\impliedby", "⇐" },
        { @"¬\impliedby", "⇍" },
        { @"\iff", "⇔" },
        { @"¬\iff", "⇎" },
        { @"\uArr", "⇑" },
        { @"\dArr", "⇓" },
        { @"\viff", "⇕" },
        { @"\mapsto", "↦" },
        { @"\MapsUp", "↥" },
        { @"\MapsDown", "↧" },
        { @"\mapsfrom", "↤" },
        { @"\Mapsfrom", "⤆" },
        { @"\Mapsto", "⤇" },
        { @"\to", "→" },
        { @"\gets", "←" },
        { @"\uarr", "↑" },
        { @"\darr", "↓" },
        { @"¬\to", "↛" },
        { @"¬\gets", "↚" },
        { @"\neg", "¬" },
        { @"\not", "¬" },
        { @"\invneg", "⌐" },
        { @"\ge", "≥" },
        { @"\gg", "≫" },
        { @"\ggg", "⋙" },
        { @"\le", "≤" },
        { @"\ll", "≪" },
        { @"\lll", "⋘" },
        { @"\approx", "≈" },
        { @"\triangleq", "≜" },
        { @"\defeq", "≝" },
        { @"\meq", "≞" },
        { @"\?eq", "≟" },
        { @"\equiv", "≡" },
        { @"\nequiv", "≢" },
        { @"\ne", "≠" },
        { @"\empty", "∅" },
        { @"\emptyset", "∅" },
        { @"\exists", "∃" },
        { @"\nexists", "∄" },
        { @"\in", "∈" },
        { @"\notin", "∉" },
        { @"\ni", "∋" },
        { @"\nni", "∌" },
        { @"\cup", "∪" },
        { @"\union", "∪" },
        { @"\cap", "∩" },
        { @"\intersection", "∩" },
        { @"\land", "∧" },
        { @"\wedge", "∧" },
        { @"\lor", "∨" },
        { @"\vee", "∨" },
        { @"\subset", "⊂" },
        { @"\supset", "⊃" },
        { @"\nsubset", "⊄" },
        { @"\nsupset", "⊅" },
        { @"\subseteq", "⊆" },
        { @"\supseteq", "⊇" },
        { @"\nsubseteq", "⊈" },
        { @"\nsupseteq", "⊉" },
        { @"\subsetneq", "⊊" },
        { @"\supsetneq", "⊋" },
        { @"\complement", "∁" },
        { @"\forall", "∀" },
        { @"\partial", "∂" },
        { @"\setminus", "⧵" },
        { @"\oplus", "⊕" },
        { @"\ominus", "⊖" },
        { @"\otimes", "⊗" },
        { @"\oslash", "⊘" },
        { @"\odot", "⊙" },
        { @"\circledcirc", "⊚" },
        { @"\vdash", "⊢" },
        { @"\dashv", "⊣" },
        { @"\top", "⊤" },
        { @"\bot", "⊥" },
        { @"\circ", "∘" },
        { @"\ast", "∗" },
        { @"\times", "·" },
        { @"\cdot", "⨯" },
        { @"\div", "÷" },
        { @"\prod", "∏" },
        { @"\coprod", "∐" },
        { @"\sum", "∑" },
        { @"\int", "∫" },
        { @"\iint", "∬" },
        { @"\iiint", "∭" },
        { @"\therefore", "∴" },
        { @"\because", "∵" },
        { @"\coloneq", "≔" },
        { @"\between", "≬" },
        { @"\succ", "≺" },
        { @"\prec", "≻" },
        { @"\pm", "±" },
        { @"\mp", "∓" },
        { @"\minus", "−" },
        { @"\propto", "∝" },
        { @"\infty", "∞" },
        { @"\ldots", "…" },
        { @"\vdots", "⋮" },
        { @"\cdots", "⋯" },
        { @"\iddots", "⋰" },
        { @"\ddots", "⋱" },
        { @"\frac", "⁄" },
        { @"\sqrt", "√" },
        { @"\deg", "°" },
        { @"\pi", "π" },
        { @"\Pi", "Π" },
        { @"\tau", "τ" },
        { @"\lambda", "λ" },
        // I think varphi and phi are swapped in VS, they look correct if you copy and paste them elsewhere
        { @"\varphi", "φ" },
        { @"\phi", "ϕ" },
        { @"\Phi", "Φ" },
        { @"\epsilon", "ϵ" },
        { @"\varepsilon", "ε" },
        { @"\gamma", "γ" },
        { @"\alpha", "α" },
        { @"\omega", "ω" },
        { @"\Omega", "Ω" },
        { @"\beta", "β" },
        { @"\sigma", "σ" },
        { @"\Sigma", "Σ" },
        { @"\delta", "δ" },
        { @"\Delta", "Δ" },
        { @"\nabla", "∇" },
        { @"\varsigma", "ς" },
        { @"\mu", "μ" },
        { @"\theta", "θ" },
        { @"\Theta", "Θ" },
        { @"\a", "𝑎" },
        { @"\b", "𝑏" },
        { @"\c", "𝑐" },
        { @"\d", "𝑑" },
        { @"\e", "𝑒" },
        { @"\f", "𝑓" },
        { @"\g", "𝑔" },
        { @"\i", "𝑖" },
        { @"\j", "𝑗" },
        { @"\k", "𝑘" },
        { @"\l", "𝑙" },
        { @"\m", "𝑚" },
        { @"\n", "𝑛" },
        { @"\o", "𝑜" },
        { @"\p", "𝑝" },
        { @"\q", "𝑞" },
        { @"\r", "𝑟" },
        { @"\s", "𝑠" },
        { @"\t", "𝑡" },
        { @"\u", "𝑢" },
        { @"\v", "𝑣" },
        { @"\w", "𝑤" },
        { @"\x", "𝑥" },
        { @"\y", "𝑦" },
        { @"\z", "𝑧" },
        { @"\O", "𝓞" },
        { @"\bbA", "𝔸" },
        { @"\bbB", "𝔹" },
        { @"\bbD", "𝔻" },
        { @"\bbE", "𝔼" },
        { @"\bbF", "𝔽" },
        { @"\bbG", "𝔾" },
        { @"\bbH", "ℍ" },
        { @"\bbI", "𝕀" },
        { @"\bbJ", "𝕁" },
        { @"\bbK", "𝕂" },
        { @"\bbL", "𝕃" },
        { @"\bbM", "𝕄" },
        { @"\bbN", "ℕ" },
        { @"\N", "ℕ" },
        { @"\bbO", "𝕆" },
        { @"\bbP", "ℙ" },
        { @"\bbQ", "ℚ" },
        { @"\Q", "ℚ" },
        { @"\bbR", "ℝ" },
        { @"\R", "ℝ" },
        { @"\bbS", "𝕊" },
        { @"\bbT", "𝕋" },
        { @"\bbU", "𝕌" },
        { @"\bbV", "𝕍" },
        { @"\bbW", "𝕎" },
        { @"\bbX", "𝕏" },
        { @"\bbY", "𝕐" },
        { @"\bbZ", "ℤ" },
        { @"\Z", "ℤ" },
    };

    // Escape prefix-"\" on macros to make them a literal "\" instead of an escape of its own
    private static readonly List<string> unicodeReplacementKeys =
        unicodeReplacements.Keys.Select((key) => key.Replace(@"\", @"\\")).ToList();

    private static readonly string rxUnicodeRelpacement =
        @"(?<!\\)(" + string.Join("|", unicodeReplacementKeys) + @")(?= |\\)";

    private string AlignMath(string str)
    {
        var lines = str.Split('\r').Select((line) => Regex.Replace(line, @" +&", " &")).ToList();
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
                string part = line[i];
                int partLen = part.Length;
                if (part.Contains(CURSOR_SAVER)) partLen -= CURSOR_SAVER.Length;
                if (partLen > alignments[i]) alignments[i] = partLen;
            }
        }
        return string.Join('\r',
            alignLines.Select((line, i) => string.Join('&',
                line.Select((part, j) => {
                    return j < line.Count() - 1
                        ? part.PadRight(alignments[j] + (part.Contains(CURSOR_SAVER) ? CURSOR_SAVER.Length : 0))
                        : part;
                })
            ))
        );
    }

    private string UnicodeReplacements(string str)
    {
        string macroPass = Regex.Replace(str, rxUnicodeRelpacement, (Match match) =>
        {
            string capture = match.Captures[0].Value;
            return unicodeReplacements.TryGetValue(capture, out string replacement)
                ? replacement
                : capture; // no change
        });

        string superscriptPass = Regex.Replace(macroPass, @"(\^{[0-9a-pr-z\-+=()]+})", (Match match) =>
        {
            string capture = match.Captures[0].Value;
            return capture.Substring("^{".Length, capture.Length - "^{}".Length)
                .Replace("0", "⁰")
                .Replace("1", "¹")
                .Replace("2", "²")
                .Replace("3", "³")
                .Replace("4", "⁴")
                .Replace("5", "⁵")
                .Replace("6", "⁶")
                .Replace("7", "⁷")
                .Replace("8", "⁸")
                .Replace("9", "⁹")
                .Replace("+", "⁺")
                .Replace("-", "⁻")
                .Replace("=", "⁼")
                .Replace("(", "⁽")
                .Replace(")", "⁾")
                .Replace("a", "ᵃ")
                .Replace("b", "ᵇ")
                .Replace("c", "ᶜ")
                .Replace("d", "ᵈ")
                .Replace("e", "ᵉ")
                .Replace("f", "ᶠ")
                .Replace("g", "ᵍ")
                .Replace("h", "ʰ")
                .Replace("i", "ⁱ")
                .Replace("j", "ʲ")
                .Replace("k", "ᵏ")
                .Replace("l", "ˡ")
                .Replace("m", "ᵐ")
                .Replace("n", "ⁿ")
                .Replace("o", "ᵒ")
                .Replace("p", "ᵖ")
                .Replace("r", "ʳ")
                .Replace("s", "ˢ")
                .Replace("t", "ᵗ")
                .Replace("u", "ᵘ")
                .Replace("v", "ᵛ")
                .Replace("w", "ʷ")
                .Replace("x", "ˣ")
                .Replace("y", "ʸ")
                .Replace("z", "ᶻ")
            ;
        });

        string subscriptPass = Regex.Replace(superscriptPass, @"(_{[0-9aexhklnopst\-+=()]+})", (Match match) =>
        {
            string capture = match.Captures[0].Value;
            return capture.Substring("_{".Length, capture.Length - "_{}".Length)
                .Replace("0", "₀")
                .Replace("1", "₁")
                .Replace("2", "₂")
                .Replace("3", "₃")
                .Replace("4", "₄")
                .Replace("5", "₅")
                .Replace("6", "₆")
                .Replace("7", "₇")
                .Replace("8", "₈")
                .Replace("9", "₉")
                .Replace("+", "₊")
                .Replace("-", "₋")
                .Replace("=", "₌")
                .Replace("(", "₍")
                .Replace(")", "₎")
                .Replace("a", "ₐ")
                .Replace("e", "ₑ")
                .Replace("x", "ₓ")
                .Replace("h", "ₕ")
                .Replace("k", "ₖ")
                .Replace("l", "ₗ")
                .Replace("m", "ₘ")
                .Replace("n", "ₙ")
                .Replace("o", "ₒ")
                .Replace("p", "ₚ")
                .Replace("s", "ₛ")
                .Replace("t", "ₜ")
            ;
        });

        string alignPass = !Regex.IsMatch(subscriptPass, @"^\\noalign\b")
            ? AlignMath(subscriptPass)
            : subscriptPass;

        return alignPass;
    }

    private static readonly string rxSource = $@"@{{{{((?:(?!}}}}).)*?{CURSOR_SAVER}.*?)}}}}";
    private static readonly string rxCopy = $@"@{{{{((?:(?!{CURSOR_SAVER}).)*?)}}}}";

    private string ApplyDuplications(string text)
    {
        var sourceMatch = Regex.Match(text, rxSource);
        if (sourceMatch.Success)
        {
            string replacement = sourceMatch.Captures[0].Value.Replace(CURSOR_SAVER, "");
            text = Regex.Replace(text, rxCopy, replacement);
        }
        return text;
    }

    private static readonly string rxEndDuplicationFront = $@"@(?:{{{CURSOR_SAVER}|{CURSOR_SAVER}{{)(.*?)}}}}";
    private static readonly string rxEndDuplicationBack = $@"@{{{{((?:(?!}}}}).)*?)@{CURSOR_SAVER}}}}}";

    private string InlineMacros(string text)
    {
        // Remove all duplicators

        if (Regex.IsMatch(text, rxEndDuplicationFront + "|" + rxEndDuplicationBack))
        {
            text = Regex.Replace(text, rxEndDuplicationFront, $"{CURSOR_SAVER}@{{{{$1}}}}");
            text = Regex.Replace(text, rxEndDuplicationBack, $"@{{{{$1}}}}{CURSOR_SAVER}");
            text = Regex.Replace(text, @"@{{?(.*?)@?}}", "@{{$1}}");
            text = Regex.Replace(text, @"@{{(.*?)}}", "$1");
        }

        // Add duplicator
        text = Regex.Replace(text, $@"@{CURSOR_SAVER}(?!{{{{)", $"@{{{{{CURSOR_SAVER}}}}}");
        //text = Regex.Replace(text, @"@(?!{{)", "@{{}}");

        return text;
    }

    private void Notes_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        int selectionStart = Notes.SelectionStart;
        string updated = Notes.Text.Insert(selectionStart, CURSOR_SAVER);
        updated = ApplyDuplications(updated);
        updated = InlineMacros(updated);
        updated = UnicodeReplacements(updated);
        int cursorPos = updated.IndexOf(CURSOR_SAVER);
        Notes.Text = updated.Replace(CURSOR_SAVER, "");
        Notes.SelectionStart = cursorPos;
    }
}
