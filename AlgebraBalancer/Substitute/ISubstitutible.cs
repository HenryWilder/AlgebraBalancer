using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBalancer.Substitute;
public interface ISubstitutible
{
    public string Name { get; }
    public string GetReplacement(string capture);
    public Regex GetRegex();
}
