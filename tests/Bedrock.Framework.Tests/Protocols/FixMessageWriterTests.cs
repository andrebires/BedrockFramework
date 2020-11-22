using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Bedrock.Framework.Experimental.Protocols.Fix;
using Bedrock.Framework.Infrastructure;
using Xunit;

namespace Bedrock.Framework.Tests.Protocols
{
    public class FixMessageWriterTests
    {
        [Fact]
        public void WriteLogonMessage()
        {
            // Arrange
            var message = new FixMessageBuilder()
                .AddField(Fix.V4_4.BeginString, "FIX.4.4")
                .AddField(Fix.V4_4.BodyLength, 75) // should be calculated
                .AddField(Fix.V4_4.MsgType, "A")
                .AddField(34, 1092)
                .AddField(49, "TESTBUY1")
                .AddField(52, "20180920-18:24:59.643")
                .AddField(56, "TESTSELL1")
                .AddField(98, 0)
                .AddField(108, 60)
                .AddField(Fix.V4_4.CheckSum, 178) // should be calculated
                .Build();
            var bufferWriter = new ArrayBufferWriter<byte>();
            var target = new FixMessageWriter();

            // Act
            target.WriteMessage(message, bufferWriter);
            
            // Assert

            Assert.Equal(97, bufferWriter.WrittenCount);
            Assert.Equal("8=FIX.4.49=7535=A34=109249=TESTBUY152=20180920-18:24:59.64356=TESTSELL198=0108=6010=178", Encoding.ASCII.GetString(bufferWriter.WrittenSpan));

        }
    }
}