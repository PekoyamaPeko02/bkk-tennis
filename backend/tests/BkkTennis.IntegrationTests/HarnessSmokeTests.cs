using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

[Collection(ApiCollection.Name)]
public class HarnessSmokeTests(ApiFactory api)
{
    [Fact]
    public async Task Health_is_ok()
    {
        var res = await api.Anonymous().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Otp_login_yields_a_token_that_works_on_me()
    {
        var user = await api.LoginAsync(displayName: "Harness");

        var me = await user.Http.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");
        Assert.Equal(user.Id, me.GetProperty("id").GetGuid());
        Assert.Equal("player", me.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Me_requires_a_token()
    {
        var res = await api.Anonymous().GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Wrong_otp_is_rejected()
    {
        var client = api.Anonymous();
        var phone = ApiFactory.NewPhone();
        (await client.PostAsJsonAsync("/api/v1/auth/otp/request", new { phone })).EnsureSuccessStatusCode();

        var verify = await client.PostAsJsonAsync("/api/v1/auth/otp/verify", new { phone, code = "000000x" });
        Assert.Equal(HttpStatusCode.BadRequest, verify.StatusCode);
        Assert.Equal("otp_invalid", (await verify.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Seeded_venues_are_served_from_the_test_database()
    {
        var res = await api.Anonymous().GetAsync("/api/v1/venues");
        res.EnsureSuccessStatusCode();
        Assert.NotEqual("[]", (await res.Content.ReadAsStringAsync()).Trim());
    }

    [Fact]
    public void Api_uses_the_fake_clock_and_no_background_expiry_job()
    {
        Assert.Same(api.Clock, api.Services.GetRequiredService<TimeProvider>());
        Assert.DoesNotContain(api.Services.GetServices<Microsoft.Extensions.Hosting.IHostedService>(),
            s => s.GetType() == typeof(BkkTennis.Api.Jobs.HoldExpiryJob));
    }
}
