using System;
using System.Buffers;
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



        }
    }
}