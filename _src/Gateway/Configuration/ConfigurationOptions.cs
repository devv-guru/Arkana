namespace Gateway.Configuration
{
    public class ConfigurationOptions
    {
        public const string? SectionName = "ConfigurationOptions";
        public Service[]? Services { get; set; }
    }

    public class Service
    {
        public string? Name { get; set; }
        public string? TargetHost { get; set; }
        public Healthcheck? HealthCheck { get; set; }
        public Httprequest? HttpRequest { get; set; }
        public IReadOnlyList<IReadOnlyDictionary<string, string>>? Transforms { get; set; }
        public Destination[]? Destinations { get; set; }
    }

    public class Healthcheck
    {
        public string? Enabled { get; set; }
        public string? Interval { get; set; }
        public string? Timeout { get; set; }
        public string? Threshold { get; set; }
        public string? Path { get; set; }
        public string? Query { get; set; }
    }

    public class Httprequest
    {
        public string? Version { get; set; }
        public string? VersionPolicy { get; set; }
        public bool AllowResponseBuffering { get; set; }
        public string? ActivityTimeout { get; set; }
    }

    public class Destination
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
    }

}
