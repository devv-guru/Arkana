using Data.Common;

namespace Data.Contexts.Metrics
{
    public sealed class WriteOnlyMetricsContext : MetricsContext, IWriteOnlyMetricsContext
    {
        public WriteOnlyMetricsContext(DataContextOptions options)
            : base(options, false)
        {

        }
    }
}
