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
            this.InitializeComponent();
        }

        private readonly static DataTable dt = new DataTable();

        private static bool IsInt(double x) =>
            Math.Abs(x % 1) <= (double.Epsilon * 100);

        private void Update(object sender, TextChangedEventArgs args)
        {
            try
            {
                string aText = InputA.Text;
                string bText = InputB.Text;

                bool isBinary = aText != string.Empty && bText != string.Empty;

                Output.Text = "";

                if (!isBinary) // Unary
                {

                    string text = aText == string.Empty ? bText : aText;

                    if (text.Length > 7)
                    {
                        throw new StackOverflowException();
                    }

                    int x = (int)dt.Compute(text, "");

                    // Square
                    {
                        Output.Text += $"\n{x}² = " + (x * x).ToString();
                    }

                    // Root
                    {
                        double root = Math.Sqrt(x);
                        Output.Text += $"\n√{x} = " + (IsInt(root) ? ((int)root).ToString() : SimplifiedRoot(x));
                    }

                    // Factors
                    {
                        var factors = Factors(x);
                        int factorsPad1 = 0;
                        int factorsPad2 = 0;
                        foreach (var f in factors)
                        {
                            factorsPad1 = Math.Max(f.Item1.ToString().Length, factorsPad1);
                            factorsPad2 = Math.Max(f.Item2.ToString().Length, factorsPad2);
                        }

                        Output.Text += "\nFactors:\n" + string.Join("\n", from f in factors select
                                f.Item1.ToString().PadLeft(factorsPad1) + " × " +
                                f.Item2.ToString().PadLeft(factorsPad2));
                    }
                }
                else // Binary
                {
                    if (aText.Length > 7 || bText.Length > 7)
                    {
                        throw new StackOverflowException();
                    }
                                        
                    int a = (int)dt.Compute(aText, "");
                    int b = (int)dt.Compute(bText, "");

                    int gcf = GCF(a, b);

                    Output.Text += $"\n{a}+{b} = " + (a + b).ToString();
                    Output.Text += $"\n{a}-{b} = " + (a - b).ToString();
                    Output.Text += $"\n{a}×{b} = " + (a * b).ToString();
                    Output.Text += $"\n{a}÷{b} = " + ((a % b == 0) ? (a / b).ToString() : $"{a / gcf}/{b / gcf}");
                    Output.Text += "\nGCF: " + gcf.ToString();
                    Output.Text += "\nLCM: " + LCM(a, b).ToString();

                    // Common factors
                    {
                        var factors = CommonFactors(a, b);

                        int factorsPad1 = 0;
                        int factorsPad2 = 0;
                        int factorsPad3 = 0;
                        foreach (var f in factors)
                        {
                            factorsPad1 = Math.Max(f.Item1.ToString().Length, factorsPad1);
                            factorsPad2 = Math.Max(f.Item2.ToString().Length, factorsPad2);
                            factorsPad3 = Math.Max(f.Item3.ToString().Length, factorsPad3);
                        }
                        
                        Output.Text += "\nCommon Factors:\n" + string.Join("\n", from f in factors select
                            f.Item1.ToString().PadLeft(factorsPad1) + " × " +
                            f.Item2.ToString().PadLeft(factorsPad2) + ", " +
                            f.Item3.ToString().PadLeft(factorsPad3));
                    }
                }
            }
            catch
            {
                Output.Text = "...";
            }
        }

        private static List<(int, int)> Factors(int n)
        {
            var factors = new List<(int, int)>
            {
                (1, n)
            };

            if (n > 0) // Positive
            {
                for (int i = 2; i < (n / i); ++i)
                {
                    if (n % i != 0) { continue; }
                    factors.Add((i, n / i));
                }
            }
            else if (n < 0) // Negative
            {
                for (int i = 2; i < -(n / i); ++i)
                {
                    if (n % i != 0) { continue; }
                    factors.Add((i, n / i));
                }
            }

            return factors;
        }

        private static List<(int, int, int)> CommonFactors(int a, int b)
        {
            var factors = new List<(int, int, int)>();

            if (a < 0 || b < 0) // Not sure what to do in this case
            {
                return factors;
            }

            for (int i = 1; i <= Math.Min(a, b); ++i)
            {
                if (a % i != 0 || b % i != 0) { continue; }
                factors.Add((i, a / i, b / i));
            }

            return factors;
        }

        private static int GCF(int a, int b)
        {
            if (a <= 0 || b <= 0) // Not sure what to do in this case
            {
                throw new NotImplementedException();
            }

            for (int gcf = Math.Min(a, b); gcf > 1; --gcf)
            {
                if (a % gcf == 0 && b % gcf == 0)
                {
                    return gcf;
                }
            }

            return 1;
        }

        private static int LCM(int a, int b)
        {
            if (a <= 0 || b <= 0) // Not sure what to do in this case
            {
                throw new NotImplementedException();
            }

            int product = a * b;

            for (int lcm = Math.Max(a, b); lcm < product; ++lcm)
            {
                if (lcm % a == 0 && lcm % b == 0)
                {
                    return lcm;
                }
            }

            return product;
        }

        private static string SimplifiedRoot(int x)
        {
            var factors = Factors(x);
            factors.Reverse(); // largest to smallest
            foreach (var (a, b) in factors)
            {
                double rootB = Math.Sqrt(b);
                if (IsInt(rootB))
                {
                    return $"{(int)rootB}√{a}";
                }

                double rootA = Math.Sqrt(a);
                if (IsInt(rootA))
                {
                    return $"{(int)rootA}√{b}";
                }
            }

            return $"√{x}";
        }
    }
}
