using Pharos.Core.Utilities;
using System.Text;
using Xunit;

namespace Pharos.Core.Tests.Utilities
{
    public class HashHelperTests
    {
        [Fact]
        public void ComputeSha256_String_ReturnsExpectedHash()
        {
            // Arrange
            string input = "hello world";
            // SHA256 of "hello world"
            string expected = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";

            // Act
            string result = HashHelper.ComputeSha256(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ComputeSha256_Bytes_ReturnsExpectedHash()
        {
            // Arrange
            byte[] input = Encoding.UTF8.GetBytes("hello world");
            string expected = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";

            // Act
            string result = HashHelper.ComputeSha256(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ComputeHmacSha256_ReturnsExpectedHmac()
        {
            // Arrange
            string message = "hello world";
            string secret = "secret";
            // HMAC-SHA256 of "hello world" with key "secret"
            string expected = "734CC62F32841568F45715AEB9F4D7891324E6D948E4C6C60C0621CDAC48623A";

            // Act
            string result = HashHelper.ComputeHmacSha256(message, secret);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
