using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LightStep
{
    public class Options
    {        
        public string AccessToken { get; set; }
        public SatelliteOptions Satellite { get; set; }
        public TimeSpan MinimumReportPeriod { get; set; }
        public TimeSpan MaximumReportPeriod { get; set; }
        public TimeSpan ReportTimeout { get; set; }
        public IDictionary<string, object> Tags { get; set; }       
        public readonly UInt64 TracerGuid = new Random().NextUInt64();

        public Options(string token, SatelliteOptions satelliteOptions)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            
            Tags = InitializeDefaultTags();
            MinimumReportPeriod = TimeSpan.FromMilliseconds(500);
            MaximumReportPeriod = TimeSpan.FromMilliseconds(2500);
            ReportTimeout = TimeSpan.FromSeconds(30);
            AccessToken = token;
            Satellite = satelliteOptions;
        }

        private IDictionary<string, object> InitializeDefaultTags()
        {
            var attributes = new Dictionary<string, object>
            {
                [LightStepConstants.TracerPlatformKey] = LightStepConstants.TracerPlatformValue,
                [LightStepConstants.TracerPlatformVersionKey] = "0.1",
                [LightStepConstants.TracerVersionKey] = "0.1",
                [LightStepConstants.ComponentNameKey] = GetComponentName(),
                [LightStepConstants.HostnameKey] = GetHostName(),
                [LightStepConstants.CommandLineKey] = GetCommandLine()
                
            };
            return attributes;
        }

        private string GetComponentName()
        {
            string compName = "";
            #if NETSTANDARD1_3
            compName = Environment.GetEnvironmentVariable("LS_COMPONENT");
            #endif
            #if NETSTANDARD2_0 || NET45
            compName = Assembly.GetEntryAssembly().GetName().Name;
            #endif
            return compName;
        }
        

        private string GetHostName()
        {
            string hostname = "";
            #if NETSTANDARD1_3 
            hostname = Environment.GetEnvironmentVariable("LS_HOSTNAME");
            #endif
            #if NETSTANDARD2_0 || NET45
            hostname = Environment.MachineName;
            #endif
            return hostname;
        }

        private string GetCommandLine()
        {
            string commandLine = "";
            #if NETSTANDARD1_3
            commandLine = Environment.GetEnvironmentVariable("LS_COMMANDLINE");
            #endif
            #if NETSTANDARD2_0 || NET45
            commandLine = Environment.CommandLine;
            #endif
            return commandLine;
        }
    }
}
