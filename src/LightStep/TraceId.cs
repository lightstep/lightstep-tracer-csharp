using System;

namespace LightStep
{
    internal struct TraceId
    {
        internal ulong Upper { get; set; }
        internal ulong? Lower { get; set; }

        internal TraceId(ulong upper, ulong? lower = null)
        {
            Upper = upper;
            Lower = lower;
        }

        internal static TraceId Parse(string traceId)
        {
            ulong upper;
            ulong? lower = null;

            if (ContainsHexChar(traceId))
            {
                if (traceId.Length <= 16)
                {
                    upper = Convert.ToUInt64(traceId, 16);
                }
                else
                {
                    upper = Convert.ToUInt64(traceId.Substring(0, 16), 16);
                    lower = Convert.ToUInt64(traceId.Substring(16), 16);
                }
            }
            else
            {
                if (traceId.Length <= 20)
                {
                    upper = Convert.ToUInt64(traceId);
                }
                else
                {
                    upper = Convert.ToUInt64(traceId.Substring(0, 20));
                    lower = Convert.ToUInt64(traceId.Substring(20));
                }
            }

            return new TraceId(upper, lower);
        }

        private static bool ContainsHexChar(string traceId)
        {
            foreach (char c in traceId)
            {
                if (!char.IsDigit(c))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Concat(
                Upper.ToString(),
                Lower.HasValue ? Lower.Value.ToString() : string.Empty
            );
        }

        public string ToString(string format)
        {
            return string.Concat(
                Upper.ToString(format),
                Lower.HasValue ? Lower.Value.ToString(format) : string.Empty
            );
        }
    }
}