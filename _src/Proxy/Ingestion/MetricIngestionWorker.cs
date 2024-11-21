using Microsoft.Extensions.Hosting;

namespace Proxy.Ingestion
{
    internal class MetricIngestionWorker : BackgroundService
    {
        private readonly MetricIngestionService _metricIngestionService;

        public MetricIngestionWorker(MetricIngestionService metricIngestionService)
        {
            _metricIngestionService = metricIngestionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _metricIngestionService.Start(stoppingToken);
        }
    }
}
