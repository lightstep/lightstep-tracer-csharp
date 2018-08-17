using System;

namespace LightStep
{
    public static class Utilities
    {
        public static UInt64 NextUInt64(this Random rand)
        {
            var buffer = new byte[sizeof(UInt64)];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}