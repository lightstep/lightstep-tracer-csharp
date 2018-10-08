# Tracing With Aspects

This is a small sample application demonstrating how to use an Aspect framework (such as PostSharp) to apply Tracing to methods.

## Installation and Setup

Add an `App.config` such as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.2" />
    </startup>
    <appSettings>
        <add key="lsKey" value="Insert Your LightStep Key Here"/>
    </appSettings>
</configuration>
```

**Important**: This sample will only build on Windows platforms, as PostSharp does not support all CoreCLR targets for compilation.