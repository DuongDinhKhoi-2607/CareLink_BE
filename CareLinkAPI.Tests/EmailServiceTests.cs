using System.Net;
using System.Text.Json;
using CareLinkAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CareLinkAPI.Tests;

public class EmailServiceTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };
        }
    }

    private class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public MockHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    [Fact]
    public async Task SendPasswordResetOtpAsync_WithoutConfig_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var service = new EmailService(config, NullLogger<EmailService>.Instance);

        // Should complete smoothly via console fallback without any unhandled exception
        await service.SendPasswordResetOtpAsync("test@example.com", "123456");
    }

    [Fact]
    public async Task SendStudentVerificationOtpAsync_WithoutConfig_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var service = new EmailService(config, NullLogger<EmailService>.Instance);

        await service.SendStudentVerificationOtpAsync("student@fpt.edu.vn", "654321");
    }

    [Fact]
    public async Task SendPasswordResetOtpAsync_WithResendApiKey_SendsCorrectRequest()
    {
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(new { id = "resend_msg_123" }));
        var httpClient = new HttpClient(handler);
        var factory = new MockHttpClientFactory(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiKey"] = "re_test_key_123456",
                ["Resend:SenderEmail"] = "onboarding@resend.dev",
                ["Resend:SenderName"] = "CareLink Medical Care"
            })
            .Build();

        var service = new EmailService(config, NullLogger<EmailService>.Instance, factory);

        await service.SendPasswordResetOtpAsync("target@example.com", "999888");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://api.resend.com/emails", handler.LastRequest!.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("re_test_key_123456", handler.LastRequest.Headers.Authorization?.Parameter);

        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains("target@example.com", handler.LastRequestBody);
        Assert.Contains("999888", handler.LastRequestBody);
        Assert.Contains("onboarding@resend.dev", handler.LastRequestBody);
    }

    [Fact]
    public async Task SendPasswordResetOtpAsync_WhenResendFails_GracefullyHandlesWithoutThrowing()
    {
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.Forbidden,
            "{\"message\":\"You can only send testing emails to your own email address.\"}");
        var httpClient = new HttpClient(handler);
        var factory = new MockHttpClientFactory(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiKey"] = "re_test_key_123456"
            })
            .Build();

        var service = new EmailService(config, NullLogger<EmailService>.Instance, factory);

        // Even if Resend responds with 403, method shouldn't crash client caller
        await service.SendPasswordResetOtpAsync("forbidden@example.com", "111222");

        Assert.NotNull(handler.LastRequest);
    }
}
