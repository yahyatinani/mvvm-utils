using System.Collections.Generic;
using MvvmUtils;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace UnitTests
{
    [TestFixture]
    public class UtilitiesTest
    {
        [Test]
        public void IsConsecutive()
        {
            IsTrue( Utilities.IsConsecutive( new List<int> { 1 } ) );
            IsTrue( Utilities.IsConsecutive( new List<int> { 1, 2 } ) );
            IsTrue( Utilities.IsConsecutive( new List<int> { 1, 2, 3 } ) );
            IsTrue( Utilities.IsConsecutive( new List<int> { 1, 2, 3, 4 } ) );
            IsFalse( Utilities.IsConsecutive( new List<int> { 1, 2, 3, 5 } ) );
            IsFalse( Utilities.IsConsecutive( new List<int> { 1, 6, 3, 4 } ) );
            IsTrue( Utilities.IsConsecutive( new List<int> { 4, 5, 1, 3, 2 } ) );
            IsTrue( Utilities.IsConsecutive( new List<int> { 3, 4, 5, 6, 7, 8, 9, 10 } ) );
        }
    }
}