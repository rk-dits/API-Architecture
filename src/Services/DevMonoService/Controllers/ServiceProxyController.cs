using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevMonoService.Controllers;

[ApiController]
[Route("api/proxy")]
[Tags("Service Proxy")]
public class ServiceProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ServiceProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("identity/{**path}")]
    [HttpPost("identity/{**path}")]
    [HttpPut("identity/{**path}")]
    [HttpDelete("identity/{**path}")]
    [HttpPatch("identity/{**path}")]
    public async Task<IActionResult> ProxyToIdentityService(string path)
    {
        var identityServiceUrl = _configuration["ServiceEndpoints:IdentityService"];
        return await ProxyRequest("IdentityService", identityServiceUrl, path);
    }

    [HttpGet("integration/{**path}")]
    [HttpPost("integration/{**path}")]
    [HttpPut("integration/{**path}")]
    [HttpDelete("integration/{**path}")]
    [HttpPatch("integration/{**path}")]
    public async Task<IActionResult> ProxyToIntegrationHub(string path)
    {
        var integrationServiceUrl = _configuration["ServiceEndpoints:IntegrationHub"];
        return await ProxyRequest("IntegrationHub", integrationServiceUrl, path);
    }

    [HttpGet("workflow/{**path}")]
    [HttpPost("workflow/{**path}")]
    [HttpPut("workflow/{**path}")]
    [HttpDelete("workflow/{**path}")]
    [HttpPatch("workflow/{**path}")]
    public async Task<IActionResult> ProxyToCoreWorkflowService(string path)
    {
        var workflowServiceUrl = _configuration["ServiceEndpoints:CoreWorkflowService"];
        return await ProxyRequest("CoreWorkflowService", workflowServiceUrl, path);
    }

    private async Task<IActionResult> ProxyRequest(string serviceName, string? serviceUrl, string path)
    {
        try
        {
            if (string.IsNullOrEmpty(serviceUrl))
            {
                return BadRequest(new { error = $"{serviceName} endpoint not configured" });
            }

            var client = _httpClientFactory.CreateClient();

            // Build the target URL
            var targetUrl = $"{serviceUrl}/api/{path}";
            if (Request.QueryString.HasValue)
            {
                targetUrl += Request.QueryString.Value;
            }

            // Create the proxy request
            var proxyRequest = new HttpRequestMessage(
                new HttpMethod(Request.Method),
                targetUrl);

            // Copy headers
            foreach (var header in Request.Headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Copy body for POST/PUT/PATCH requests
            if (Request.ContentLength > 0)
            {
                var bodyStream = new MemoryStream();
                await Request.Body.CopyToAsync(bodyStream);
                bodyStream.Position = 0;
                proxyRequest.Content = new StreamContent(bodyStream);

                if (Request.ContentType != null)
                {
                    proxyRequest.Content.Headers.TryAddWithoutValidation("Content-Type", Request.ContentType);
                }
            }

            // Send the request
            var response = await client.SendAsync(proxyRequest);

            // Copy response
            var responseContent = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new
            {
                error = $"{serviceName} is not available",
                message = ex.Message,
                suggestion = $"Make sure {serviceName} is running on {serviceUrl}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Proxy error",
                message = ex.Message,
                service = serviceName
            });
        }
    }
}