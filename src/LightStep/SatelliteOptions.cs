namespace LightStep
{
    public class SatelliteOptions
    {
        public string SatelliteHost { get; }
        public int SatellitePort { get; }
        public bool UsePlaintext { get; }

        public SatelliteOptions() : this("collector.lightstep.com", 443, false)
        {
            
        }

        public SatelliteOptions(string host, int port, bool usePlaintext)
        {
            SatelliteHost = host;
            SatellitePort = port;
            UsePlaintext = usePlaintext;
        }
    }
}