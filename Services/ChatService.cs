
using System.Net.WebSockets;
using DTOs;

namespace Services;

public class ChatService
{
    private readonly List<System.Net.WebSockets.WebSocket> _sockets = new();

    public async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket)
    {
        _sockets.Add(webSocket);

        // This variable will save the data that will receive the websocket
        var buffer = new byte[1024 * 2];
        bool closeIntention = false;

        while(webSocket.State == WebSocketState.Open && !closeIntention)
        {
            // Retrieve message
            var message = await RetrieveSocketMessage(webSocket, buffer);
            var memoryStream = message.Item1;
            var result = message.Item2;
            closeIntention = message.Item3;
          
            // Process message
            var filteredContent = FilterWords(memoryStream, result);

            // Send message to sockets
            buffer = filteredContent.ToBytes();
           await this.SendSocketMessage(buffer[..result.Count]);
        }
        _sockets.Remove(webSocket);
    }

    private WebSocketMessage FilterWords(MemoryStream ms, WebSocketReceiveResult result)
    {
        var content = new WebSocketMessage(ms.ToArray(), result.Count);

        if(new List<string>{"puto", "wey"}.Contains(content.Message))
            content.Message = "****";
        
        return content;
    }


    /// <summary>
    /// This method retrieve data fron websocket connection, also rewrite buffer variable
    /// </summary>
    /// <param name="webSocket"></param>
    /// <returns></returns>
    private async Task<(MemoryStream, WebSocketReceiveResult, bool)> RetrieveSocketMessage(System.Net.WebSockets.WebSocket webSocket, byte[] buffer)
    {
        var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;
        bool closeIntention = false;
        do 
        {
            result = await webSocket.ReceiveAsync(buffer : new ArraySegment<byte>(buffer), cancellationToken: default);
            memoryStream.Write(buffer, 0, result.Count);
        } while(!result.EndOfMessage);

        
        if(result.MessageType == WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken: default);
            closeIntention = true;
        }
        return(memoryStream, result, closeIntention);
    }

    /// <summary>
    /// Send data to any connected client
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private async Task SendSocketMessage(byte[] buffer)
    {
        foreach(var s in _sockets)
        {
            await s.SendAsync(buffer: buffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: default);
        }

    }

}