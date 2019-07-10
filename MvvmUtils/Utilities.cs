using System.Collections.Generic;
using System.Linq;

namespace MvvmUtils
{
    internal static class Utilities
    {
        internal static bool IsConsecutive( List<int> numbers )
        {
            numbers.Sort();

            return numbers.Last() - numbers[0] == numbers.Count - 1;
        }
    }
}