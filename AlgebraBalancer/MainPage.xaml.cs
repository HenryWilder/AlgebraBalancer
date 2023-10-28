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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Inputs.Children.Add(new AlgebraInput());
            Inputs.Children.Add(new AlgebraInput());
            Inputs.Children.Add(new AlgebraInput());
        }

        private static string GetUnaryOpsString(int x)
        {
            string result = "";

            {
                string oddOrEven = IsOdd(x) ? "odd" : "even";
                string primeOrComposite = IsPrime(x) ? "prime" : "composite";
                result += $"{x} is an {oddOrEven} {primeOrComposite}";
            }

            // Square
            result += $"\n{x}² = " + (x * x).ToString();

            // Root
            result += $"\n√{x} = ";
            try
            {
                Radical radical = SimplifiedRoot(x);
                if (radical.coefficient != 1 && radical.radicand != 1)
                {
                    result += $"(√{radical.coefficient * radical.coefficient})√{radical.radicand} = ";
                }
                result += radical.ToString();
            }
            catch (Exception e) { result += e.Message; }

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
        
        private static string GetBinaryOpsString(int a, int b)
        {
            string result =
                  a == b ? $"{a} = {b}"
                : a >  b ? $"{a} > {b}"
                : a <  b ? $"{a} < {b}"
                         : $"{a} ≠ {b}";

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

        private static string GetNOpsString(List<int> parameters)
        {
            switch (parameters.Count)
            {
                case 1: return GetUnaryOpsString(parameters[0]);
                case 2: return GetBinaryOpsString(parameters[0], parameters[1]);
                default: return "...";
            };
        }

        private void Update(object sender, RoutedEventArgs args)
        {
            try
            {
                List<int> parameters = Inputs.Children
                    .Select((input) => (input as AlgebraInput).Value)
                    .Where((int? value) => value.HasValue)
                    .Select((int? value) => value.Value)
                    .ToList();

                Output.Text = GetNOpsString(parameters);
            }
            catch
            {
                Output.Text = "...";
            }
        }
    }
}
