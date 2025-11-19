# Grafana Configuration for Acme Platform

This directory contains Grafana dashboards and configuration for monitoring the Acme Platform.

## Dashboard Overview

### 1. API Overview Dashboard (`api-overview.json`)

- **Purpose**: High-level API metrics and SLI/SLO monitoring
- **Key Metrics**:
  - Error rates (4xx/5xx responses)
  - Request rate and throughput
  - Response time percentiles (P50, P95, P99)
  - Service availability (SLI: 99.9%)
  - Top endpoints by request volume

### 2. Infrastructure Metrics Dashboard (`infrastructure-metrics.json`)

- **Purpose**: System and runtime performance monitoring
- **Key Metrics**:
  - CPU and memory usage
  - .NET GC performance
  - ASP.NET Core request metrics
  - Service health status
  - Memory allocation trends

## Setup Instructions

### Prerequisites

- Prometheus collecting metrics from your services
- Grafana instance (local or hosted)

### 1. Import Dashboards

#### Via Grafana UI:

1. Open Grafana web interface
2. Navigate to **Dashboards** > **Import**
3. Upload the JSON files from this directory
4. Configure the Prometheus data source

#### Via API:

```bash
# Import API Overview dashboard
curl -X POST \
  http://admin:admin@localhost:3000/api/dashboards/db \
  -H 'Content-Type: application/json' \
  -d @api-overview.json

# Import Infrastructure dashboard
curl -X POST \
  http://admin:admin@localhost:3000/api/dashboards/db \
  -H 'Content-Type: application/json' \
  -d @infrastructure-metrics.json
```

### 2. Configure Data Sources

#### Prometheus Data Source:

```yaml
# prometheus.yml - Add this scrape config
scrape_configs:
  - job_name: "acme-api-gateway"
    static_configs:
      - targets: ["localhost:5000"]
    metrics_path: "/metrics"
    scrape_interval: 15s

  - job_name: "acme-workflow-service"
    static_configs:
      - targets: ["localhost:5001"]
    metrics_path: "/metrics"
    scrape_interval: 15s

  - job_name: "acme-integration-hub"
    static_configs:
      - targets: ["localhost:5002"]
    metrics_path: "/metrics"
    scrape_interval: 15s
```

### 3. Enable Metrics in Applications

Add to your `appsettings.json`:

```json
{
  "Metrics": {
    "Enabled": true,
    "Port": 9090,
    "Path": "/metrics"
  }
}
```

Add to your `Program.cs`:

```csharp
// Enable Prometheus metrics
builder.Services.AddSingleton<IMetricsLogger, PrometheusMetricsLogger>();
builder.Services.AddPrometheusCounters();

var app = builder.Build();

// Map metrics endpoint
app.MapPrometheusScrapingEndpoint();
```

## Key SLIs and SLOs

### Service Level Indicators (SLIs):

- **Availability**: Percentage of successful requests (non-5xx)
- **Latency**: 95th percentile response time
- **Error Rate**: Percentage of failed requests

### Service Level Objectives (SLOs):

- **Availability**: ≥ 99.9% (≤ 43 minutes downtime/month)
- **Latency P95**: ≤ 200ms for API endpoints
- **Error Rate**: ≤ 0.1% for production traffic

### Error Budget:

- **Monthly Error Budget**: 0.1% of total requests
- **Alert Threshold**: 50% of error budget consumed
- **Emergency Response**: 90% of error budget consumed

## Alerting Rules

### High Priority Alerts:

- **Service Down**: `up{job=~"acme.*"} == 0`
- **High Error Rate**: `rate(http_requests_total{status=~"5.."}[5m]) > 0.01`
- **High Latency**: `histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 0.5`
- **Low Availability**: SLI drops below 99.9%

### Medium Priority Alerts:

- **High Memory Usage**: `process_working_set_bytes > 1GB`
- **High CPU Usage**: `rate(process_cpu_seconds_total[5m]) > 0.8`
- **GC Pressure**: `rate(dotnet_collection_count_total[5m]) > 5`

## Dashboard Variables

Both dashboards support these template variables:

- **Environment**: `dev`, `staging`, `prod`
- **Service**: Filter by specific service instance
- **Time Range**: Configurable time window for analysis

## Custom Metrics

### Business Metrics:

```csharp
// Example: Track workflow operations
_metrics.Counter("workflow_operations_total")
    .WithTag("operation_type", operationType)
    .WithTag("status", "success")
    .Increment();

// Example: Track integration hub requests
_metrics.Histogram("integration_requests_duration_seconds")
    .WithTag("provider", providerName)
    .Observe(duration.TotalSeconds);
```

### Security Metrics:

```csharp
// Track authentication events
_metrics.Counter("auth_attempts_total")
    .WithTag("result", "success|failure")
    .WithTag("method", "jwt|apikey")
    .Increment();

// Track rate limiting
_metrics.Counter("rate_limit_hits_total")
    .WithTag("endpoint", endpoint)
    .Increment();
```

## Performance Tuning

### Recommended Settings:

- **Scrape Interval**: 15-30 seconds for production
- **Retention**: 15 days for detailed metrics
- **Recording Rules**: Pre-calculate complex queries
- **Alert Evaluation**: Every 30 seconds

### Recording Rules Example:

```yaml
# prometheus-recording-rules.yml
groups:
  - name: acme.rules
    rules:
      - record: acme:http_request_rate
        expr: sum(rate(http_requests_total[5m])) by (job, endpoint)

      - record: acme:error_rate
        expr: |
          sum(rate(http_requests_total{status=~"5.."}[5m])) by (job) 
          / 
          sum(rate(http_requests_total[5m])) by (job)
```

## Troubleshooting

### Common Issues:

1. **No Data**: Check Prometheus scrape targets and service discovery
2. **Missing Metrics**: Verify metrics are exposed on `/metrics` endpoint
3. **High Cardinality**: Review metric labels and use recording rules
4. **Performance**: Optimize queries and use appropriate time ranges

### Debug Commands:

```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# Test metric queries
curl 'http://localhost:9090/api/v1/query?query=up'

# Validate dashboard JSON
cat api-overview.json | jq '.'
```

## Links and Resources

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Dashboard Best Practices](https://grafana.com/docs/grafana/latest/best-practices/)
- [SLI/SLO Implementation Guide](https://sre.google/workbook/implementing-slos/)
- [.NET Metrics with Prometheus](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/metrics)
