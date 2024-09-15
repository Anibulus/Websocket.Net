namespace DTOs;

public class WebSocketMessage
{
    public WebSocketMessage() { }

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
        using (var ms = new MemoryStream(buffer, 0, length))
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
