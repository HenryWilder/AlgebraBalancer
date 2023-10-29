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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        string[] headers = { "A", "B", "C" };
        foreach (string header in headers)
        {
            Inputs.Children.Add(new AlgebraInput(header));
        }
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
        catch (OverflowException) { result += new Huge(); }
        result += $"\n{a} ÷ {b} = {new Fraction(a, b).Simplified()}";
        result += $"\n{a} % {b} = " + (b != 0 ? new Number(a % b) : new Undefined());
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

        var magnitude = new Radical(a * a + b * b + c * c).Simplified();
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
            else
            {
                throw new NotImplementedException();
            }
            result += $"\nÂ = ({aPart}, {bPart}, {cPart})";
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
            await Task.Run(() => calculations = Calculations(parameters));
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
        if (MathMacroSelector.SelectedValue is ComboBoxItem selectedItem)
        {
            string selection = selectedItem.Content.ToString();
            string macroText = string.Empty;
            switch (selection)
            {
                case "FOIL":
                    var (a, b, c, d) = (Macro_FOIL_A.Text, Macro_FOIL_B.Text, Macro_FOIL_C.Text, Macro_FOIL_D.Text);

                    int abPad = Math.Max(a.Length, b.Length);
                    a = a.PadLeft(abPad);
                    b = b.PadLeft(abPad);
                    int cdPad = Math.Max(a.Length, b.Length);
                    c = c.PadLeft(cdPad);
                    d = d.PadLeft(cdPad);

                    macroText = $"\n{a} * {c} = \n{a} * {d} = \n{b} * {c} = \n{b} * {d} = \n";
                    break;
            }
            Notes.Text += macroText;
        }
    }
}
