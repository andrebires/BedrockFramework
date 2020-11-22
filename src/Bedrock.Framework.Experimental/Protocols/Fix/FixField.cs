using System;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public readonly struct FixField
    {
        public FixField(int tag, FixValue value)
        {
            Tag = tag;
            Value = value;
        }

        public int Tag { get; }

        public FixValue Value { get; }

        public void Deconstruct(out int tag, out FixValue value)
        {
            tag = Tag;
            value = Value;
        }

        public override string ToString() => $"{Tag}={Value}";
    }
}