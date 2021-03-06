﻿using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;
using Bedrock.Framework.Infrastructure;
using Bedrock.Framework.Protocols;
using static Bedrock.Framework.Experimental.Protocols.Fix.Constants;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public class FixMessageReader : IMessageReader<FixMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined,
            out FixMessage message)
        {
            var messageBuilder = new FixMessageBuilder();
            
            var reader = new SequenceReader<byte>(input);

            if (!TryReadField(in reader, out var beginStringField) ||
                beginStringField.Tag != Fix.V4_4.BeginString ||
                !TryReadField(in reader, out var bodyLengthField) ||
                bodyLengthField.Tag != Fix.V4_4.BodyLength)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            messageBuilder.AddField(beginStringField.Tag, Encoding.ASCII.GetString(beginStringField.Value.ToSpan()));

            if (!Utf8Parser.TryParse(bodyLengthField.Value.ToSpan(), out int bodyLength, out var lengthConsumed) ||
                lengthConsumed < bodyLengthField.Value.Length)
            {
                examined = reader.Position;
                message = default;
                return false;
            }

            messageBuilder.AddField(bodyLengthField.Tag, bodyLength);
            
            var body = input.Slice(reader.Consumed, bodyLength);
            reader.Advance(bodyLength);

            #if DEBUG
            var bodyString = Encoding.ASCII.GetString(body.ToArray());
            #endif

            // Calculate the checksum before consuming the field, which is not included in calculation
            var calculatedChecksum = ComputeChecksum(input.Slice(0, reader.Consumed));
            
            if (!TryReadField(in reader, out var checksumField) ||
                checksumField.Tag != Fix.V4_4.CheckSum ||
                !Utf8Parser.TryParse(checksumField.Value.ToSpan(), out int checksum, out var checksumConsumed) ||
                checksumConsumed < checksumField.Value.Length ||
                checksum != calculatedChecksum)
            {
                examined = reader.Position;
                message = default;
                return false;
            }
            
            var bodyReader = new SequenceReader<byte>(body);
            while (TryReadField(in bodyReader, out var field))
            {
                // TODO: Instead of allocating for all body fields (ToArray), check the tag type and use the correct constructor.
                messageBuilder.AddField(field.Tag, field.Value.ToArray());
            }

            messageBuilder.AddField(checksumField.Tag, checksum);
            
            examined = reader.Position;
            consumed = reader.Position;

            message = messageBuilder.Build();
            return true;
        }
        
        private static bool TryReadField(in SequenceReader<byte> reader, out (int Tag, ReadOnlySequence<byte> Value) field)
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
}