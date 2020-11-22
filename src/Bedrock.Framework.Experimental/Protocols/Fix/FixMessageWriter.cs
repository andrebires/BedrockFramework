using System;
using System.Buffers;
using Bedrock.Framework.Infrastructure;
using Bedrock.Framework.Protocols;
using static Bedrock.Framework.Experimental.Protocols.Fix.Constants;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public class FixMessageWriter : IMessageWriter<FixMessage>
    {
        public static readonly Memory<byte> SohMemory = new[] { SOH }; 
        public static readonly Memory<byte> EqualMemory = new[] { EQUAL };
        
        public void WriteMessage(FixMessage message, IBufferWriter<byte> output)
        {
            var writer = new BufferWriter<IBufferWriter<byte>>(output);

            for (int i = 0; i < message.Fields.Length; i++)
            {
                var field = message.Fields[i];

                if (i == 0 && 
                    field.Tag != Fix.V4_4.BeginString)
                {
                    throw new ArgumentException("Invalid Fix message");
                }
                
                writer.WriteNumeric((ulong)field.Tag);
                writer.Write(EqualMemory.Span);
                var memory = (ReadOnlyMemory<byte>) field.Value;
                writer.Write(memory.Span);
                writer.Write(SohMemory.Span);
            }
            
            writer.Commit();
        }
    }

    public static class Constants
    {
        public const byte SOH = 0x01;   // Start of Heading
        public const byte EQUAL = 0x3D; // Equal signal
    }
}