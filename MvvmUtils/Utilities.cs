using System.Collections.Generic;
using System.Linq;

namespace MvvmUtils
{
    internal static class Utilities
    {
        internal static bool IsConsecutive( List<int> numbers )
        {
            numbers.Sort();
            var l = numbers.Distinct().ToList();

            return l.Last() - l.First() == l.Count - 1;
        }
    }
}