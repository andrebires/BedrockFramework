using System;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public class FixMessage 
    {
        public FixMessage(FixField[] fields)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        public FixField[] Fields { get; }

        public override string ToString() => ToString('|');
        
        public string ToString(char separator) => string.Join(separator, Fields);
    }
}