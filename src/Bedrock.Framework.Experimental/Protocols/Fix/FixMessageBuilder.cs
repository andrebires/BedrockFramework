using System;
using System.Collections.Generic;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
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
        
        public FixMessageBuilder AddField(int tag, string value)
        {
            _fields.Add(new FixField(tag, new FixValue(value)));
            return this;
        }
        
        public FixMessageBuilder AddField(int tag, ReadOnlyMemory<byte> value)
        {
            _fields.Add(new FixField(tag, new FixValue(value)));
            return this;
        }
        
        public FixMessage Build() => new FixMessage(_fields.ToArray());
    }
}