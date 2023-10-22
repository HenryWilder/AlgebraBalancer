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

        private void Factor(object sender, TextChangedEventArgs args)
        {
            string input = InputBox.Text;
            try
            {
                int n = int.Parse(input);
                var factors = new List<FactorPair>
                {
                    new FactorPair(1, n)
                };

                if (n > 0) // Positive
                {
                    for (int i = 2; i < (n / i); ++i)
                    {
                        factors.Add(new FactorPair(i, n / i));
                    }
                }
                else if (n < 0) // Negative
                {
                    for (int i = 2; i < -(n / i); ++i)
                    {
                        factors.Add(new FactorPair(i, n / i));
                    }
                }

                string leftText  = factors[0].a.ToString();
                string timesText = "×";
                string rightText = factors[0].b.ToString();
                for (int i = 1; i < factors.Count(); ++i)
                {
                    leftText  += "\n" + factors[i].a.ToString();
                    timesText += "\n×";
                    rightText += "\n" + factors[i].b.ToString();
                }

                ResultBoxLeft .Text = leftText;
                ResultBoxTimes.Text = timesText;
                ResultBoxRight.Text = rightText;
            }
            catch (FormatException)
            {
                ResultBoxLeft .Text = "...";
                ResultBoxTimes.Text = "×";
                ResultBoxRight.Text = "...";
            }
        }
    }
}
