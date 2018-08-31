using System;
using Google.Protobuf;

namespace LightStep.Collector
{
    /// <summary>
    /// Helper methods for the ProtoConverter
    /// </summary>
    public static class ProtoConverterExtensions
    {
        /// <summary>
        /// Extension method to determine if an object is a number or number-like value.
        /// </summary>
        /// <param name="value">Any value.</param>
        /// <returns>True if the value is a number or number-like value, false otherwise.</returns>
        public static bool IsNumericDataType(this object value)
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

        /// <summary>
        /// Extension method to determine if an object is a boolean value.
        /// </summary>
        /// <param name="value">Any value.</param>
        /// <returns>True is the value is a boolean value, false otherwise.</returns>
        public static bool IsBooleanDataType(this object value)
        {
            return value is bool;
        }
    }
}
