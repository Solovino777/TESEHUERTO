using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");

        try
        {
            listener.Start();
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
            return;
        }

        Console.WriteLine("Esperando a que un cliente se conecte...");

        while (true)
        {
            var context = await listener.GetContextAsync();
            context.Response.AddHeader("Access-Control-Allow-Origin", "https://solovino777.github.io/TESE/"); // Configuración CORS
            await ProcessRequest(context);
        }
    }

    static async Task ProcessRequest(HttpListenerContext context)
    {
        if (context.Request.IsWebSocketRequest)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            var webSocket = wsContext.WebSocket;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var temperaturaCelsius = SimularLecturaTemperatura();
                    var humedad = SimularLecturaHumedad();

                    var temperaturaStr = $"{temperaturaCelsius} °C";
                    var humedadStr = $"{humedad}%";

                    var response = $"{temperaturaStr}\n{humedadStr}";
                    var buffer = Encoding.UTF8.GetBytes(response);
                    var segment = new ArraySegment<byte>(buffer);

                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

                    Console.WriteLine($"Temperatura y humedad enviadas: {temperaturaStr}, {humedadStr}");

                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the server", CancellationToken.None);
            }
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
            context.Response.Close();
        }
    }

    static int SimularLecturaTemperatura()
    {
        return new Random().Next(-20, 50);
    }

    static int SimularLecturaHumedad()
    {
        return new Random().Next(0, 100);
    }
}
