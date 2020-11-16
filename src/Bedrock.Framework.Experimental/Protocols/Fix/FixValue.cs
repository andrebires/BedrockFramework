using System;
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Bedrock.Framework.Experimental.Protocols.Fix
{
    /// <summary>
    /// Represents a storage type for FIX values, optimized for the stack.
    /// </summary>
    public readonly struct FixValue
    {
        // The sentinels indicate which storage method is being used.
        private static readonly object _sentinelLong = new object();
        private static readonly object _sentinelDateTimeOffset = new object();
        private static readonly object _sentinelInt = new object();
        private static readonly object _sentinelDouble = new object();
        private static readonly object _sentinelChar = new object();
        private static readonly object _sentinelBoolean = new object();
        private static readonly object _sentinelRaw = new object();
        
        private readonly long _numberValue;
        private readonly object _objectOrSentinelValue;
        private readonly ReadOnlyMemory<byte> _rawValue;
        
        private FixValue(long numberValue, object objectOrSentinelValue, ReadOnlyMemory<byte> rawValue)
        {
            _numberValue = numberValue;
            _objectOrSentinelValue = objectOrSentinelValue;
            _rawValue = rawValue;
        }

        public FixValue(long value)
            : this(value, _sentinelLong, default)
        {
            
        }
        
        public FixValue(DateTimeOffset value)
            : this(value.Ticks, _sentinelDateTimeOffset, default)
        {
            
        }
        
        public FixValue(int value)
            : this(value, _sentinelInt, default)
        {
            
        }
        
        public FixValue(double value)
            : this(BitConverter.DoubleToInt64Bits(value), _sentinelDouble, default)
        {
            
        }
        
        public FixValue(char value)
            : this(value, _sentinelChar, default)
        {
            
        }

        public FixValue(bool value)
            : this(value ? 1 : 0, _sentinelBoolean, default)
        {
            
        }

        public FixValue(ReadOnlyMemory<byte> value)
            : this(default, _sentinelRaw, value)
        {
            
        }
        
        public FixValue(string value)
            : this(default, value, default)
        {
            
        }

        public override string ToString() => this;

        public static implicit operator string(FixValue value)
        {
            switch (value.Type)
            {
                case StorageType.Long:
                case StorageType.Int:
                    return value._numberValue.ToString(NumberFormatInfo.InvariantInfo);
                case StorageType.DateTimeOffset:
                    return new DateTimeOffset(value._numberValue, TimeSpan.Zero).ToString("O");
                case StorageType.Double:
                    return BitConverter.Int64BitsToDouble(value._numberValue).ToString(NumberFormatInfo.InvariantInfo);
                case StorageType.Char:
                    return ((char) value._numberValue).ToString(CultureInfo.InvariantCulture);
                case StorageType.Boolean:
                    return (value._numberValue != 0).ToString();
                case StorageType.String:
                    return (string) value._objectOrSentinelValue;
                case StorageType.Raw:
                    return Encoding.ASCII.GetString(value._rawValue.Span);
            }
            
            throw new NotSupportedException("Unsupported storage type");
        }
        
        public static implicit operator int(FixValue value) => checked((int)(long)value);
        
        public static implicit operator long(FixValue value)
        {
            switch (value.Type)
            {
                case StorageType.Long:
                case StorageType.Int:
                    return value._numberValue;
                case StorageType.Raw:
                    if (Utf8Parser.TryParse(value._rawValue.Span, out int parsed, out _))
                    {
                        return parsed;
                    } 
                    break;
            }
            
            throw new InvalidCastException($"Unable to cast from {value.Type} to long: '{value}'");
        }

        private StorageType Type
        {
            get
            {
                if (_objectOrSentinelValue == _sentinelLong) return StorageType.Long;
                if (_objectOrSentinelValue == _sentinelDateTimeOffset) return StorageType.DateTimeOffset;
                if (_objectOrSentinelValue == _sentinelInt) return StorageType.Int;
                if (_objectOrSentinelValue == _sentinelDouble) return StorageType.Double;
                if (_objectOrSentinelValue == _sentinelChar) return StorageType.Char;
                if (_objectOrSentinelValue == _sentinelBoolean) return StorageType.Boolean;
                if (_objectOrSentinelValue == _sentinelRaw) return StorageType.Raw;
                if (_objectOrSentinelValue is string) return StorageType.String;
                throw new InvalidOperationException("Unknown storage type");
            }
        }

        private enum StorageType
        {
            Long,
            DateTimeOffset,
            Int,
            Double,
            Char,
            Boolean,
            String,
            Raw
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