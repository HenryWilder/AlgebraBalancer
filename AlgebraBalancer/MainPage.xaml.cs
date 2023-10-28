﻿using System;
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
            result += "\nFactors:";
            try
            {
                List<(string a, string b, string sum, string diff)>
                    factorStrings = Factors(x)
                        .Select((f) => ($"{f.a}", $"{f.b}", $"{f.a + f.b}", $"{f.b - f.a}"))
                        .ToList();

                int aPad    = factorStrings.Max((f) => f.a   .Length);
                int bPad    = factorStrings.Max((f) => f.b   .Length);
                int sumPad  = factorStrings.Max((f) => f.sum .Length);
                int diffPad = factorStrings.Max((f) => f.diff.Length);

                foreach (var (a, b, sum, diff) in factorStrings)
                {
                    string aPadded    =    a.PadLeft(   aPad);
                    string bPadded    =    b.PadLeft(   bPad);
                    string sumPadded  =  sum.PadLeft( sumPad);
                    string diffPadded = diff.PadLeft(diffPad);

                    result += $"\n{aPadded} × {bPadded}; Σ={sumPadded}; Δ={diffPadded})";
                }
            }
            catch
            {
                result += "...";
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
            result += $"\n{a} × {b} = {a * b}";
            result += $"\n{a} ÷ {b} = ";
            try
            {
                result += SimplifiedFraction(a, b).ToString();
            }
            catch (DivideByZeroException)
            {
                result += "undefined";
            }
            result += $"\n{a} % {b} = " + (b != 0 ? $"{a % b}" : "undefined");

            {
                result += $"\n{a} ^ {b} = ";
                double powered = Math.Pow(a, Math.Abs(b));
                if (powered > ulong.MaxValue)
                {
                    result += (b < 0)
                        ? $"ε"
                        : $"𝓗";
                }
                else
                {
                    result += (b < 0)
                        ? $"1/{Convert.ToUInt64(powered)}"
                        : $"{Convert.ToUInt64(powered)}";
                }
            }


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
                List<(string common, string a, string b)>
                    factorStrings = CommonFactors(a, b)
                        .Select((f) => ($"{f.common}", $"{f.a}", $"{f.b}"))
                        .ToList();

                int commonPad = factorStrings.Max((f) => f.common.Length);
                int aPad      = factorStrings.Max((f) => f.a.Length);
                int bPad      = factorStrings.Max((f) => f.b.Length);

                foreach (var (cFacStr, aFacStr, bFacStr) in factorStrings)
                {
                    string commonPadded = cFacStr.PadLeft(commonPad);
                    string aPadded      = aFacStr.PadLeft(aPad);
                    string bPadded      = bFacStr.PadLeft(bPad);

                    result += $"\n{commonPadded} × ({aPadded}, {bPadded})";
                }
            }
            catch
            {
                result += "...";
            }

            return result;
        }
        
        private static string Calculations(int a, int b, int c)
        {
            string result = "";

            Radical magnitude = SimplifiedRoot(a * a + b * b + c * c);
            result += $"|A| = {magnitude}";
            {
                RadicalFraction aRadFrac = SimplifiedRadicalFraction(a, magnitude);
                RadicalFraction bRadFrac = SimplifiedRadicalFraction(b, magnitude);
                RadicalFraction cRadFrac = SimplifiedRadicalFraction(c, magnitude);
                result += $"\nÂ = ({aRadFrac}, {bRadFrac}, {cRadFrac})";
            }
            result += $"\nΣ({a}, {b}, {c}) = {a + b + c}";
            result += $"\nΣ({a}, {b}, {c}) = {a + b + c}";
            result += $"\n∏({a}, {b}, {c}) = {a * b * c}";

            result += "\nGCF: ";
            try   { result += GCF(GCF(a, b), c).ToString(); }
            catch { result += "..."; }

            result += "\nLCM: ";
            try   { result += LCM(LCM(a, b), c).ToString(); }
            catch { result += "..."; }

            result += $"\n{a}𝑥² + {b}𝑥 + {c} =";
            result += $"\n  (-({b})±√(({b})²-4({a})({c})))/2({a})";
            try
            {
                var quadratic = SimplifiedRadicalFraction(SimplifiedRoot(b * b - 4 * a * c), 2 * a);
                quadratic.addSubNumerator = -b;

                string unsimplified = $"\n  ({-b}±√{b * b - 4 * a * c})/{2 * a}";
                string   simplified = $"\n  {quadratic}"; // Currently inaccurate because b isn't getting simplified

                result += unsimplified + (simplified != unsimplified ? simplified : string.Empty);
            }
            catch
            {
                result += $"\n  ({-b}±√{b * b - 4 * a * c})/{2 * a}";
                result += $"\n  imaginary or undefined";
            }

            return result;
        }

        private static string Calculations(List<int> parameters)
        {
            switch (parameters.Count)
            {
                case 1: return Calculations(parameters[0]);
                case 2: return Calculations(parameters[0], parameters[1]);
                case 3: return Calculations(parameters[0], parameters[1], parameters[2]);
                default: return "...";
            };
        }

        private string calculations;

        private async void Update(object sender, RoutedEventArgs args)
        {
            Output.Text = "Calculating...";
            try
            {
                List<int> parameters = Inputs.Children
                    .Select((input) => (input as AlgebraInput).Value)
                    .Where((int? value) => value.HasValue)
                    .Select((int? value) => value.Value)
                    .ToList();

                await Task.Run(() => calculations = Calculations(parameters));

                Output.Text = calculations;
            }
            catch
            {
                Output.Text = "...";
            }
        }
    }
}
