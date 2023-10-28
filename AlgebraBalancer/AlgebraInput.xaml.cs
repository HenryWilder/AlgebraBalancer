using System;
using System.Collections.Generic;
using System.Data;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AlgebraBalancer
{
    public sealed partial class AlgebraInput : UserControl
    {
        public AlgebraInput() =>
            InitializeComponent();

        private readonly static DataTable dt = new DataTable();

        // Null: no value (skip)
        // StackOverflowException: cannot calculate
        // int: valid value
        public int? Value
        {
            get
            {
                string text = Input.Text;
                if (text.Length == 0)
                {
                    return null;
                }
                else if (text.Length > 7)
                {
                    throw new StackOverflowException();
                }
                return (int)dt.Compute(text, "");
            }
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                EchoInput.Text = Value.HasValue ? Value.Value.ToString() : string.Empty;
            }
            catch
            {
                EchoInput.Text = "...";
            }
        }
    }
}
