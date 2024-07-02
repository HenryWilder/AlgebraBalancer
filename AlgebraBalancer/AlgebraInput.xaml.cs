using System;
using System.Data;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AlgebraBalancer;

public sealed partial class AlgebraInput : UserControl
{
    public AlgebraInput() =>
        InitializeComponent();

    public AlgebraInput(string header)
    {
        InitializeComponent();
        Input.Header = header;
    }

    private readonly static DataTable dt = new();

    // Null: no value (skip)
    // StackOverflowException: cannot calculate
    // int: valid value
    public int? GetValue()
    {
        string text = Input.Text;
        if (text.Length == 0)
        {
            return null;
        }
        else if (text.Length > Input.MaxLength)
        {
            throw new StackOverflowException();
        }
        return (int)dt.Compute(text, "");
    }

    public void SetValue(string value)
    {
        Input.Text = value;
    }

    private void Input_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            int? value = GetValue();
            (EchoInput.Text, EchoInput.Opacity) = value.HasValue
                ? (value.Value.ToString(), 1.0)
                : ("∅", 0.5);
        }
        catch
        {
            EchoInput.Opacity = 1.0;
            EchoInput.Text = "...";
        }
    }
}
