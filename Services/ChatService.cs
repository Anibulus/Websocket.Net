
using System.Net.WebSockets;

namespace Services;

public class ChatService
{
    private readonly List<System.Net.WebSockets.WebSocket> _sockets = new();

    public async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket)
    {
        _sockets.Add(webSocket);

        // This variable will save the data that will receive the websocket
        var buffer = new byte[1024 * 2];

        while(webSocket.State == WebSocketState.Open)
        {
            //This method retrieve data fron websocket connection, also rewrite buffer variable
            var result = await webSocket.ReceiveAsync(buffer : new ArraySegment<byte>(buffer), cancellationToken: default);

            
            if(result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken: default);
                break;
            }

            var content = new WebSocketMessage(buffer, result.Count);
            if(new List<string>{"puto", "wey"}.Contains(content.Message))
                content.Message = "****";
            buffer = content.ToBytes();

            // Send data to any connected client
            foreach(var s in _sockets)
            {
                await s.SendAsync(buffer: buffer[..result.Count], WebSocketMessageType.Text, endOfMessage: true, cancellationToken: default);
            }

        }
        _sockets.Remove(webSocket);
    }


    public class WebSocketMessage
    {
        public WebSocketMessage()
        {

        }
        public WebSocketMessage(byte[] buffer, int length)
        {
            var result = FromBytes(buffer, length);
            this.Message = result.Message;
            this.User = result.User;
            this.Admin = result.Admin;
        }
        public string User { get; set; }
        public string Message { get; set; }
        public bool Admin { get; set; }

        public WebSocketMessage FromBytes(byte[] buffer, int length)
        {
            using(var ms = new MemoryStream(buffer, 0,length))
            {
                
                var json = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
                Console.WriteLine(json);
                return System.Text.Json.JsonSerializer.Deserialize<WebSocketMessage>(json) ?? new();
            }
        }

        public byte[] ToBytes()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }


}