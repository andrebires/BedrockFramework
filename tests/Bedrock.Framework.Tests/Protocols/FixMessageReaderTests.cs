using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Bedrock.Framework.Experimental.Protocols.Fix;
using Xunit;

namespace Bedrock.Framework.Tests.Protocols
{
    public class FixMessageReaderTests
    {
        [Fact]
        public void ParseLogonMessage()
        {
            // Arrange
            var rawMessage = Encoding.ASCII.GetBytes("8=FIX.4.49=7535=A34=109249=TESTBUY152=20180920-18:24:59.64356=TESTSELL198=0108=6010=178");
            var sequence = new ReadOnlySequence<byte>(rawMessage);
            var reader = new FixMessageReader();

            var consumed = new SequencePosition();
            var examined = new SequencePosition();
            
            // Act
            var result = reader.TryParseMessage(sequence, ref consumed, ref examined, out var fixMessage);
            
            // Assert
            Assert.True(result);
            Assert.Equal(10, fixMessage.Fields.Length);
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 8));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 9));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 35));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 34));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 49));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 52));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 56));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 98));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 108));
            Assert.Single(fixMessage.Fields.Where(f => f.Tag == 10));
            Assert.Equal("FIX.4.4", fixMessage.Fields.First(f => f.Tag == 8).Value);
            Assert.Equal(75, fixMessage.Fields.First(f => f.Tag == 9).Value);
            Assert.Equal("A", fixMessage.Fields.First(f => f.Tag == 35).Value);
            Assert.Equal(1092, fixMessage.Fields.First(f => f.Tag == 34).Value);
            Assert.Equal("TESTBUY1", fixMessage.Fields.First(f => f.Tag == 49).Value);
        }
    }
}