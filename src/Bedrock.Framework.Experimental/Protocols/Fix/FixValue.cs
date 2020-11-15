using System;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    public readonly struct FixValue
    {
        private static readonly object _sentinelLong = new object();
        private static readonly object _sentinelDateTimeOffset = new object();
        private static readonly object _sentinelInt = new object();
        private static readonly object _sentinelDouble = new object();
        private static readonly object _sentinelChar = new object();
        private static readonly object _sentinelBoolean = new object();
        
        private readonly long _numberValue;
        private readonly object _objectOrSentinelValue;
        
        private FixValue(long numberValue, object objectOrSentinelValue)
        {
            _numberValue = numberValue;
            _objectOrSentinelValue = objectOrSentinelValue;
        }

        public FixValue(long value)
            : this(value, _sentinelLong)
        {
            
        }
        
        public FixValue(DateTimeOffset value)
            : this(value.Ticks, _sentinelDateTimeOffset)
        {
            
        }
        
        public FixValue(int value)
            : this(value, _sentinelInt)
        {
            
        }
        
        public FixValue(double value)
            : this(BitConverter.DoubleToInt64Bits(value), _sentinelDouble)
        {
            
        }
        
        public FixValue(char value)
            : this(value, _sentinelChar)
        {
            
        }

        public FixValue(bool value)
            : this(value ? 1 : 0, _sentinelBoolean)
        {
            
        }

        public FixValue(string value)
            : this(default, value)
        {
            
        }

        public FixDataType DataType
        {
            get
            {
                if (_objectOrSentinelValue == _sentinelInt) return FixDataType.Int;
                if (_objectOrSentinelValue == _sentinelDouble) return FixDataType.Float;
                if (_objectOrSentinelValue == _sentinelChar) return FixDataType.Char;
                if (_objectOrSentinelValue is string) return FixDataType.String;
                throw new InvalidOperationException("Unknown data type");
            }
        }

    }

    /// <summary>
    /// https://btobits.com/fixopaedia/fixdic44/index.html?tag_44_Price.html
    /// </summary>
    public enum FixDataType
    {
        Int,
        Float,
        Char,
        String,
        Data
    }
}