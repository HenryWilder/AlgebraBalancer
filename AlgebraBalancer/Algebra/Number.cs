using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;
public struct Number(int value) : IAlgebraicAtomic
{
    public int value = value;

    public readonly bool IsInoperable => false;

    public static implicit operator int(Number num) => num.value;
    public static implicit operator Number(int value) => new(value);
    public override readonly string ToString() => $"{value}";
    public readonly string AsEquality(string lhs) => $"{lhs} = {ToString()}";

    public static Number operator -(Number rhs) => new(-rhs.value);

    public readonly Number Squared() => (Number)(value * value);
}
