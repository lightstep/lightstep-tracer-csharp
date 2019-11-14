using Xunit;

namespace LightStep.Tests
{
    public class TraceIdTests
    {
        [Theory]
        [InlineData("12345", 12345, null)] // int based 64 bit
        [InlineData("12345ef", 19088879, null)] // hex based 64 bit
        [InlineData("1844674407370955161518446744073709551615", 18446744073709551615, 18446744073709551615)] // int based 128 bit
        [InlineData("aef5705a090040838f1359ebafa5c0c6", 12607106263893950595, 10309682840780259526)] // hex based 128 bit
        public void CanParseCorrectly(string traceId, ulong expectedUpper, ulong? expectedLower)
        {
            var t = TraceId.Parse(traceId);

            Assert.Equal(expectedUpper, t.Upper);
            Assert.Equal(expectedLower, t.Lower);
            Assert.Equal(string.Concat(expectedUpper, expectedLower.HasValue ? expectedLower.ToString() : string.Empty), t.ToString());
            Assert.Equal(string.Concat(expectedUpper.ToString("x"), expectedLower.HasValue ? expectedLower.Value.ToString("x") : string.Empty), t.ToString("x"));
        }
    }
}
