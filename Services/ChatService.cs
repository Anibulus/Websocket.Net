
using System.Net.WebSockets;

namespace Services;

public class ChatService
{
    private readonly List<System.Net.WebSockets.WebSocket> _sockets = new();

    public async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket)
    {
        _sockets.Add(webSocket);

        var buffer = new byte[1024 * 2];

        while(webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer : new ArraySegment<byte>(buffer), cancellationToken: default);
             //para que es cancelation token y por que default
            if(result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken: default);
                break;
            }

            foreach(var s in _sockets)
            {
                await s.SendAsync(buffer: buffer[..result.Count], WebSocketMessageType.Text, endOfMessage: true, cancellationToken: default);
            }

        }
        _sockets.Remove(webSocket);
    }

}