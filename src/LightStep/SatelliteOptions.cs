namespace LightStep
{
    /// <summary>
    ///     Contains information on a LightStep Satellite Endpoint.
    /// </summary>
    public class SatelliteOptions
    {
        /// <summary>
        ///     Create a new Satellite Endpoint.
        /// </summary>
        /// <param name="host">satellite hostname</param>
        /// <param name="port">satellite port</param>
        /// <param name="usePlaintext">unused</param>
        public SatelliteOptions(string host, int port = 443, bool usePlaintext = false)
        {
            SatelliteHost = host;
            SatellitePort = port;
            UsePlaintext = usePlaintext;
        }

        /// <summary>
        ///     Hostname of a Satellite
        /// </summary>
        public string SatelliteHost { get; }

        /// <summary>
        ///     Port number of a Satellite
        /// </summary>
        public int SatellitePort { get; }

        /// <summary>
        ///     Currently unused
        /// </summary>
        public bool UsePlaintext { get; }
    }
}