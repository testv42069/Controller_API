using Microsoft.AspNetCore.Mvc;
using CoreLib.Models;
using System.Collections.Concurrent;

namespace ControllerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        // Stores connected clients in memory
        private static readonly ConcurrentDictionary<string, ClientInfo> Clients = new();

        // POST api/client/register
        [HttpPost("register")]
        public IActionResult RegisterClient([FromBody] ClientInfo client)
        {
            if (client == null || string.IsNullOrWhiteSpace(client.HWID))
                return BadRequest("Invalid client info");

            Clients[client.HWID] = client;
            return Ok(new { message = "Client registered", client });
        }

        // GET api/client/list
        [HttpGet("list")]
        public IActionResult GetClients()
        {
            return Ok(Clients.Values.ToList());
        }

        // POST api/client/send-command
        [HttpPost("send-command")]
        public IActionResult SendCommand([FromQuery] string hwid, [FromQuery] string command)
        {
            if (string.IsNullOrWhiteSpace(hwid) || string.IsNullOrWhiteSpace(command))
                return BadRequest("HWID and command are required");

            if (Clients.TryGetValue(hwid, out var client))
            {
                // In real-world, you'd push the command to the client (SignalR, polling, etc.)
                client.LastCommand = command;
                Clients[hwid] = client;
                return Ok(new { message = $"Command '{command}' sent to {client.PCName}" });
            }

            return NotFound("Client not found");
        }

        // GET api/client/get-command
        [HttpGet("get-command")]
        public IActionResult GetCommand([FromQuery] string hwid)
        {
            if (Clients.TryGetValue(hwid, out var client))
            {
                var cmd = client.LastCommand;
                client.LastCommand = null; // clear after reading
                Clients[hwid] = client;
                return Ok(new { command = cmd });
            }

            return NotFound("Client not found");
        }
    }
}
