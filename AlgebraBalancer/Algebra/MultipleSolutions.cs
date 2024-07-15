using AlgebraBalancer.Notation;

namespace AlgebraBalancer.Algebra;

public class MultipleSolutions(params IAlgebraicNotation[] solutions) : IAlgebraicNotation
{
    public IAlgebraicNotation[] solutions = solutions;

    public override string ToString() => (solutions.Length == 2
        ? $"{solutions[0]} or {solutions[1]}"
        : string.Join<IAlgebraicNotation>(", ", solutions));

    public string AsEquality(string lhs) => $"{lhs}={ToString()}";

    public bool IsInoperable => false;
}
