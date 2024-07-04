using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.Foundation;

namespace AlgebraBalancer.Algebra.Balancer;

public enum Operation
{
    //tex: $$a - b = c \to a = c + b$$
    Add,

    //tex: $$a + b = c \to a = c - b$$
    Sub,

    //tex: $$\frac{a}{b} = c \to a = bc$$
    Mul,

    //tex: $$ab = c \to a = \frac{c}{b} \iff b \ne 0$$
    Div,

    //tex: $$\sqrt[b]{a} = c \to a = c^b$$
    Pow,

    //tex: $$a^b = c \to a = \pm\sqrt[b]{c}$$
    Root,

    //tex: $$b^a = c \to a = \log_b{c}$$
    Log,

    //tex: $$e^a = b \to a = \ln{b}$$
    Ln,

    //tex: $$\ln a = b \to a = e^b$$
    Exp,

    //tex: $$\lvert a \rvert = b \to a = \pm b$$
    Sign,

    //tex: $$ax^2 + bx + c = 0 \to x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a} \iff a \ne 0$$
    Quadratic,
}

public enum Refactor
{
    //tex: $$\begin{aligned}
    //ab + cb &\to b(a + c) \\
    //ab^2 + cb &\to b(ab + c)
    //\end{aligned}$$
    Factor,

    //tex:
    //$$\begin{aligned}
    //(a+b)(c+d) &\to ac + ad + bc + bd \\
    //(a+b)^2 &\to a^2 + 2ab + b^2 \\
    //\end{aligned}$$
    FOIL,
}

public interface RelationshipItem { }

public struct RelSide(string value) : RelationshipItem
{
    public string value = value;
    public static implicit operator string(RelSide side) => side.value;
    public static implicit operator RelSide(string value) => new(value);
}

public enum Comparator
{
    EQ, // =
    NE, // !=
    GT, // >
    GE, // >=
    LT, // <
    LE, // <=
    QE, // ?=
}

public struct RelComp(Comparator value) : RelationshipItem
{
    public static readonly Regex reCmp = new(@"(&\s*)?(?:[≠≥≤≟]|(?:==)|(?:!=)|(?:>=)|(?:<=)|(?:\?=)|(?:(?<![!?>=<])=(?!=))|(?:>(?!=))|(?:<(?!=)))(\s*&)?");

    public Comparator value = value;
    public static implicit operator Comparator(RelComp cmp) => cmp.value;
    public static implicit operator RelComp(Comparator value) => new(value);

    public static RelComp FromString(string str) => new(str switch
    {
        "=" or "==" => Comparator.EQ,
        "≠" or "!=" => Comparator.NE,
        ">" => Comparator.GT,
        "≥" or ">=" => Comparator.GE,
        "<" => Comparator.LT,
        "≤" or "<=" => Comparator.LE,
        "≟" or "?=" => Comparator.QE,
        _ => throw new NotImplementedException($"The comparator \"{str}\" is not recognized"),
    });
}

public class Relationship
{
    private static readonly Regex reAroundCmp = new(@$"(?={RelComp.reCmp}\s*)|(?<={RelComp.reCmp})");
    public static Relationship Parse(string str)
    {
        string[] items = reAroundCmp.Split(str.Replace("&", ""))
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        Debug.Assert(items.Length > 1 && items.Length % 2 == 1);
        return new Relationship(items);
    }

    public Relationship(params object[] items)
    {
        Debug.Assert(items.Length > 1 && items.Length % 2 == 1);
        this.items = new RelationshipItem[items.Length];
        for (int i = 0; i < items.Length; ++i)
        {
            object item = items[i];
            if (i % 2 == 0)
            {
                if (item is RelSide side)
                {
                    this.items[i] = side;
                }
                else if (item is string sideStr)
                {
                    this.items[i] = new RelSide(sideStr);
                }
                else
                {
                    Debug.Assert(false, $"Expected item {i} to be RelSide");
                }
            }
            else
            {
                if (item is RelComp cmp)
                {
                    this.items[i] = cmp;
                }
                else if (item is Comparator cmpEnum)
                {
                    this.items[i] = new RelComp(cmpEnum);
                }
                else if (item is string cmpStr)
                {
                    this.items[i] = RelComp.FromString(cmpStr);
                }
                else
                {
                    Debug.Assert(false, $"Expected item {i} to be RelComp");
                }
            }
        }
    }

    public RelationshipItem[] items;

    public int NumSides => items.Length / 2;

    private IEnumerable<string> Sides
    {
        get => items
            .Where(x => x is RelSide)
            .Select(x => (string)(RelSide)x);

        set
        {
            Debug.Assert(value.Count() == NumSides);
            for (int i = 0; i < value.Count(); ++i)
            {
                items[i * 2] = new RelSide(value.ElementAt(i));
            }
        }
    }

    public void ApplyOperation(Operation operation, string value)
    {
        switch (operation)
        {
            case Operation.Add:
                Sides = Sides.Select(x => $"{x} + {value}");
                break;
            case Operation.Sub:
                break;
            case Operation.Mul:
                break;
            case Operation.Div:
                break;
            case Operation.Pow:
                break;
            case Operation.Root:
                break;
            case Operation.Log:
                break;
            case Operation.Ln:
                break;
            case Operation.Exp:
                break;
            case Operation.Sign:
                break;
            case Operation.Quadratic:
                break;

            default: throw new NotImplementedException();
        }
    }

    public void ApplyRefactor(Refactor refactor)
    {
        switch (refactor)
        {
            case Refactor.Factor:
                break;
            case Refactor.FOIL:
                break;

            default: throw new NotImplementedException();
        }
    }
}
