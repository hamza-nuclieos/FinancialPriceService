using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FinancialPriceService.Services
{
    public class BinancePriceService
    {
        private readonly ClientWebSocket _clientWebSocket = new();
        private readonly List<WebSocket> _subscribers = new();

        // Store the latest price update
        private volatile string _latestPrice;

        public BinancePriceService()
        {
            Task.Run(() => ConnectToBinance());
        }

        public void Subscribe(WebSocket webSocket)
        {
            _subscribers.Add(webSocket);
        }

        public string GetLatestPrice()
        {
            return _latestPrice;
        }

        private async Task ConnectToBinance()
        {
            try
            {
                await _clientWebSocket.ConnectAsync(new Uri("wss://stream.binance.com:443/ws/btcusdt@aggTrade"), CancellationToken.None);

                var buffer = new byte[1024 * 4];
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Parse the price from the Binance message
                    var json = JObject.Parse(message);
                    _latestPrice = json["p"]?.ToString();  // "p" is the price field

                    await BroadcastPriceUpdate(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to Binance WebSocket: {ex.Message}");
            }
        }

        private async Task BroadcastPriceUpdate(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var subscriber in _subscribers)
            {
                if (subscriber.State == WebSocketState.Open)
                {
                    await subscriber.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
