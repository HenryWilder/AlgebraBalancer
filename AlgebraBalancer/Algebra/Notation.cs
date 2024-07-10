namespace AlgebraBalancer.Notation;

public interface IAlgebraicNotation
{
    public bool IsInoperable { get; }
    public string AsEquality(string lhs);
}

public interface IAlgebraicExpression : IAlgebraicNotation
{
    public IAlgebraicNotation Simplified();
}

public interface IAlgebraicAtomic : IAlgebraicNotation { }

public struct Undefined : IAlgebraicAtomic
{
    public readonly bool IsInoperable => true;
    public override readonly string ToString() => "∄";
    public readonly string AsEquality(string lhs) => $"∄{lhs}";
}

public struct Huge : IAlgebraicAtomic
{
    public readonly bool IsInoperable => true;
    public override readonly string ToString() => "𝑥 : |𝑥|≥2³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≥ 2³²";
}

public struct Tiny : IAlgebraicAtomic
{
    public readonly bool IsInoperable => true;
    public override readonly string ToString() => "𝑥 : |𝑥|≤2⁻³²";
    public readonly string AsEquality(string lhs) => $"|{lhs}| ≤ 2⁻³²";
}

public struct NotEnoughInfo : IAlgebraicAtomic
{
    public readonly bool IsInoperable => true;
    public override readonly string ToString() => "?";
    public readonly string AsEquality(string lhs) => $"{lhs} = ?";
}

/// <summary>
/// "No hair"
/// Not really constants, but instances are indistinguishable from eachother and so can be treated like constants.
/// </summary>
public static class Bald
{
    public static readonly Undefined UNDEFINED = new();
    public static readonly Huge HUGE = new();
    public static readonly Tiny TINY = new();
    public static readonly NotEnoughInfo NOT_ENOUGH_INFO = new();
}
