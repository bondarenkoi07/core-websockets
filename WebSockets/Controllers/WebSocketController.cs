using System;
using System.Collections.Generic;

using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebSockets.Models;

namespace WebSockets.Controllers
{
    

    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;

        private static List<MessageModel> _messages = new ();

        private static List<WebSocket> _clients = new();

        private static int _counter = 0;
        
        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
            _logger.Log(LogLevel.Information, "New controller created");
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
              string remoteIpAddress;
              try
              {
                  remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString() + ":" + HttpContext.Connection.RemotePort.ToString();
              }
              catch (NullReferenceException)
              {
                  remoteIpAddress = "Unknown";
              }
              using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
              _logger.Log(LogLevel.Information, "WebSocket connection established");
              _clients.Add(webSocket);
              await Echo(webSocket, remoteIpAddress);
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }
        
        private async Task Echo(WebSocket webSocket, string sender)
        {
            var data = JsonConvert.SerializeObject(_messages);

            var jsonBytes =  Encoding.UTF8.GetBytes($"{data}");
            
            await webSocket.SendAsync(new ArraySegment<byte>(jsonBytes, 0, jsonBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var text = Encoding.UTF8.GetString(buffer);
                _messages.Add(new MessageModel
                    {
                        Id = _counter++,
                        Ip = sender,
                        Message = text
                    }
                );
                var serverMsg = Encoding.UTF8.GetBytes($"{text}");

                _logger.Log(LogLevel.Information,$"client's count {_clients.Count}");
                foreach (var client in _clients)
                {
                    if (client != webSocket)
                    {
                        await client.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }
                _logger.Log(LogLevel.Information, "Messages sent to Client");
                
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
                
            } 
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _clients.Remove(webSocket);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }
    }
}