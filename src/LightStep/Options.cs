using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTracing.Tag;

namespace LightStep
{
    public class Options
    {        
        public string AccessToken { get; set; }
        public (string, int, bool) Satellite { get; set; }
        public TimeSpan MinimumReportPeriod { get; set; }
        public TimeSpan MaximumReportPeriod { get; set; }
        public TimeSpan ReportTimeout { get; set; }
        public IDictionary<string, object> Tags { get; set; }       
        public readonly UInt64 TracerGuid = new Random().NextUInt64();

        public Options(string token, (string, int, bool)? satellite = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            
            Tags = InitializeDefaultTags();
            MinimumReportPeriod = TimeSpan.FromMilliseconds(500);
            MaximumReportPeriod = TimeSpan.FromMilliseconds(2500);
            ReportTimeout = TimeSpan.FromSeconds(30);
            Satellite = satellite.HasValue ? 
                (satellite.Value.Item1, satellite.Value.Item2, satellite.Value.Item3) : 
                ("collector.lightstep.com", 443, false);
            AccessToken = token;
        }

        private IDictionary<string, object> InitializeDefaultTags()
        {
            var attributes = new Dictionary<string, object>
            {
                [LightStepConstants.TracerPlatformKey] = LightStepConstants.TracerPlatformValue,
                [LightStepConstants.TracerPlatformVersionKey] = "0.1",
                [LightStepConstants.TracerVersionKey] = "0.1",
                [LightStepConstants.ComponentNameKey] = Process.GetCurrentProcess().ProcessName,
                [LightStepConstants.HostnameKey] = Environment.MachineName,
                [LightStepConstants.CommandLineKey] = Environment.CommandLine
                
            };
            return attributes;
        }
    }
}