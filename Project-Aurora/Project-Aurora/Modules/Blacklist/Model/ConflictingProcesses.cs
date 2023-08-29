using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aurora.Modules.Blacklist.Model;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ConflictingProcesses
{
    public List<ShutdownProcess>? ShutdownAurora { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ShutdownProcess
{
    public string ProcessName { get; set; } = "unset";
    public string? Reason { get; set; }
}