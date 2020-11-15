using System;
using System.Buffers;
using System.Buffers.Text;
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
                beginStringField.Tag != (int)Tags.BeginString ||
                !TryReadField(ref reader, out var bodyLengthField) ||
                bodyLengthField.Tag != (int)Tags.BodyLength)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            if (!Utf8Parser.TryParse(bodyLengthField.Value.Span, out int bodyLength, out var lengthConsumed) ||
                lengthConsumed < bodyLengthField.Value.Length)
            {
                examined = reader.Position;
                message = default;
                return false;
            }
            
            var body = input.Slice(reader.Consumed, bodyLength);
            reader.Advance(bodyLength);

            var bodyString = Encoding.ASCII.GetString(body.ToArray());

            // Calculate the checksum before consuming the field, which is not included in calculation
            var calculatedChecksum = ComputeAdditionChecksum(input.Slice(0, reader.Consumed));
            
            if (!TryReadField(ref reader, out var checksumField) ||
                checksumField.Tag != (int)Tags.CheckSum ||
                !Utf8Parser.TryParse(checksumField.Value.Span, out int checksum, out var checksumConsumed) ||
                checksumConsumed < checksumField.Value.Length ||
                checksum != calculatedChecksum)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            examined = reader.Position;
            consumed = reader.Position;
            message = new FixMessage();
            return true;
        }
        
        private static byte ComputeAdditionChecksum(ReadOnlySequence<byte> data)
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
        
        private static bool TryReadField(ref SequenceReader<byte> reader, out FixField fixField)
        {
            if (!reader.TryReadTo(out ReadOnlySpan<byte> tag, EQUAL) ||
                !Utf8Parser.TryParse(tag, out int tagNumber, out var consumed) ||
                consumed < tag.Length || 
                !reader.TryReadTo(out ReadOnlySequence<byte> value, SOH))
            {
                fixField = default;
                return false;
            }

            fixField = new FixField(tagNumber, value.ToMemory());
            return true;
        }
    }

    public readonly struct FixField
    {
        public FixField(int tag, ReadOnlyMemory<byte> value)
        {
            Tag = tag;
            Value = value;
        }

        public int Tag { get; }

        public ReadOnlyMemory<byte> Value { get; }

        public void Deconstruct(out int tag, out ReadOnlyMemory<byte> value)
        {
            tag = Tag;
            value = Value;
        }
    }

    public class FixMessage 
    {
        
    }
    

    /// <summary>
    /// Tags from 4.4 fix specification
    /// </summary>
    public enum Tags
    {
        BeginString = 8,
        BodyLength = 9,
        CheckSum = 10,
    }
}