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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private readonly static DataTable dt = new DataTable();

        private string GetUnaryOpsString(int x)
        {
            string result = string.Empty;

            // Square
            result += $"\n{x}² = " + (x * x).ToString();

            // Root
            result += $"\n√{x} = ";
            try   { result += SimplifiedRoot(x).ToString(); }
            catch { result += "..."; }

            // Factors
            result += "\nFactors:\n";
            try
            {
                var factors = Factors(x);
                int factorsPad1 = 0;
                int factorsPad2 = 0;
                int sumPad = 0;
                int differencePad = 0;
                foreach (var f in factors)
                {
                    factorsPad1 = Math.Max(f.Item1.ToString().Length, factorsPad1);
                    factorsPad2 = Math.Max(f.Item2.ToString().Length, factorsPad2);
                    sumPad = Math.Max((f.Item2 + f.Item1).ToString().Length, differencePad);
                    differencePad = Math.Max((f.Item2 - f.Item1).ToString().Length, differencePad);
                }

                result += string.Join("\n", from f in factors
                                                 select
                        f.Item1.ToString().PadLeft(factorsPad1) + " × " +
                        f.Item2.ToString().PadLeft(factorsPad2) + "; Σ=" +
                        (f.Item2 + f.Item1).ToString().PadLeft(sumPad) + "; Δ=" +
                        (f.Item2 - f.Item1).ToString().PadLeft(differencePad));
            }
            catch
            {
                result += "...";
            }

            return result;
        }
        
        private string GetBinaryOpsString(int a, int b)
        {
            string result = string.Empty;

            result += $"\n{a} + {b} = " + (a + b).ToString();
            result += $"\n{a} - {b} = " + (a - b).ToString();
            result += $"\n{a} × {b} = " + (a * b).ToString();
            result += $"\n{a} ÷ {b} = ";
            try   { result += SimplifiedFraction(a, b).ToString(); }
            catch { result += "..."; }


            result += "\nGCF: ";
            try   { result += GCF(a, b).ToString(); }
            catch { result += "..."; }

            result += "\nLCM: ";
            try   { result += LCM(a, b).ToString(); }
            catch { result += "..."; }

            // Common factors
            result += "\nCommon Factors:";
            try
            {
                // c = Common

                var factors = CommonFactors(a, b);
                var factorStrings = (
                    from fac in factors
                    select (
                        fac.Item1.ToString(),
                        fac.Item2.ToString(),
                        fac.Item3.ToString()
                    )).ToList();
                
                int cFacPad = 0, aFacPad = 0, bFacPad = 0;
                foreach (var (cFacStr, aFacStr, bFacStr) in factorStrings)
                {
                    cFacPad = Math.Max(cFacStr.Length, cFacPad);
                    aFacPad = Math.Max(aFacStr.Length, aFacPad);
                    bFacPad = Math.Max(bFacStr.Length, bFacPad);
                }

                foreach (var (cFacStr, aFacStr, bFacStr) in factorStrings)
                {
                    string cStr = cFacStr.PadLeft(cFacPad);
                    string aStr = aFacStr.PadLeft(aFacPad);
                    string bStr = bFacStr.PadLeft(bFacPad);

                    result += $"\n{cStr} × ({aStr}, {bStr})";
                }
            }
            catch
            {
                result += "...";
            }

            return result;
        }

        private void Update(object sender, TextChangedEventArgs args)
        {
            try
            {
                string aText = InputA.Text;
                string bText = InputB.Text;

                bool isUsingA = aText != string.Empty;
                bool isUsingB = bText != string.Empty;
                bool isBinary = isUsingA && isUsingB;

                EchoInputA.Visibility = isUsingA || !isBinary ? Visibility.Visible : Visibility.Collapsed;
                EchoInputB.Visibility = isUsingB &&  isBinary ? Visibility.Visible : Visibility.Collapsed;

                Output.Text = "";

                if (!isBinary) // Unary
                {
                    string text = isUsingA ? aText : bText;

                    if (text.Length > 7)
                    {
                        throw new StackOverflowException();
                    }

                    int x = (int)dt.Compute(text, "");
                    EchoInputA.Text = x.ToString();

                    Output.Text = GetUnaryOpsString(x);
                }
                else // Binary
                {
                    if (aText.Length > 7 || bText.Length > 7)
                    {
                        throw new StackOverflowException();
                    }
                                        
                    int a = (int)dt.Compute(aText, "");
                    int b = (int)dt.Compute(bText, "");
                    EchoInputA.Text = a.ToString();
                    EchoInputB.Text = b.ToString();

                    Output.Text = GetBinaryOpsString(a, b);
                }
            }
            catch
            {
                Output.Text = "...";
            }
        }
    }
}
