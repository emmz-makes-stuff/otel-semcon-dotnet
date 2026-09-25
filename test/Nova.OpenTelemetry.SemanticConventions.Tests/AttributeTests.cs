using System;
using System.Reflection;
using Nova.OpenTelemetry.SemanticConventions;
using Nova.OpenTelemetry.SemanticConventions.Incubating;
using Xunit;

namespace Nova.OpenTelemetry.SemanticConventions.Tests;

public class AttributeTests
{
    [Fact]
    public void StableAttributeKeysMatchRegistryNames()
    {
        Assert.Equal("http.request.method", HttpAttributes.HttpRequestMethod);
        Assert.Equal("http.response.status_code", HttpAttributes.HttpResponseStatusCode);
        Assert.Equal("url.full", UrlAttributes.UrlFull);
    }

    [Fact]
    public void StableEnumValuesExcludeUnstableMembers()
    {
        Assert.Equal("GET", HttpAttributes.HttpRequestMethodValues.Get);
        Assert.Equal("_OTHER", HttpAttributes.HttpRequestMethodValues.Other);
        Assert.Null(typeof(HttpAttributes.HttpRequestMethodValues).GetField("Query"));
    }

    [Fact]
    public void TemplateAttributesAppendTheKey()
    {
        Assert.Equal("http.request.header.content-type", HttpAttributes.HttpRequestHeader("content-type"));
    }

    [Fact]
    public void IncubatingKeepsStableDefinitionsAsObsoletePointingToStablePackage()
    {
        Assert.Equal(HttpAttributes.HttpRequestMethod, HttpIncubatingAttributes.HttpRequestMethod);

        var obsolete = typeof(HttpIncubatingAttributes).GetField(nameof(HttpIncubatingAttributes.HttpRequestMethod))!
            .GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(obsolete);
        Assert.Contains("Nova.OpenTelemetry.SemanticConventions.HttpAttributes.HttpRequestMethod", obsolete.Message);
    }

    [Fact]
    public void IncubatingLeavesUnstableDefinitionsUnmarked()
    {
        Assert.Equal("QUERY", HttpIncubatingAttributes.HttpRequestMethodValues.Query);

        var query = typeof(HttpIncubatingAttributes.HttpRequestMethodValues).GetField("Query")!;
        Assert.False(query.IsDefined(typeof(ObsoleteAttribute)));
    }

    [Fact]
    public void IncubatingMarksDeprecatedDefinitionsObsoleteWithRegistryNote()
    {
        var obsolete = typeof(DbIncubatingAttributes).GetField(nameof(DbIncubatingAttributes.DbSystem))!
            .GetCustomAttribute<ObsoleteAttribute>();
        Assert.Equal("Replaced by `db.system.name`.", obsolete?.Message);
    }
}
