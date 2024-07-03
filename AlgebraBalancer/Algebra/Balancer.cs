using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraBalancer.Algebra;
internal static class Balancer
{
    public enum Operation
    {
        // Both sides

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

        // One side

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

    public class Relationship
    {
        readonly string[] sides = [];
    }
}
