using System.Collections.Generic;

using Xunit;

namespace NippyWard.Text.Tests
{
    public class RFC1459StringComparerTests
    {
        private readonly string _lowerCaseString = @"{test}|";
        private readonly string _upperCaseString = @"[TEST]\";
        private readonly IEqualityComparer<Utf8String> _comparer
            = new RFC1459StringComparer();

        [Fact]
        public void EqualityTest()
        {
            Utf8String lStr = new Utf8String(this._lowerCaseString);
            Utf8String rStr = new Utf8String(this._upperCaseString);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            Assert.True(this._comparer.Equals(lStr, rStr));
        }


        [Fact]
        public void HashingTest()
        {
            HashSet<Utf8String> hash = new HashSet<Utf8String>
            (
                this._comparer
            );

            Utf8String str = new Utf8String(this._lowerCaseString);

            Assert.NotNull(str);
            Assert.True(hash.Add(str));

            Utf8String other = new Utf8String(this._upperCaseString);

            Assert.NotNull(other);
            Assert.False(hash.Add(other));
        }
    }
}
