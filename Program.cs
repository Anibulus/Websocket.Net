using Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddSingleton<ChatService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseWebSockets();
app.MapGet("/", async (HttpContext context, ChatService chatService) => 
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await chatService.HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.Redirect("/Chat.html");
    }
});

app.MapGet("/chat", async  context => {
    context.Response.Redirect("/Chat.html");
});

app.Run();