using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using WebApplication1.Dtos;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/purchases")]
[Produces(MediaTypeNames.Application.Json)]
public class PurchasesController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<PurchasesController> _log;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public PurchasesController(
        IHttpClientFactory http,
        IConfiguration cfg,
        ILogger<PurchasesController> log)
    {
        _http = http;
        _cfg = cfg;
        _log = log;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<PurchaseResponse>> Purchase([FromBody] PurchaseRequest request, CancellationToken ct)
    {
  
        if (request is null) return BadRequest("Body required.");
        if (request.Amount <= 0) return BadRequest("Amount must be > 0.");
        if (string.IsNullOrWhiteSpace(request.Type)) return BadRequest("Type required.");
        if (string.IsNullOrWhiteSpace(request.Reference)) return BadRequest("Reference required.");
        if (request.Card is null || string.IsNullOrWhiteSpace(request.Card.Id))
            return BadRequest("Card.id required.");

        var token = _cfg["PurchaseApi:Token"];
        if (string.IsNullOrWhiteSpace(token))
            return StatusCode(500, "Purchase API token not configured.");

 
        var shaped = new
        {
            type = request.Type,
            extendedType = request.ExtendedType,
            amount = request.Amount,
            created = (request.Created == default ? DateTimeOffset.UtcNow : request.Created)
                      .ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            card = request.Card,
            reference = request.Reference,
            narration = request.Narration,
            additionalData = request.AdditionalData
        };
        var payloadJson = JsonSerializer.Serialize(shaped, JsonOpts);

        var client = _http.CreateClient("PurchaseApi"); 

        var candidates = new Uri[]
        {
            new Uri("api/transaction", UriKind.Relative),            
            new Uri("/interview/api/transaction", UriKind.Relative)  
        };

        foreach (var rel in candidates.Distinct())
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post, rel);
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            msg.Content = new StringContent(payloadJson, Encoding.UTF8, MediaTypeNames.Application.Json);

            var finalUrl = new Uri(client.BaseAddress!, rel);
            _log.LogInformation("Calling payment endpoint: {Url}", finalUrl);

            HttpResponseMessage resp;
            string body;
            try
            {
                resp = await client.SendAsync(msg, ct);
                body = await resp.Content.ReadAsStringAsync(ct);
            }
            catch (OperationCanceledException oce)
            {
                _log.LogError(oce, "Payment request timed out/cancelled");
                return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "Timeout or cancelled", message = oce.Message });
            }
            catch (HttpRequestException hre)
            {
                _log.LogError(hre, "HTTP failure calling payment gateway");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "HTTP request failed", message = hre.Message, inner = hre.InnerException?.Message });
            }

            if (resp.IsSuccessStatusCode)
            {
                var model = JsonSerializer.Deserialize<PurchaseResponse>(body, JsonOpts);
                return model is null
                    ? StatusCode(StatusCodes.Status502BadGateway, "Invalid response from payment gateway.")
                    : Ok(model);
            }

          
            if ((int)resp.StatusCode == 404 && body.Contains("/docs/index.html", StringComparison.OrdinalIgnoreCase))
                continue;

     
            return StatusCode((int)resp.StatusCode, body);
        }

        return NotFound(new
        {
            error = "Not found at both endpoints",
            tried = candidates.Select(u => new Uri(client.BaseAddress!, u).ToString()).ToArray()
        });
    }
}
