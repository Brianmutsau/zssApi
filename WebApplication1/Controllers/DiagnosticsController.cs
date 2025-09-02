using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        public DiagnosticsController(IConfiguration cfg) => _cfg = cfg;

        [HttpGet("config")]
        public IActionResult Config()
        {
            var baseUrl = _cfg["PurchaseApi:BaseUrl"];
            var path = _cfg["PurchaseApi:TransactionPath"] ?? "/api/transaction";
            var combined = $"{(baseUrl ?? "").TrimEnd('/')}/{path.TrimStart('/')}";
            return Ok(new
            {
                baseUrl,
                path,
                combined,
                tokenLen = (_cfg["PurchaseApi:Token"]?.Length ?? 0)
            });
        }

        [HttpGet("connect")]
        public async Task<IActionResult> Connect()
        {
            try
            {
                var host = "secure.v.co.zw";
                var ips = await Dns.GetHostAddressesAsync(host);
                var ip4s = ips.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Select(a => a.ToString()).ToArray();
                if (ip4s.Length == 0) return StatusCode(500, "No IPv4 addresses found for secure.v.co.zw.");

                using var tcp = new TcpClient(AddressFamily.InterNetwork);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                await tcp.ConnectAsync(ip4s[0], 443, cts.Token);
                return Ok(new { resolvedIPv4 = ip4s, connected = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { connected = false, error = ex.Message });
            }
        }
    }
}
