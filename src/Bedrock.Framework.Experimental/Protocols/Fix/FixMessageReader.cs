using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using Bedrock.Framework.Infrastructure;
using Bedrock.Framework.Protocols;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public class FixMessageReader : IMessageReader<FixMessage>
    {
        public static readonly byte SOH = 0x01;   // Start of Heading
        public static readonly byte EQUAL = 0x3D; // Equal signal
        
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined,
            out FixMessage message)
        {
            var reader = new SequenceReader<byte>(input);

            if (!TryReadField(ref reader, out var beginStringField) ||
                beginStringField.Tag != (int)FixTag.BeginString ||
                !TryReadField(ref reader, out var bodyLengthField) ||
                bodyLengthField.Tag != (int)FixTag.BodyLength)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            if (!Utf8Parser.TryParse(bodyLengthField.Value.ToSpan(), out int bodyLength, out var lengthConsumed) ||
                lengthConsumed < bodyLengthField.Value.Length)
            {
                examined = reader.Position;
                message = default;
                return false;
            }
            
            var body = input.Slice(reader.Consumed, bodyLength);
            reader.Advance(bodyLength);

            //var bodyString = Encoding.ASCII.GetString(body.ToArray());

            // Calculate the checksum before consuming the field, which is not included in calculation
            var calculatedChecksum = ComputeChecksum(input.Slice(0, reader.Consumed));
            
            if (!TryReadField(ref reader, out var checksumField) ||
                checksumField.Tag != (int)FixTag.CheckSum ||
                !Utf8Parser.TryParse(checksumField.Value.ToSpan(), out int checksum, out var checksumConsumed) ||
                checksumConsumed < checksumField.Value.Length ||
                checksum != calculatedChecksum)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            examined = reader.Position;
            consumed = reader.Position;

            var messageBuilder = new FixMessageBuilder();
                //.AddField(beginStringField.Tag, beginStringField.Value);

            message = messageBuilder.Build();
            return true;
        }
        
        private static bool TryReadField(ref SequenceReader<byte> reader, out (int Tag, ReadOnlySequence<byte> Value) field)
        {
            if (!reader.TryReadTo(out ReadOnlySpan<byte> tag, EQUAL) ||
                !Utf8Parser.TryParse(tag, out int tagNumber, out var consumed) ||
                consumed < tag.Length || 
                !reader.TryReadTo(out ReadOnlySequence<byte> value, SOH))
            {
                field = default;
                return false;
            }

            field = (tagNumber, value);
            return true;
        }
        
        private static byte ComputeChecksum(ReadOnlySequence<byte> data)
        {
            byte sum = 0;
            unchecked // Let overflow occur without exceptions
            {
                foreach (var memory in data)
                {
                    foreach (byte b in memory.Span) 
                    {
                        sum += b;
                    }
                }
            }
            return sum;
        }
    }

    public class FixMessageBuilder
    {
        private readonly List<FixField> _fields;

        public FixMessageBuilder()
        {
            _fields = new List<FixField>(50);
        }

        public FixMessageBuilder AddField(int tag, int value)
        {
            _fields.Add(new FixField(tag, new FixValue(value)));
            return this;
        }
        
        public FixMessage Build() => new FixMessage(_fields.ToArray());
    }
    
}