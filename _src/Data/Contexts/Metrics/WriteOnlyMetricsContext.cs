using Data.Common;
using Domain.Options;
using Microsoft.Extensions.Options;

namespace Data.Contexts.Metrics
{
    public sealed class WriteOnlyMetricsContext : MetricsContext, IWriteOnlyMetricsContext
    {
        public WriteOnlyMetricsContext(IOptions<EnvironmentOptions> environmentOptions, IOptions<DataContextOptions> contextOptions)
            : base(environmentOptions.Value, contextOptions.Value, false)
        {

        }
    }
}
