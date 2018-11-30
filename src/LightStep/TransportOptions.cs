using System;

namespace LightStep
{
    /// <summary>
    /// Options for the transport used to send spans to LightStep.
    /// </summary>
    [Flags]
    public enum TransportOptions
    {
        /// <summary>
        /// Binary protobuf encoding over HTTP
        /// </summary>
        BinaryProto,
        /// <summary>
        /// JSON protobuf encoding over HTTP
        /// </summary>
        JsonProto
    }
}