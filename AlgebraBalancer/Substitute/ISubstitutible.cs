using System.Text.RegularExpressions;

namespace AlgebraBalancer.Substitute;
public interface ISubstitutible
{
    public string Name { get; }
    public string GetReplacement(string capture, Substitutor substitutor, int maxDepth = 20);
    public Regex GetRegex();
}
