namespace LightStep
{
    public class SatelliteOptions
    {
        public string SatelliteHost { get; }
        public int SatellitePort { get; }
        public bool UsePlaintext { get; }

        public SatelliteOptions(string host, int port = 443, bool usePlaintext = false)
        {
            SatelliteHost = host;
            SatellitePort = port;
            UsePlaintext = usePlaintext;
        }
    }
}
