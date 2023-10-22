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

using static System.Net.Mime.MediaTypeNames;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlgebraBalancer
{
    struct FactorPair
    {
        public int a;
        public int b;

        public FactorPair(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Update(object sender, TextChangedEventArgs args)
        {
            try
            {
                string aText = InputA.Text;
                string bText = InputB.Text;

                if (aText.Length > 10 || bText.Length > 10)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (aText == string.Empty || bText == string.Empty) // Unary
                {
                    string text = aText == string.Empty ? bText : aText;
                    int x = int.Parse(text);

                    var factors = Factors(x);
                    int factorsPad1 = 0;
                    int factorsPad2 = 0;
                    foreach (var f in factors)
                    {
                        factorsPad1 = Math.Max(f.a.ToString().Length, factorsPad1);
                        factorsPad2 = Math.Max(f.b.ToString().Length, factorsPad2);
                    }

                    OutputFactors.Text = string.Join("\n",
                        from f in factors
                        select f.a.ToString().PadLeft(factorsPad1) + " × " + f.b.ToString().PadLeft(factorsPad2));
                }
                else // Binary
                {
                    int a = int.Parse(aText);
                    int b = int.Parse(bText);
                }
            }
            catch
            {
                OutputFactors.Text = "...";
            }
        }

        private List<FactorPair> Factors(int n)
        {
            var factors = new List<FactorPair>
            {
                new FactorPair(1, n)
            };

            if (n > 0) // Positive
            {
                for (int i = 2; i < (n / i); ++i)
                {
                    if (n % i != 0) { continue; }
                    factors.Add(new FactorPair(i, n / i));
                }
            }
            else if (n < 0) // Negative
            {
                for (int i = 2; i < -(n / i); ++i)
                {
                    if (n % i != 0) { continue; }
                    factors.Add(new FactorPair(i, n / i));
                }
            }
            return factors;
        }
    }
}
