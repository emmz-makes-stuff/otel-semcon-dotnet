using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection;
using Nova.OpenTelemetry.SemanticConventions;
using Nova.OpenTelemetry.SemanticConventions.Incubating;
using Xunit;

namespace Nova.OpenTelemetry.SemanticConventions.Tests;

public sealed class MetricTests : IDisposable
{
    private readonly Meter _meter = new($"{nameof(MetricTests)}.{Guid.NewGuid():N}");

    public void Dispose() => _meter.Dispose();

    [Fact]
    public void MetricMetadataMatchesRegistry()
    {
        Assert.Equal("http.server.request.duration", HttpMetrics.ServerRequestDuration.Name);
        Assert.Equal("s", HttpMetrics.ServerRequestDuration.Unit);
        Assert.Equal("Duration of HTTP server requests.", HttpMetrics.ServerRequestDuration.Description);
    }

    [Fact]
    public void FactoryCreatesInstrumentWithMetadata()
    {
        Histogram<double> histogram = HttpMetrics.CreateServerRequestDuration(_meter);

        Assert.Same(_meter, histogram.Meter);
        Assert.Equal(HttpMetrics.ServerRequestDuration.Name, histogram.Name);
        Assert.Equal(HttpMetrics.ServerRequestDuration.Unit, histogram.Unit);
        Assert.Equal(HttpMetrics.ServerRequestDuration.Description, histogram.Description);
    }

    [Fact]
    public void RecordedMeasurementsReachListeners()
    {
        var recorded = new List<(string Name, double Value, string? Method)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == _meter) l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
        {
            string? method = null;
            foreach (var tag in tags)
            {
                if (tag.Key == HttpAttributes.HttpRequestMethod) method = tag.Value as string;
            }
            recorded.Add((instrument.Name, value, method));
        });
        listener.Start();

        HttpMetrics.CreateServerRequestDuration(_meter)
            .Record(0.25, new KeyValuePair<string, object?>(HttpAttributes.HttpRequestMethod, HttpAttributes.HttpRequestMethodValues.Get));

        Assert.Equal([("http.server.request.duration", 0.25, "GET")], recorded);
    }

    [Fact]
    public void ObservableFactoryReportsObservedValues()
    {
        var observed = new List<long>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == _meter) l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => observed.Add(value));
        listener.Start();

        ObservableUpDownCounter<long> counter = ProcessIncubatingMetrics.CreateMemoryUsage(
            _meter, () => [new Measurement<long>(1024)]);
        listener.RecordObservableInstruments();

        Assert.Equal("process.memory.usage", counter.Name);
        Assert.Equal([1024L], observed);
    }

    [Fact]
    public void InstrumentKindAndValueTypeFollowRegistry()
    {
        Assert.IsType<Histogram<long>>(HttpIncubatingMetrics.CreateServerRequestBodySize(_meter));
        Assert.IsType<UpDownCounter<long>>(HttpIncubatingMetrics.CreateServerActiveRequests(_meter));
        Assert.IsType<Counter<double>>(ProcessIncubatingMetrics.CreateCpuTime(_meter));
        Assert.IsType<Gauge<long>>(SystemIncubatingMetrics.CreateCpuFrequency(_meter));
    }

    [Fact]
    public void IncubatingStableMetricPointsToStableFactory()
    {
        var obsolete = typeof(HttpIncubatingMetrics)
            .GetMethod(nameof(HttpIncubatingMetrics.CreateServerRequestDuration), [typeof(Meter)])!
            .GetCustomAttribute<ObsoleteAttribute>();

        Assert.Equal(
            "Use Nova.OpenTelemetry.SemanticConventions.HttpMetrics.CreateServerRequestDuration instead.",
            obsolete?.Message);
    }
}
