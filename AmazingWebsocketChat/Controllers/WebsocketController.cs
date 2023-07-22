using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AmazingWebsocketChat;
using AmazingWebsocketChat.Database;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;


[ApiController]
[Route("[controller]")]
public class WebSocketController : ControllerBase
{
    IDatabaseClient dbClient;
    public WebSocketController()
    {
        dbClient = SQLDatabaseClient.getInstance();
    }

    [Route("websocketget")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            Task.Run(() => writeChatsToSocket(webSocket));
            await ReadChatsFromWebsocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task ReadChatsFromWebsocket(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var memStream = new MemoryStream();
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        memStream.Write(buffer, 0, receiveResult.Count);
        while (!receiveResult.CloseStatus.HasValue)
        {
            if (receiveResult.EndOfMessage && !receiveResult.CloseStatus.HasValue)
            {
                memStream.Position = 0;
                StreamReader reader = new StreamReader(memStream);
                string jsonString = reader.ReadToEnd();
                memStream = new MemoryStream();
                ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(jsonString);
                await this.dbClient.insertChat(chatResponse);
            }

            receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
            memStream.Write(buffer, 0, receiveResult.Count);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private async Task writeChatsToSocket(WebSocket webSocket)
    {
        int prevVersion = 0, currVersion = 0;
        while (!webSocket.CloseStatus.HasValue)
        {
            List<ChatResponse> response = await this.dbClient.getChats();
            string allChats = JsonSerializer.Serialize(response);
            byte[] byteArray = Encoding.ASCII.GetBytes(allChats);
            if (response.Count > 0)
            {
                currVersion = response[0].SentTime;
            }

            if (currVersion > prevVersion)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(byteArray, 0, byteArray.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            prevVersion = currVersion;
            Thread.Sleep(1000);
        }
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Closing web socket",
            CancellationToken.None
        );
    }

}