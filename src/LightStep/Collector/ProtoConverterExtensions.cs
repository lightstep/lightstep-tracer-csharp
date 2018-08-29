using System;
using Google.Protobuf;

namespace LightStep.Collector
{
    public static class ProtoConverterExtensions
    {
        public static bool IsNumericDatatype(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        public static bool IsBooleanDatatype(this object value)
        {
            return value is bool;
        }
    }
}
