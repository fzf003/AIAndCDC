using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CdcHttpServer.Models;
using CdcHttpServer.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AIAndCDC.Tests;

public class CdcHttpServerApiTests
{
    [Fact]
    public async Task PostCdc_ShouldStoreEvent_AndExposeQueryEndpoints()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var payload = """
        {
          "source": { "schema": "dbo", "table": "Product" },
          "op": "c",
          "ts_ms": 1713777777000,
          "before": null,
          "after": {
            "Sku": "TEST-HTTP-001",
            "Price": 99.99,
            "ProductName": "HTTP 测试商品"
          }
        }
        """;

        using var postResponse = await client.PostAsync(
            "/cdc",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var postJson = await postResponse.Content.ReadFromJsonAsync<JsonElement>();
        var eventId = postJson.GetProperty("eventId").GetString();

        Assert.False(string.IsNullOrWhiteSpace(eventId));

        var events = await client.GetFromJsonAsync<PagedResult<CdcEvent>>("/api/events?page=1&size=10");
        Assert.NotNull(events);
        Assert.Equal(1, events.Total);
        Assert.Single(events.Items);
        Assert.Equal("dbo", events.Items[0].SourceSchema);
        Assert.Equal("Product", events.Items[0].SourceTable);
        Assert.Equal("c", events.Items[0].Op);

        var singleEvent = await client.GetFromJsonAsync<CdcEvent>($"/api/events/{eventId}");
        Assert.NotNull(singleEvent);
        Assert.Equal(eventId, singleEvent.Id);

        var stats = await client.GetFromJsonAsync<EventStats>("/api/stats");
        Assert.NotNull(stats);
        Assert.Equal(1, stats.TotalEvents);
        Assert.Equal(1, stats.ByOp["c"]);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnProblemDetails_WhenPagingIsInvalid()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/events?page=0&size=10");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("分页参数不合法。", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Health_ShouldReturnStructuredStatus()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var health = await client.GetFromJsonAsync<JsonElement>("/health");

        Assert.Equal("ok", health.GetProperty("status").GetString());
        Assert.Equal("externalized", health.GetProperty("checks").GetProperty("redisConsumer").GetString());
        Assert.True(health.GetProperty("configuration").GetProperty("eventStoreCapacity").GetInt32() > 0);
    }
}
