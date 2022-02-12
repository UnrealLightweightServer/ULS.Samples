using Microsoft.AspNetCore.Mvc;
using System.Buffers.Binary;
using System.Net;
using System.Net.WebSockets;
using TicTacToeServer.MatchFramework;
using TicTacToeServer.Networking;

namespace TicTacToeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerCommController : ControllerBase
    {
        private readonly ILogger<PlayerCommController> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly MatchManager _matchManager;

        public PlayerCommController(ILogger<PlayerCommController> logger, IHostApplicationLifetime applicationLifetime,
            MatchManager matchManager)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _matchManager = matchManager;
        }

        [HttpGet("ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await PlayerCommLoop(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task PlayerCommLoop(HttpContext httpContext, WebSocket webSocket)
        {            
            CommChannel commChannel = new CommChannel(httpContext.Connection.RemoteIpAddress, httpContext.Connection.RemotePort);

            RemoteClient controller = new RemoteClient(_matchManager, commChannel);
            controller.OnDisconnectForced += () =>
            {
                Console.WriteLine("========= controller.OnDisconnectForced");
                commChannel.Stop();
            };

            Memory<byte> buffer = new byte[1024 * 4];

            commChannel.Start();

            var task = Task.Run(async () =>
            {
                while (commChannel.IsActive)
                {
                    try
                    {
                        _logger.LogTrace("PRE Send");
                        await commChannel.SendQueuedPackets(webSocket);
                        _logger.LogTrace("Post Send");
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            });

            while (commChannel.IsActive && 
                webSocket.State == WebSocketState.Open)
            {
                try
                {
                    _logger.LogTrace("PRE Receive");
                    var result = await webSocket.ReceiveAsync(buffer, _applicationLifetime.ApplicationStopping);
                    _logger.LogTrace("POST Receive");
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        int count = result.Count;
                        _logger.LogTrace($"Received {count} bytes");
                        if (count >= 0)
                        {
                            commChannel.ProcessReceivedPacket(buffer.Slice(0, count));
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    commChannel.Stop();
                }
                catch (WebSocketException)
                {
                    commChannel.Stop();
                }

                _logger.LogTrace("END Loop");
            }
            _logger.LogTrace("CLOSE");

            commChannel.Stop();
            Task.WaitAll(task);

            try
            {
                await webSocket.CloseAsync(webSocket.CloseStatus != null ? webSocket.CloseStatus.Value : WebSocketCloseStatus.Empty,
                    webSocket.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException)
            {
                // This happens when the socket is already closed, so it's fine.
            }

            //var player = controller.PossessedPlayer;
            controller?.OnDisconnected();
            _matchManager.PlayerLoggedOff(controller);
        }
    }
}